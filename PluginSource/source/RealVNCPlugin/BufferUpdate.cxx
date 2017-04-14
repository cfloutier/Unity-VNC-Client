#include "BufferUpdate.h"

#include <stdio.h>
#include <string.h>
#include <rfb/LogWriter.h>

using namespace rfb::unity;
using namespace rdr;
using namespace rfb;


static LogWriter vlog("BufferUpdate");

BufferUpdate::BufferUpdate(const Rect& r, U8 * pixels) : m_src(0, 0)
{
	mode = BufferUpdate::imageRect;
	m_destRc = r;
	m_pixels = pixels;
	m_pix = 0;
	
}

BufferUpdate::BufferUpdate(const Rect& r, Pixel pix): m_src(0, 0)
{
	mode = BufferUpdate::fillRect;
	m_destRc = r;
	m_pix = pix;
	m_pixels = 0;	
}

BufferUpdate::BufferUpdate(const Rect& r, int srcX, int srcY): m_src(srcX, srcY)
{
	mode = BufferUpdate::copyRect;
	m_destRc = r;
	m_pixels = 0;
}

BufferUpdate::~BufferUpdate()
{
	
}

U8* getPixels(const Rect& r, U8 * buffer, int stride, int bytesPerPixel)
{
	return &buffer[r.tl.x* bytesPerPixel + r.tl.y *stride];
}

void BufferUpdate::FillRect(const Rect& r, Pixel pix, U8 * buffer, int stride)
{
	//vlog.debug("fillRect__ %dx%d [%d,%d]", r.tl.x, r.tl.y, r.width(), r.height());
	int bytesPerPixel = 4;
	U8* start = getPixels(r, buffer, stride, bytesPerPixel);
	int bytesPerRow = stride;

	int bytesPerFill = bytesPerPixel * r.width();

	U8* end = start + (bytesPerRow * r.height());
	U8* data = start;
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

void BufferUpdate::CopyRect(const Rect &rect, const Point &move_by_delta, U8 * buffer, int stride)
{
	unsigned int bytesPerPixel = 4;

	// We assume that the specified rectangle is pre-clipped to the buffer
	unsigned int bytesPerRow, bytesPerMemCpy;
	Rect srect = rect.translate(move_by_delta.negate());
	
	bytesPerRow = stride;
	bytesPerMemCpy = rect.width() *bytesPerPixel ;
	getPixels(rect, buffer, stride, bytesPerPixel);
	if (move_by_delta.y <= 0) {
		U8* dest = &buffer[rect.tl.x* bytesPerPixel + rect.tl.y *stride];   
		U8* src = &buffer[srect.tl.x* bytesPerPixel + srect.tl.y *stride];    
		
		for (int i = rect.tl.y; i<rect.br.y; i++) {
			memmove(dest, src, bytesPerMemCpy);
			dest += bytesPerRow;
			src += bytesPerRow;
		}
	}
	else {
		U8* dest = &buffer[rect.tl.x* bytesPerPixel + (rect.br.y - 1) *stride];
		U8* src = &buffer[srect.tl.x* bytesPerPixel + (srect.br.y - 1) *stride];  
		for (int i = rect.tl.y; i<rect.br.y; i++) {
			memmove(dest, src, bytesPerMemCpy);
			dest -= bytesPerRow;
			src -= bytesPerRow;
		}
	}
}

void BufferUpdate::ImageRect(const Rect& r, const void* pixels, U8 * buffer, int destStride) {
	int bytesPerPixel = 4;
	
	U8* dest = getPixels(r, buffer, destStride, bytesPerPixel);

	int bytesPerDestRow = destStride;

	int bytesPerSrcRow = bytesPerPixel *  r.width();

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
		FillRect(m_destRc, m_pix, buffer, stride);
		break;
	case rfb::unity::BufferUpdate::copyRect:
		CopyRect(m_destRc, m_src, buffer, stride);
		break;
	case rfb::unity::BufferUpdate::imageRect:
		ImageRect(m_destRc, m_pixels, buffer, stride);
		break;
	default:
		break;
	}
}