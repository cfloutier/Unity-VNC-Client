#include "TextureModifer.h";

#include <assert.h>
#include <math.h>
#include <vector>
#include "windows.h"

TextureModifer::TextureModifer(
	void * handle,
	RenderAPI* CurrentAPI,
	UnityGfxRenderer DeviceType,
	IUnityInterfaces* UnityInterfaces,
	IUnityGraphics* Graphics)
{
	m_TextureHandle = handle;
}

float g_startTime = -1;
void TextureModifer::randomUpdate()
{
	// Unknown / unsupported graphics device type? Do nothing
	if (m_CurrentAPI == NULL)
		return;

	void* textureHandle = m_TextureHandle;
	int width = 256;
	int height = 256;
	if (!textureHandle)
		return;

	int textureRowPitch;
	void* textureDataPtr = m_CurrentAPI->BeginModifyTexture(textureHandle, width, height, &textureRowPitch);
	if (!textureDataPtr)
		return;

	float g_Time = (float)GetTickCount() / 1000;
	if (g_startTime == -1)
		g_startTime = g_Time;
	g_Time = g_Time - g_startTime;
	const float t = g_Time * 4.0f;

	unsigned char* dst = (unsigned char*)textureDataPtr;
	for (int y = 0; y < height; ++y)
	{
		unsigned char* ptr = dst;
		for (int x = 0; x < width; ++x)
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

	m_CurrentAPI->EndModifyTexture(textureHandle, width, height, textureRowPitch, textureDataPtr);
}


void TextureModifer::Update()
{
	randomUpdate();
}
