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

 // -=- BufferUpdate.h


#ifndef _BUFFERUPDATE_H__
#define _BUFFERUPDATE_H__

#include <rfb/Rect.h>
#include <rfb/Pixel.h>

using namespace rdr;


namespace rfb 
{
	namespace unity 
	{
		// useful to temporize buffer modifications
		class BufferUpdate
		{
		public:
			// pixel buffer
			BufferUpdate(const Rect& r, void * pixels);
			// fill rect
			BufferUpdate(const Rect& r, Pixel pix);
			// copy rect
			BufferUpdate(const Rect& r, int srcX, int srcY);

			~BufferUpdate();

			void apply(U8 * buffer, int stride);
		

		protected:



			enum BufferUpdateMode
			{
				fillRect,
				copyRect,
				setRect
			};

			BufferUpdateMode mode;
			Rect m_destRc;
			void * m_pixels;
			Pixel m_pix;
			Point m_src;
			
		};
	};
};

#endif


