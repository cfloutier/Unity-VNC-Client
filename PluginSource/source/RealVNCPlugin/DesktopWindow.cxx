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
#include <windows.h>
#include "DesktopWindow.h"


#include <commctrl.h>
#include <rfb/Configuration.h>
#include <rfb/LogWriter.h>
#include <rfb_win32/WMShatter.h>
#include <rfb_win32/LowLevelKeyEvents.h>
#include <rfb_win32/MonitorInfo.h>
#include <rfb_win32/DeviceContext.h>
#include <rfb_win32/Win32Util.h>

using namespace rfb;
using namespace rfb::win32;
using namespace rfb::unity;

// - Statics & consts

static LogWriter vlog("DesktopWindow");

const int TIMER_BUMPSCROLL = 1;
const int TIMER_POINTER_INTERVAL = 2;
const int TIMER_POINTER_3BUTTON = 3;

//
// -=- DesktopWindow instance implementation
//

DesktopWindow::DesktopWindow(Callback* cb)
	:
	client_size(0, 0, 16, 16),
	window_size(0, 0, 32, 32),

	cursorVisible(false), cursorAvailable(false), cursorInBuffer(false),
	systemCursorVisible(true), trackingMouseLeave(false),
	palette_changed(false),
	callback(cb) 
{
	vlog.debug("DesktopWindow");
	m_pTexture = 0;
	// Create the window
	const char* name = "DesktopWindow";
}

DesktopWindow::~DesktopWindow() 
{
	vlog.debug("~DesktopWindow");

	vlog.debug("~DesktopWindow done");
}



/*

LRESULT
DesktopWindow::processMessage(UINT msg, WPARAM wParam, LPARAM lParam) {
  switch (msg) {

	// -=- Process standard window messages

  case WM_DISPLAYCHANGE:
	// Display format has changed - notify callback
	callback->displayChanged();
	break;

  case WM_PAINT:
	{
	  PAINTSTRUCT ps;
	  HDC paintDC = BeginPaint(handle, &ps);
	  if (!paintDC)
		throw rdr::SystemException("unable to BeginPaint", GetLastError());
	  Rect pr = Rect(ps.rcPaint.left, ps.rcPaint.top, ps.rcPaint.right, ps.rcPaint.bottom);

	  if (!pr.is_empty()) {

		// Draw using the correct palette
		PaletteSelector pSel(paintDC, windowPalette.getHandle());

		if (texture->bitmap) {
		  // Update the bitmap's palette
		  if (palette_changed) {
			palette_changed = false;
			texture->refreshPalette();
		  }

		  // Get device context
		  BitmapDC bitmapDC(paintDC, texture->bitmap);

		  // Blit the border if required
		  Rect bufpos = desktopToClient(texture->getRect());
		  if (!pr.enclosed_by(bufpos)) {
			vlog.debug("draw border");
			HBRUSH black = (HBRUSH) GetStockObject(BLACK_BRUSH);
			RECT r;
			SetRect(&r, 0, 0, bufpos.tl.x, client_size.height()); FillRect(paintDC, &r, black);
			SetRect(&r, bufpos.tl.x, 0, bufpos.br.x, bufpos.tl.y); FillRect(paintDC, &r, black);
			SetRect(&r, bufpos.br.x, 0, client_size.width(), client_size.height()); FillRect(paintDC, &r, black);
			SetRect(&r, bufpos.tl.x, bufpos.br.y, bufpos.br.x, client_size.height()); FillRect(paintDC, &r, black);
		  }

		  // Do the blit
		  Point buf_pos = clientToDesktop(pr.tl);

		  if (!BitBlt(paintDC, pr.tl.x, pr.tl.y, pr.width(), pr.height(),
					  bitmapDC, buf_pos.x, buf_pos.y, SRCCOPY))
			throw rdr::SystemException("unable to BitBlt to window", GetLastError());
		}
	  }

	  EndPaint(handle, &ps);

	  // - Notify the callback that a paint message has finished processing
	  callback->paintCompleted();
	}
	return 0;

	// -=- Palette management

  case WM_PALETTECHANGED:
	vlog.debug("WM_PALETTECHANGED");
	if ((HWND)wParam == handle) {
	  vlog.debug("ignoring");
	  break;
	}
  case WM_QUERYNEWPALETTE:
	vlog.debug("re-selecting palette");
	{
	  WindowDC wdc(handle);
	  PaletteSelector pSel(wdc, windowPalette.getHandle());
	  if (pSel.isRedrawRequired()) {
		InvalidateRect(handle, 0, FALSE);
		UpdateWindow(handle);
	  }
	}
	return TRUE;

	// -=- Window position

	// Prevent the window from being resized to be too large if in normal mode.
	// If maximized or fullscreen the allow oversized windows.

  case WM_WINDOWPOSCHANGING:
	{
	  WINDOWPOS* wpos = (WINDOWPOS*)lParam;
	  if (wpos->flags & SWP_NOSIZE)
		break;

	  // Work out how big the window should ideally be
	  DWORD current_style = GetWindowLong(handle, GWL_STYLE);
	  DWORD style = current_style & ~(WS_VSCROLL | WS_HSCROLL);
	  RECT r;
	  SetRect(&r, 0, 0, texture->width(), texture->height());
	  AdjustWindowRect(&r, style, FALSE);
	  Rect reqd_size = Rect(r.left, r.top, r.right, r.bottom);

	  if (current_style & WS_VSCROLL)
		reqd_size.br.x += GetSystemMetrics(SM_CXVSCROLL);
	  if (current_style & WS_HSCROLL)
		reqd_size.br.y += GetSystemMetrics(SM_CXHSCROLL);
	  RECT current;
	  GetWindowRect(handle, &current);

	  if (!(GetWindowLong(handle, GWL_STYLE) & WS_MAXIMIZE) && !fullscreenActive) {
		// Ensure that the window isn't resized too large
		if (wpos->cx > reqd_size.width()) {
		  wpos->cx = reqd_size.width();
		  wpos->x = current.left;
		}
		if (wpos->cy > reqd_size.height()) {
		  wpos->cy = reqd_size.height();
		  wpos->y = current.top;
		}
	  }
	}
	break;

	// Add scrollbars if required and update window size info we have cached.

  case WM_SIZE:
	{
	  Point old_offset = desktopToClient(Point(0, 0));

	  // Update the cached sizing information
	  RECT r;
	  GetWindowRect(handle, &r);
	  window_size = Rect(r.left, r.top, r.right, r.bottom);
	  GetClientRect(handle, &r);
	  client_size = Rect(r.left, r.top, r.right, r.bottom);

	  // Determine whether scrollbars are required
	  calculateScrollBars();

	  // Redraw if required
	  if ((!old_offset.equals(desktopToClient(Point(0, 0)))))
		InvalidateRect(handle, 0, TRUE);
	}
	break;

  case WM_VSCROLL:
  case WM_HSCROLL:
	{
	  Point delta;
	  int newpos = (msg == WM_VSCROLL) ? scrolloffset.y : scrolloffset.x;

	  switch (LOWORD(wParam)) {
	  case SB_PAGEUP: newpos -= 50; break;
	  case SB_PAGEDOWN: newpos += 50; break;
	  case SB_LINEUP: newpos -= 5; break;
	  case SB_LINEDOWN: newpos += 5; break;
	  case SB_THUMBTRACK:
	  case SB_THUMBPOSITION: newpos = HIWORD(wParam); break;
	  default: vlog.info("received unknown scroll message");
	  };

	  if (msg == WM_HSCROLL)
		setViewportOffset(Point(newpos, scrolloffset.y));
	  else
		setViewportOffset(Point(scrolloffset.x, newpos));

	  SCROLLINFO si;
	  si.cbSize = sizeof(si);
	  si.fMask  = SIF_POS;
	  si.nPos   = newpos;
	  SetScrollInfo(handle, (msg == WM_VSCROLL) ? SB_VERT : SB_HORZ, &si, TRUE);
	}
	break;

	// -=- Bump-scrolling

  case WM_TIMER:
	switch (wParam) {
	case TIMER_BUMPSCROLL:
	  if (!setViewportOffset(scrolloffset.translate(bumpScrollDelta)))
		bumpScrollTimer.stop();
	  break;
	case TIMER_POINTER_INTERVAL:
	case TIMER_POINTER_3BUTTON:
	  ptr.handleTimer(callback, wParam);
	  break;
	}
	break;

	// -=- Cursor shape/visibility handling

  case WM_SETCURSOR:
	if (LOWORD(lParam) != HTCLIENT)
	  break;
	SetCursor(cursorInBuffer ? dotCursor : arrowCursor);
	return TRUE;

  case WM_MOUSELEAVE:
	trackingMouseLeave = false;
	cursorOutsideBuffer();
	return 0;

	// -=- Mouse input handling

  case WM_MOUSEMOVE:
  case WM_LBUTTONUP:
  case WM_MBUTTONUP:
  case WM_RBUTTONUP:
  case WM_LBUTTONDOWN:
  case WM_MBUTTONDOWN:
  case WM_RBUTTONDOWN:
#ifdef WM_MOUSEWHEEL
  case WM_MOUSEWHEEL:
#endif
	if (has_focus)
	{
	  if (!trackingMouseLeave) {
		TRACKMOUSEEVENT tme;
		tme.cbSize = sizeof(TRACKMOUSEEVENT);
		tme.dwFlags = TME_LEAVE;
		tme.hwndTrack = handle;
		_TrackMouseEvent(&tme);
		trackingMouseLeave = true;
	  }
	  int mask = 0;
	  if (LOWORD(wParam) & MK_LBUTTON) mask |= 1;
	  if (LOWORD(wParam) & MK_MBUTTON) mask |= 2;
	  if (LOWORD(wParam) & MK_RBUTTON) mask |= 4;

#ifdef WM_MOUSEWHEEL
	  if (msg == WM_MOUSEWHEEL) {
		int delta = (short)HIWORD(wParam);
		int repeats = (abs(delta)+119) / 120;
		int wheelMask = (delta > 0) ? 8 : 16;
		vlog.debug("repeats %d, mask %d\n",repeats,wheelMask);
		for (int i=0; i<repeats; i++) {
		  ptr.pointerEvent(callback, oldpos, mask | wheelMask);
		  ptr.pointerEvent(callback, oldpos, mask);
		}
	  } else {
#endif
		Point clientPos = Point(LOWORD(lParam), HIWORD(lParam));
		Point p = clientToDesktop(clientPos);

		// If the mouse is not within the server texture area, do nothing
		cursorInBuffer = texture->getRect().contains(p);
		if (!cursorInBuffer) {
		  cursorOutsideBuffer();
		  break;
		}

		// If we're locally rendering the cursor then redraw it
		if (cursorAvailable) {
		  // - Render the cursor!
		  if (!p.equals(cursorPos)) {
			hideLocalCursor();
			cursorPos = p;
			showLocalCursor();
			if (cursorVisible)
			  hideSystemCursor();
		  }
		}

		// If we are doing bump-scrolling then try that first...
		if (processBumpScroll(clientPos))
		  break;

		// Send a pointer event to the server
		ptr.pointerEvent(callback, p, mask);
		oldpos = p;
#ifdef WM_MOUSEWHEEL
	  }
#endif
	} else {
	  cursorOutsideBuffer();
	}
	break;

	// -=- Track whether or not the window has focus

  case WM_SETFOCUS:
	has_focus = true;
	break;
  case WM_KILLFOCUS:
	has_focus = false;
	cursorOutsideBuffer();
	// Restore the keyboard to a consistent state
	kbd.releaseAllKeys(callback);
	break;

	// -=- Handle the extra window menu items

	// Pass system menu messages to the callback and only attempt
	// to process them ourselves if the callback returns false.
  case WM_SYSCOMMAND:
	// Call the supplied callback
	if (callback->sysCommand(wParam, lParam))
	  break;

	// - Not processed by the callback, so process it as a system message
	switch (wParam & 0xfff0) {

	  // When restored, ensure that full-screen mode is re-enabled if required.
	case SC_RESTORE:
	  {
	  if (GetWindowLong(handle, GWL_STYLE) & WS_MINIMIZE) {
		rfb::win32::SafeDefWindowProc(handle, msg, wParam, lParam);
		setFullscreen(fullscreenRestore);
	  }
	  else if (fullscreenActive)
		setFullscreen(false);
	  else
		rfb::win32::SafeDefWindowProc(handle, msg, wParam, lParam);

	  return 0;
	  }

	  // If we are maximized or minimized then that cancels full-screen mode.
	case SC_MINIMIZE:
	case SC_MAXIMIZE:
	  fullscreenRestore = fullscreenActive;
	  setFullscreen(false);
	  break;

	  // If the menu is about to be shown, make sure it's up to date
	case SC_KEYMENU:
	case SC_MOUSEMENU:
	  callback->refreshMenu(true);
	  break;

	};
	break;

	// Treat all menu commands as system menu commands
  case WM_COMMAND:
	SendMessage(handle, WM_SYSCOMMAND, wParam, lParam);
	return 0;

	// -=- Handle keyboard input

  case WM_KEYUP:
  case WM_KEYDOWN:
	// Hook the MenuKey to pop-up the window menu
	if (menuKey && (wParam == menuKey)) {

	  bool ctrlDown = (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;
	  bool altDown = (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;
	  bool shiftDown = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
	  if (!(ctrlDown || altDown || shiftDown)) {

		// If MenuKey is being released then pop-up the menu
		if ((msg == WM_KEYDOWN)) {
		  // Make sure it's up to date
		  callback->refreshMenu(false);

		  // Show it under the pointer
		  POINT pt;
		  GetCursorPos(&pt);
		  cursorInBuffer = false;
		  TrackPopupMenu(GetSystemMenu(handle, FALSE),
			TPM_CENTERALIGN | TPM_VCENTERALIGN, pt.x, pt.y, 0, handle, 0);
		}

		// Ignore the MenuKey keypress for both press & release events
		return 0;
	  }
	}
	case WM_SYSKEYDOWN:
	case WM_SYSKEYUP:
	kbd.keyEvent(callback, wParam, lParam, (msg == WM_KEYDOWN) || (msg == WM_SYSKEYDOWN));
	return 0;

	// -=- Handle the window closing

  case WM_CLOSE:
	vlog.debug("WM_CLOSE %x", handle);
	callback->closeWindow();
	break;

  };

  return rfb::win32::SafeDefWindowProc(handle, msg, wParam, lParam);
}

*/

void
DesktopWindow::notifyClipboardChanged(const char* text, int len) 
{
	callback->clientCutText(text, len);
}

void
DesktopWindow::setSize(int w, int h)
{
	vlog.debug("setSize %dx%d", w, h);

	// Resize the backing texture
	m_pTexture->setSize(w, h);
}


PixelFormat
DesktopWindow::getNativePF() const
{
	PixelFormat pf(32, 32, 8, 8, 8, 0, 8, 16);
	pf.trueColour = true;
	return pf;
}

/*
void
DesktopWindow::refreshWindowPalette(int start, int count) {
	vlog.debug("refreshWindowPalette(%d, %d)", start, count);

	Colour colours[256];
	if (count > 256) {
		vlog.debug("%d palette entries", count);
		throw rdr::Exception("too many palette entries");
	}

	// Copy the palette from the DIBSectionBuffer
	ColourMap* cm = texture->getColourMap();
	if (!cm) return;
	for (int i = 0; i < count; i++) {
		int r, g, b;
		cm->lookup(i, &r, &g, &b);
		colours[i].r = r;
		colours[i].g = g;
		colours[i].b = b;
	}

	// Set the window palette
	windowPalette.setEntries(start, count, colours);

	// Cause the window to be redrawn
	palette_changed = true;
}
*/

void
DesktopWindow::serverCutText(const char* str, int len) {
	CharArray t(len + 1);
	memcpy(t.buf, str, len);
	t.buf[len] = 0;
	clipboard.setClipText(t.buf);
}


void DesktopWindow::fillRect(const Rect& r, Pixel pix)
{

	m_pTexture->fillRect(r, pix);

}
void DesktopWindow::imageRect(const Rect& r, void* pixels) {

	m_pTexture->imageRect(r, pixels);
}

void DesktopWindow::copyRect(const Rect& r, int srcX, int srcY)
{
	m_pTexture->copyRect(r, r.tl.x - srcX, r.tl.y - srcY);
}
