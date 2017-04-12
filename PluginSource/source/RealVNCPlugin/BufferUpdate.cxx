#include "BufferUpdate.h"

#include <stdio.h>
#include <string.h>

using namespace rfb::unity;
using namespace rdr;
using namespace rfb;

BufferUpdate::BufferUpdate(const Rect& r, void * pixels): m_src(0, 0)
{
	mode = BufferUpdate::fillRect;
	m_destRc = r;
	m_pixels = pixels;
	m_pix = 0;
	
}

BufferUpdate::BufferUpdate(const Rect& r, Pixel pix): m_src(0, 0)
{
	mode = BufferUpdate::setRect;
	m_destRc = r;
	m_pix = pix;
	m_pixels = 0;
	
}

BufferUpdate::BufferUpdate(const Rect& r, int srcX, int srcY): m_src(srcX, srcY)
{
	mode = BufferUpdate::copyRect;
	m_destRc = r;
}

BufferUpdate::~BufferUpdate()
{
	if (m_pixels)
		delete[] m_pixels;
}

U8* getPixels(const Rect& r, U8 * buffer, int stride, int bytesPerPixel)
{
	return &buffer[(r.tl.x + (r.tl.y *stride)) * bytesPerPixel];
}

void fillRect__(const Rect& r, Pixel pix, U8 * buffer, int stride)
{
	int bytesPerPixel = 4;
	U8* data = getPixels(r, buffer, stride, bytesPerPixel);
	int bytesPerRow = bytesPerPixel * stride;

	int bytesPerFill = bytesPerPixel * r.width();

	U8* end = data + (bytesPerRow * r.height());
	while (data < end) {
		switch (bytesPerPixel) {
		case 1:
			memset(data, pix, bytesPerFill);
			break;
		case 2:
		{
			U16* optr = (U16*)data;
			U16* eol = optr + r.width();
			while (optr < eol)
				*optr++ = pix;
		}
		break;
		case 4:
		{
			U32* optr = (U32*)data;
			U32* eol = optr + r.width();
			while (optr < eol)
				*optr++ = pix;
		}
		break;
		}
		data += bytesPerRow;
	}
}

void copyRect__(const Rect &rect, const Point &move_by_delta, U8 * buffer, int stride)
{
	unsigned int bytesPerPixel = 4;
	
	U8* data = getPixels(rect, buffer, stride, bytesPerPixel);
	// We assume that the specified rectangle is pre-clipped to the buffer
	unsigned int bytesPerRow, bytesPerMemCpy;
	Rect srect = rect.translate(move_by_delta.negate());
	
	bytesPerRow = stride * bytesPerPixel;
	bytesPerMemCpy = rect.width() * bytesPerPixel;
	if (move_by_delta.y <= 0) {
		U8* dest = data + rect.tl.x*bytesPerPixel + rect.tl.y*bytesPerRow;
		U8* src = data + srect.tl.x*bytesPerPixel + srect.tl.y*bytesPerRow;
		for (int i = rect.tl.y; i<rect.br.y; i++) {
			memmove(dest, src, bytesPerMemCpy);
			dest += bytesPerRow;
			src += bytesPerRow;
		}
	}
	else {
		U8* dest = data + rect.tl.x*bytesPerPixel + (rect.br.y - 1)*bytesPerRow;
		U8* src = data + srect.tl.x*bytesPerPixel + (srect.br.y - 1)*bytesPerRow;
		for (int i = rect.tl.y; i<rect.br.y; i++) {
			memmove(dest, src, bytesPerMemCpy);
			dest -= bytesPerRow;
			src -= bytesPerRow;
		}
	}
}

void imageRect__(const Rect& r, const void* pixels, U8 * buffer, int destStride) {
	int bytesPerPixel = 4;
	
	U8* dest = getPixels(r, buffer, destStride, bytesPerPixel);
	int bytesPerDestRow = bytesPerPixel * destStride;
	int srcStride = r.width();
	int bytesPerSrcRow = bytesPerPixel * srcStride;
	int bytesPerFill = bytesPerPixel * r.width();
	const U8* src = (const U8*)pixels;
	U8* end = dest + (bytesPerDestRow * r.height());
	while (dest < end) {
		memcpy(dest, src, bytesPerFill);
		dest += bytesPerDestRow;
		src += bytesPerSrcRow;
	}
}



void BufferUpdate::apply(U8 * buffer, int stride)
{
	switch (mode)
	{
	case rfb::unity::BufferUpdate::fillRect:
		fillRect__(m_destRc, m_pix, buffer, stride);
		break;
	case rfb::unity::BufferUpdate::copyRect:
		copyRect__(m_destRc, m_src, buffer, stride);
		break;
	case rfb::unity::BufferUpdate::setRect:
		imageRect__(m_destRc, m_pixels, buffer, stride);
		break;
	default:
		break;

	}



}