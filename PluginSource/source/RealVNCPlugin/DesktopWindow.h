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

 // -=- DesktopWindow.h

 // Each VNC connection instance (CConn) creates a DesktopWindow the
 // server initialisation message has been received.  The CConn is
 // responsible for all RFB-specific and network issues.  The
 // DesktopWindow is responsible for all GUI management issues.
 //
 // DesktopWindow provides a FullFramePixelBuffer interface for the
 // CConn to render updates into.  It also requires a callback object
 // to be supplied, which will be notified when various events occur.

#ifndef __RFB_WIN32_DESKTOP_WINDOW_H__
#define __RFB_WIN32_DESKTOP_WINDOW_H__

#include <rfb/Cursor.h>
#include <rfb_win32/CKeyboard.h>
#include <rfb_win32/CPointer.h>
#include <rfb_win32/Clipboard.h>
#include <rfb_win32/DIBSectionBuffer.h>
#include <rfb_win32/LogicalPalette.h>


#include "UnityTexturehandler.h"

using namespace rfb::win32;

namespace rfb
{
	namespace unity
	{
		class DesktopWindow : rfb::win32::Clipboard::Notifier {
		public:
			class Callback;

			DesktopWindow(Callback* cb_);
			~DesktopWindow();

			

			// - Determine the native pixel format of the window
			//   This can (and often will) differ from the PixelBuffer format
			PixelFormat getNativePF() const;

		
			// - Set the window title
			void setName(const char* name);

			// - Set the key that causes the system/F8 menu to be displayed
			void setMenuKey(rdr::U8 key) { menuKey = key; }

			// - Pointer event handling
			void setEmulate3(bool em3) { ptr.enableEmulate3(em3); }
			void setPointerEventInterval(int interval) { ptr.enableInterval(interval); }

			// - Set the pixel format, size etc of the underlying PixelBuffer
			void setPF(const PixelFormat& pf);
			PixelFormat getPF() const { return texture->getPF(); }

			void setSize(int w, int h);
			void setColour(int i, int r, int g, int b) { texture->setColour(i, r, g, b); }

			// - Set the window fullscreen / determine whether it is fullscreen
			void setFullscreen(bool fs);
			bool isFullscreen() { return fullscreenActive; }

			// - Set whether to disable special Windows keys & pass them straight to server
			void setDisableWinKeys(bool dwk);

			// - Set/get which monitor the window should be displayed on
			void setMonitor(const char* monitor);
			char* getMonitor() const;

			// - Set the local clipboard
			void serverCutText(const char* str, int len);

			// - Draw into the desktop buffer & update the window
			void fillRect(const Rect& r, Pixel pix);
			void imageRect(const Rect& r, void* pixels);
			void copyRect(const Rect& r, int srcX, int srcY);

			void invertRect(const Rect& r);

			// - Update the window palette if the display is palette-based.
			//   Colours are pulled from the desktop buffer's ColourMap.
			//   Only the specified range of indexes is dealt with.
			//   After the update, the entire window is redrawn.
			void refreshWindowPalette(int start, int count);

			// Clipboard::Notifier interface
			void notifyClipboardChanged(const char* text, int len);

			// DesktopWindow Callback interface
			class Callback : public InputHandler {
			public:
				virtual ~Callback() {}
				virtual void displayChanged() = 0;
				virtual void paintCompleted() = 0;
				
				virtual void closeWindow() = 0;
				virtual void refreshMenu(bool enableSysItems) = 0;
			};

			// Currently accessible so that the CConn can releaseAllKeys & check
			// whether Ctrl and Alt are down...
			rfb::win32::CKeyboard kbd;

			UnityTextureHandler * texture;

			
			void Hide();
			bool isVisible();


		protected:
			// Routines to convert between Desktop and client (window) coordinates
			Point desktopToClient(const Point& p) {
				Point pos = p;
				if (client_size.width() > texture->width())
					pos.x += (client_size.width() - texture->width()) / 2;
				else if (client_size.width() < texture->width())
					pos.x -= scrolloffset.x;
				if (client_size.height() > texture->height())
					pos.y += (client_size.height() - texture->height()) / 2;
				else if (client_size.height() < texture->height())
					pos.y -= scrolloffset.y;
				return pos;
			}
			Rect desktopToClient(const Rect& r) {
				return Rect(desktopToClient(r.tl), desktopToClient(r.br));
			}
			Point clientToDesktop(const Point& p) {
				Point pos = p;
				if (client_size.width() > texture->width())
					pos.x -= (client_size.width() - texture->width()) / 2;
				else if (client_size.width() < texture->width())
					pos.x += scrolloffset.x;
				if (client_size.height() > texture->height())
					pos.y -= (client_size.height() - texture->height()) / 2;
				else if (client_size.height() < texture->height())
					pos.y += scrolloffset.y;
				return pos;
			}
			Rect clientToDesktop(const Rect& r) {
				return Rect(clientToDesktop(r.tl), clientToDesktop(r.br));
			}

			// Internal routine used by the scrollbars & bump scroller to select
			// the portion of the Desktop to display
			bool setViewportOffset(const Point& tl);

			// Win32-specific input handling
			rfb::win32::CPointer ptr;
			Point oldpos;
			rfb::win32::Clipboard clipboard;

			// Palette handling
			LogicalPalette windowPalette;
			bool palette_changed;

			// - Full-screen mode
			RECT fullscreenOldRect;
			DWORD fullscreenOldFlags;
			bool fullscreenActive;
			bool fullscreenRestore;

			// Cursor handling
			Cursor cursor;
			bool systemCursorVisible;  // Should system-cursor be drawn?
			bool trackingMouseLeave;
			bool cursorInBuffer;    // Is cursor position within server buffer? (ONLY for LocalCursor)
			bool cursorVisible;     // Is cursor currently rendered?
			bool cursorAvailable;   // Is cursor available for rendering?
			Point cursorPos;
			ManagedPixelBuffer cursorBacking;
			

			// Local window state
			
			bool has_focus;
			Rect window_size;
			Rect client_size;
			Point scrolloffset;
			Point maxscrolloffset;
			
			rdr::U8 menuKey;

			Callback* callback;
		};

	};

};

#endif // __RFB_WIN32_DESKTOP_WINDOW_H__


