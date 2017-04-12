/* Copyright (C) 2002-2005 RealVNC Ltd.  All Rights Reserved.
 *
 * This is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This software is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this software; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307,
 * USA.
 */

 //#include <windows.h>
#include <winsock2.h>

#include "CConn.h"
#include "ConnectionThread.h"
#include "DesktopWindow.h"


#include <rfb/encodings.h>
#include <rfb/secTypes.h>
#include <rfb/CSecurityNone.h>
#include <rfb/CSecurityVncAuth.h>
#include <rfb/CMsgWriter.h>
#include <rfb/Configuration.h>
#include <rfb/LogWriter.h>
#include <rfb_win32/AboutDialog.h>

#include "VNCClient.h"


using namespace rdr;
using namespace rfb;
using namespace rfb::win32;
using namespace rfb::unity;

// - Statics & consts
static LogWriter vlog("CConn");


CConn::CConn()
	: m_pDesktopWindow(0), sock(0), sockEvent(CreateEvent(0, TRUE, FALSE, 0)), requestUpdate(false),
	sameMachine(false), encodingChange(false), formatChange(false),
	reverseConnection(false), lastUsedEncoding_(encodingRaw), isClosed_(false)
{
	
}

CConn::~CConn()
{
	if (m_pDesktopWindow != NULL)
		delete m_pDesktopWindow;
}

bool CConn::initialise(network::Socket* s, VNCClient * pClient, bool reverse)
{
	m_pClient = pClient;

	// Set the server's name for MRU purposes
	CharArray endpoint(s->getPeerEndpoint());
	setServerName(endpoint.buf);
	if (!options.host.buf)
		options.setHost(endpoint.buf);

	// Initialise the underlying CConnection
	setStreams(&s->inStream(), &s->outStream());

	// Enable processing of window messages while blocked on I/O
	s->inStream().setBlockCallback(this);

	// Initialise the viewer options
	applyOptions(options);

	// - Set which auth schemes we support, in order of preference
	addSecType(secTypeVncAuth);
	addSecType(secTypeNone);

	// Start the RFB protocol
	sock = s;
	reverseConnection = reverse;
	initialiseProtocol();

	return true;
}

void CConn::applyOptions(ConnOptions& opt) {
	// - If any encoding-related settings have changed then we must
	//   notify the server of the new settings
	encodingChange |= ((options.useLocalCursor != opt.useLocalCursor) ||
		(options.useDesktopResize != opt.useDesktopResize) ||
		(options.preferredEncoding != opt.preferredEncoding));

	// - If the preferred pixel format has changed then notify the server
	formatChange |= (options.fullColour != opt.fullColour);
	if (!opt.fullColour)
		formatChange |= (options.lowColourLevel != opt.lowColourLevel);

	// - Save the new set of options
	options = opt;

	// - Set optional features in ConnParams
	cp.supportsLocalCursor = options.useLocalCursor;
	cp.supportsDesktopResize = options.useDesktopResize;

	// - Configure connection sharing on/off
	setShared(options.shared);

	// - Whether to use protocol 3.3 for legacy compatibility
	setProtocol3_3(options.protocol3_3);
}


void
CConn::displayChanged() {
	// Display format has changed - recalculate the full-colour pixel format
	calculateFullColourPF();
}

void
CConn::paintCompleted() {
	// A repaint message has just completed - request next update if necessary
	requestNewUpdate();
}

/*
bool
CConn::sysCommand(WPARAM wParam, LPARAM lParam) {
  // - If it's one of our (F8 Menu) messages
  switch (wParam) {
  case IDM_FULLSCREEN:
	options.fullScreen = !window->isFullscreen();
	window->setFullscreen(options.fullScreen);
	return true;
  case IDM_CTRL_KEY:
	window->kbd.keyEvent(this, VK_CONTROL, 0, !window->kbd.keyPressed(VK_CONTROL));
	return true;
  case IDM_ALT_KEY:
	window->kbd.keyEvent(this, VK_MENU, 0, !window->kbd.keyPressed(VK_MENU));
	return true;
  case IDM_SEND_MENU_KEY:
	window->kbd.keyEvent(this, options.menuKey, 0, true);
	window->kbd.keyEvent(this, options.menuKey, 0, false);
	return true;
  case IDM_SEND_CAD:
	window->kbd.keyEvent(this, VK_CONTROL, 0, true);
	window->kbd.keyEvent(this, VK_MENU, 0, true);
	window->kbd.keyEvent(this, VK_DELETE, 0x1000000, true);
	window->kbd.keyEvent(this, VK_DELETE, 0x1000000, false);
	window->kbd.keyEvent(this, VK_MENU, 0, false);
	window->kbd.keyEvent(this, VK_CONTROL, 0, false);
	return true;
  case IDM_REQUEST_REFRESH:
	try {
	  writer()->writeFramebufferUpdateRequest(Rect(0,0,cp.width,cp.height), false);
	  requestUpdate = false;
	} catch (rdr::Exception& e) {
	  close(e.str());
	}
	return true;
  case IDM_NEWCONN:
	{
	  Thread* newThread = new CConnThread;
	}
	return true;
  case IDM_OPTIONS:
	// Update the monitor device name in the CConnOptions instance
	options.monitor.replaceBuf(window->getMonitor());
	showOptionsDialog();
	return true;
  case IDM_INFO:
	infoDialog.showDialog(this);
	return true;
  case IDM_ABOUT:
	AboutDialog::instance.showDialog();
	return true;
  };
  return false;
}
*/

void
CConn::closeWindow() {
	vlog.info("window closed");
	close();
}

void
CConn::blockCallback() {
	// - An InStream has blocked on I/O while processing an RFB message
	//   We re-enable socket event notifications, so we'll know when more
	//   data is available, then we sit and dispatch window events until
	//   the notification arrives.
	if (!isClosed()) {
		if (WSAEventSelect(sock->getFd(), sockEvent, FD_READ | FD_CLOSE) == SOCKET_ERROR)
			throw rdr::SystemException("Unable to wait for sokcet data", WSAGetLastError());
	}
	while (true) {
		// If we have closed then we can't block waiting for data
		if (isClosed())
			throw rdr::EndOfStream();

		// Wait for socket data, or a message to process
		DWORD result = MsgWaitForMultipleObjects(1, &sockEvent, FALSE, INFINITE, QS_ALLINPUT);
		if (result == WAIT_OBJECT_0) {
			// - Network event notification.  Return control to I/O routine.
			break;
		}
		else if (result == WAIT_FAILED) {
			// - The wait operation failed - raise an exception
			throw rdr::SystemException("blockCallback wait error", GetLastError());
		}

		// - There should be a message in the message queue
		MSG msg;
		while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE)) {
			// IMPORTANT: We mustn't call TranslateMessage() here, because instead we
			// call ToAscii() in CKeyboard::keyEvent().  ToAscii() stores dead key
			// state from one call to the next, which would be messed up by calls to
			// TranslateMessage() (actually it looks like TranslateMessage() calls
			// ToAscii() internally).
			DispatchMessage(&msg);
		}
	}

	// Before we return control to the InStream, reset the network event
	WSAEventSelect(sock->getFd(), sockEvent, 0);
	ResetEvent(sockEvent);
}


void CConn::keyEvent(rdr::U32 key, bool down) {
	if (!options.sendKeyEvents) return;
	try {
		writer()->keyEvent(key, down);
	}
	catch (rdr::Exception& e) {
		close(e.str());
	}
}
void CConn::pointerEvent(const Point& pos, int buttonMask) {
	if (!options.sendPtrEvents) return;
	try {
		writer()->pointerEvent(pos, buttonMask);
	}
	catch (rdr::Exception& e) {
		close(e.str());
	}
}
void CConn::clientCutText(const char* str, int len) {
	if (!options.clientCutText) return;
	if (state() != RFBSTATE_NORMAL) return;
	try {
		writer()->clientCutText(str, len);
	}
	catch (rdr::Exception& e) {
		close(e.str());
	}
}


CSecurity* CConn::getCSecurity(int secType)
{
	switch (secType) {
	case secTypeNone:
		return new CSecurityNone();
	case secTypeVncAuth:
		return new CSecurityVncAuth(this);
	default:
		throw Exception("Unsupported secType?");
	}
}


void
CConn::setColourMapEntries(int first, int count, U16* rgbs) {
	vlog.debug("setColourMapEntries: first=%d, count=%d", first, count);
	int i;
	for (i = 0; i < count; i++)
		m_pDesktopWindow->setColour(i + first, rgbs[i * 3], rgbs[i * 3 + 1], rgbs[i * 3 + 2]);
}

void
CConn::bell() {
	if (options.acceptBell)
		MessageBeep(-1);
}


void
CConn::setDesktopSize(int w, int h) 
{
	vlog.debug("setDesktopSize %dx%d", w, h);

	// Resize the window's buffer
	if (m_pDesktopWindow)
		m_pDesktopWindow->setSize(w, h);

	if (m_pClient)
		m_pClient->setConnectionState(BufferSizeChanged);

	// Tell the underlying CConnection
	CConnection::setDesktopSize(w, h);
}


void
CConn::close(const char* reason) {
	// If already closed then ignore this
	if (isClosed())
		return;

	// Save the reason & flag that we're closed & shutdown the socket
	isClosed_ = true;
	closeReason_.replaceBuf(strDup(reason));
	sock->shutdown();
}

void
CConn::framebufferUpdateEnd()
{
	


	if (options.autoSelect)
		autoSelectFormatAndEncoding();

	// Always request the next update
	requestUpdate = true;

	// Check that at least part of the window has changed
	requestNewUpdate();	
}


// autoSelectFormatAndEncoding() chooses the format and encoding appropriate
// to the connection speed:
//   Above 16Mbps (timing for at least a second), same machine, switch to raw
//   Above 3Mbps, switch to hextile
//   Below 1.5Mbps, switch to ZRLE
//   Above 1Mbps, switch to full colour mode
void
CConn::autoSelectFormatAndEncoding() {
	int kbitsPerSecond = sock->inStream().kbitsPerSecond();
	unsigned int newEncoding = options.preferredEncoding;

	if (kbitsPerSecond > 16000 && sameMachine &&
		sock->inStream().timeWaited() >= 10000) {
		newEncoding = encodingRaw;
	}
	else if (kbitsPerSecond > 3000) {
		newEncoding = encodingHextile;
	}
	else if (kbitsPerSecond < 1500) {
		newEncoding = encodingZRLE;
	}

	if (newEncoding != options.preferredEncoding) {
		vlog.info("Throughput %d kbit/s - changing to %s encoding",
			kbitsPerSecond, encodingName(newEncoding));
		options.preferredEncoding = newEncoding;
		encodingChange = true;
	}

	if (kbitsPerSecond > 1000) {
		if (!options.fullColour) {
			vlog.info("Throughput %d kbit/s - changing to full colour",
				kbitsPerSecond);
			options.fullColour = true;
			formatChange = true;
		}
	}
}

void
CConn::requestNewUpdate() {
	if (!requestUpdate) return;

	if (formatChange)
	{
		// Print the current pixel format
		char str[256];
		m_pDesktopWindow->getPF().print(str, 256);
		vlog.info("Using pixel format %s", str);

		// Save the connection pixel format and tell server to use it
		cp.setPF(m_pDesktopWindow->getPF());
		writer()->writeSetPixelFormat(cp.pf());
	}

	if (encodingChange) {
		vlog.info("Using %s encoding", encodingName(options.preferredEncoding));
		writer()->writeSetEncodings(options.preferredEncoding, true);
	}

	writer()->writeFramebufferUpdateRequest(Rect(0, 0, cp.width, cp.height),
		!formatChange);

	encodingChange = formatChange = requestUpdate = false;
}


void
CConn::calculateFullColourPF() {
	// If the server is palette based then use palette locally
	// Also, don't bother doing bgr222
	if (!serverDefaultPF.trueColour || (serverDefaultPF.depth < 6)) {
		fullColourPF = serverDefaultPF;
		options.fullColour = true;
	}
	else {
		// If server is trueColour, use lowest depth PF
		PixelFormat native = m_pDesktopWindow->getNativePF();
		if ((serverDefaultPF.bpp < native.bpp) ||
			((serverDefaultPF.bpp == native.bpp) &&
			(serverDefaultPF.depth < native.depth)))
			fullColourPF = serverDefaultPF;
		else
			fullColourPF = m_pDesktopWindow->getNativePF();
	}
	formatChange = true;
}


void
CConn::setName(const char* name) 
{
	CConnection::setName(name);
}


void CConn::serverInit()
{
	CConnection::serverInit();

	// Show the window
	m_pDesktopWindow = new DesktopWindow(this);
	m_pDesktopWindow->setSize(cp.width, cp.height);
	applyOptions(options);

	// Save the server's current format
	serverDefaultPF = cp.pf();

	// Calculate the full-colour format to use
	calculateFullColourPF();

	// Request the initial update
	vlog.info("requesting initial update");
	formatChange = encodingChange = requestUpdate = true;
	requestNewUpdate();
}

void
CConn::serverCutText(const char* str, int len) {
	if (!options.serverCutText) return;
	m_pDesktopWindow->serverCutText(str, len);
}


void CConn::beginRect(const Rect& r, unsigned int encoding)
{
	sock->inStream().startTiming();
}

void CConn::endRect(const Rect& r, unsigned int encoding)
{
	sock->inStream().stopTiming();
	lastUsedEncoding_ = encoding;

}

void CConn::fillRect(const Rect& r, Pixel pix) {
	m_pDesktopWindow->fillRect(r, pix);
}
void CConn::imageRect(const Rect& r, void* pixels) {
	m_pDesktopWindow->imageRect(r, pixels);
}
void CConn::copyRect(const Rect& r, int srcX, int srcY) {
	m_pDesktopWindow->copyRect(r, srcX, srcY);
}

void CConn::getUserPasswd(char** user, char** password)
{
	if (user && options.userName.buf)
		*user = strDup(options.userName.buf);
	if (password && options.password.buf)
		*password = strDup(options.password.buf);
	if ((user && !*user) || (password && !*password))
	{
		vlog.error("Error, no password : todo, ask to unity");
	}
	if (user) options.setUserName(*user);
	if (password) options.setPassword(*password);
}
