#include "UnityTextureHandler.h"

#include <assert.h>
#include <math.h>
#include <vector>
#include "windows.h"

#include "UnityLog.h"

using namespace rfb::unity;

UnityTextureHandler::UnityTextureHandler()
{
	m_width = m_height = 512;
	tempBuffer = NULL;
	InitializeCriticalSection(&CriticalSection);
}

UnityTextureHandler::~UnityTextureHandler()
{
	exitThread = true;

	while (threadIsRunning)
		Sleep(5);

	DeleteCriticalSection(&CriticalSection);

	if (tempBuffer != NULL) delete[] tempBuffer;
}

void UnityTextureHandler::run()
{
	UNITYLOG("Start Rendering Thread");
	threadIsRunning = true;
	while (!exitThread)
	{
		EnterCriticalSection(&CriticalSection);

		Sinuses();

		LeaveCriticalSection(&CriticalSection);

		Sleep(5);
	}


	UNITYLOG("End Rendering Thread");

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

	if (tempBuffer != NULL) delete[] tempBuffer;
	bufferSize = textureRowPitch*m_height;
	tempBuffer = new unsigned char[bufferSize];

	// start the main Thread
	start();
}



float g_startTime = -1;
void UnityTextureHandler::Sinuses()
{
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

	EnterCriticalSection(&CriticalSection);
	return textureDataPtr;
}


void UnityTextureHandler::endModify(void * textureDataPtr)
{
	m_CurrentAPI->EndModifyTexture(m_TextureHandle, m_width, m_height, textureRowPitch, textureDataPtr);
	LeaveCriticalSection(&CriticalSection);
}

void UnityTextureHandler::Noise()
{
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
}

void UnityTextureHandler::Update()
{
	void * textureDataPtr = startModify();
	if (textureDataPtr == NULL)
		return;

	memcpy(textureDataPtr, tempBuffer, bufferSize);

	endModify(textureDataPtr);
}
