#include "UnityTextureHandler.h"

#include <assert.h>
#include <math.h>
#include <vector>
#include "windows.h"
#include <rfb/LogWriter.h>

using namespace rfb::unity;
using namespace rfb;

static LogWriter vlog("UnityTextureHandler");

UnityTextureHandler::UnityTextureHandler()
{
	m_width = m_height = 512;
	tempBuffer = NULL;
	m_ready = false;
	
	InitializeCriticalSection(&CriticalSectionTempBuffer);
}

UnityTextureHandler::~UnityTextureHandler()
{
	exitThread = true;

	while (threadIsRunning)
		Sleep(5);

	DeleteCriticalSection(&CriticalSectionTempBuffer);

	if (tempBuffer != NULL) delete[] tempBuffer;
}

void UnityTextureHandler::run()
{
	vlog.debug("Start Rendering Thread");
	threadIsRunning = true;
	while (!exitThread)
	{
		RandomSquare(256);
		Sleep(5);
	}

	vlog.debug("End Rendering Thread");

	threadIsRunning = false;
}

void UnityTextureHandler::build(void * handle,
	RenderAPI* CurrentAPI,
	UnityGfxRenderer DeviceType,
	IUnityInterfaces* UnityInterfaces,
	IUnityGraphics* Graphics)
{
	m_TextureHandle = handle;
	m_CurrentAPI = CurrentAPI;
	m_DeviceType = DeviceType;
	m_UnityInterfaces = UnityInterfaces;
	m_Graphics = Graphics;

	// just to get the  textureRowPitch
	void * t = startModify();
	endModify(t);

	EnterCriticalSection(&CriticalSectionTempBuffer);

	if (tempBuffer != NULL) 
		delete[] tempBuffer;

	bufferSize = textureRowPitch*m_height;
	tempBuffer = new unsigned char[bufferSize];
	
	LeaveCriticalSection(&CriticalSectionTempBuffer);

	vlog.debug("Build texture, size is %dx%d", m_width, m_height);
	vlog.debug("Build texture, buffer size is %d", bufferSize);
	vlog.debug("Build texture, tempBuffer is 0x%X", tempBuffer);
	// start the main Thread
	//start();

	m_ready = true;
}

void UnityTextureHandler::setSize(int width, int height)
{
	vlog.debug("setSize %dx%d", width, height);

	m_width = width;
	m_height = height;

	m_ready = false;
}


float g_startTime = -1;
void UnityTextureHandler::Sinuses()
{
	EnterCriticalSection(&CriticalSectionTempBuffer);
	float g_Time = (float)GetTickCount() / 1000;
	if (g_startTime == -1)
		g_startTime = g_Time;
	g_Time = g_Time - g_startTime;
	const float t = g_Time * 4.0f;

	unsigned char* dst = tempBuffer;
	for (int y = 0; y < m_height; ++y)
	{
		unsigned char* ptr = dst;
		for (int x = 0; x < m_width; ++x)
		{
			// Simple "plasma effect": several combined sine waves
			int vv = int(
				(127.0f + (127.0f * sinf(x / 7.0f + t))) +
				(127.0f + (127.0f * sinf(y / 5.0f - t))) +
				(127.0f + (127.0f * sinf((x + y) / 6.0f - t))) +
				(127.0f + (127.0f * sinf(sqrtf(float(x*x + y*y)) / 4.0f - t)))
				) / 4;

			// Write the texture pixel
			ptr[0] = vv;
			ptr[1] = vv;
			ptr[2] = vv;
			ptr[3] = vv;

			// To next pixel (our pixels are 4 bpp)
			ptr += 4;
		}

		// To next image row
		dst += textureRowPitch;
	}

	LeaveCriticalSection(&CriticalSectionTempBuffer);
}
void UnityTextureHandler::Noise()
{
	EnterCriticalSection(&CriticalSectionTempBuffer);
	unsigned char* dst = tempBuffer;

	for (int y = 0; y < m_height; ++y)
	{
		unsigned char* ptr = dst;
		for (int x = 0; x < m_width; ++x)
		{
			// Write the texture pixel
			ptr[0] = rand() % 256;
			ptr[1] = rand() % 256;
			ptr[2] = rand() % 256;
			ptr[3] = rand() % 256;

			// To next pixel (our pixels are 4 bpp)
			ptr += 4;
		}

		// To next image row
		dst += textureRowPitch;
	}

	LeaveCriticalSection(&CriticalSectionTempBuffer);
}

void UnityTextureHandler::RandomSquare(int sizeSqr)
{
	EnterCriticalSection(&CriticalSectionTempBuffer);
	unsigned char* dst = tempBuffer;

	int xPos = rand() % (m_width - sizeSqr);
	int ypos = rand() % (m_height - sizeSqr);

	Rect rc(xPos, ypos, xPos+sizeSqr, ypos +sizeSqr);

	BufferUpdate::FillRect(rc, rand(), tempBuffer, textureRowPitch);

	LeaveCriticalSection(&CriticalSectionTempBuffer);
}

void* UnityTextureHandler::startModify()
{
	// Unknown / unsupported graphics device type? Do nothing
	if (m_CurrentAPI == NULL)
		return NULL;

	if (!m_TextureHandle)
		return NULL;

	void* textureDataPtr = m_CurrentAPI->BeginModifyTexture(m_TextureHandle, m_width, m_height, &textureRowPitch);
	if (!textureDataPtr)
		return NULL;

	EnterCriticalSection(&CriticalSectionTempBuffer);
	return textureDataPtr;
}

void UnityTextureHandler::endModify(void * textureDataPtr)
{
	m_CurrentAPI->EndModifyTexture(m_TextureHandle, m_width, m_height, textureRowPitch, textureDataPtr);
	LeaveCriticalSection(&CriticalSectionTempBuffer);
}

// the whole buffer is copied for each update
// a good optimisation shoudl eb to copy only the modified part
void UnityTextureHandler::Update()
{
	if (!m_ready)
		return;

	void * textureDataPtr = startModify();
	if (textureDataPtr == NULL)
		return;

	//applyPendingUpdate();
	memcpy(textureDataPtr, tempBuffer, bufferSize);

	endModify(textureDataPtr);
}

void UnityTextureHandler::setColour(int i, int r, int g, int b)
{
	vlog.debug("Should not happens today (indexed colors ???)");
}

void UnityTextureHandler::invalidateRect(const Rect& r)
{
	vlog.debug("TODO : invalidateRect ");
}

void UnityTextureHandler::applyPendingUpdate()
{
	std::list<BufferUpdate *>::iterator i;
	for (i = pendingUpdateList.begin(); i != pendingUpdateList.end(); i++)
	{
		ApplyBufferUpdate(*i);
		delete *i;
	}
}

void UnityTextureHandler::ApplyBufferUpdate(BufferUpdate  * pUpdate)
{
	EnterCriticalSection(&CriticalSectionTempBuffer);
	
	pUpdate->apply(tempBuffer, textureRowPitch);
	LeaveCriticalSection(&CriticalSectionTempBuffer);
}

void UnityTextureHandler::addUpdate(BufferUpdate * pUpdate)
{
	if (!m_ready)
	{
		pendingUpdateList.push_back(pUpdate);
	}
	else
	{
		ApplyBufferUpdate(pUpdate);
		delete pUpdate;
	}
}

void UnityTextureHandler::fillRect(const Rect& r, Pixel pix)
{
	addUpdate(new BufferUpdate(r, pix));
}

void UnityTextureHandler::imageRect(const Rect& r, void* pixels)
{
	addUpdate(new BufferUpdate(r, (U8 *) pixels));
}

void UnityTextureHandler::copyRect(const Rect& r, int srcX, int srcY)
{
	addUpdate(new BufferUpdate(r, srcX, srcY));
}

