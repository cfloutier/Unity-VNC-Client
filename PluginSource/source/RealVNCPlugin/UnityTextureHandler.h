#ifndef __TEXTURE_MODIFER
#define __TEXTURE_MODIFER

#include "UnityPlugin/PlatformBase.h"
#include "UnityPlugin/RenderAPI.h"

#include "rfb/Threading.h"

using namespace rfb;

class UnityTextureHandler : Thread
{
public:
	UnityTextureHandler();
	~UnityTextureHandler();

	void setTextureSize(int Width, int Height);
	void build(void * handle,
		RenderAPI* CurrentAPI,
		UnityGfxRenderer DeviceType,
		IUnityInterfaces* UnityInterfaces,
		IUnityGraphics* Graphics);

	void Update();

	int GetWidth()	{		return width;	}
	int GetHeight(){		return height;	}

	void run();

protected:
	unsigned char* tempBuffer;

	bool exitThread = false;
	bool threadIsRunning = false;
	CRITICAL_SECTION CriticalSection;

	void Sinuses();
	void Noise();
	void* startModify();
	void endModify(void * textureHandle);

	void*	m_TextureHandle = 0;
	int width = 256;
	int height = 256;
	int textureRowPitch;
	int bufferSize;

	RenderAPI* m_CurrentAPI = NULL;
	UnityGfxRenderer m_DeviceType = kUnityGfxRendererNull;
	IUnityInterfaces* m_UnityInterfaces = NULL;
	IUnityGraphics* m_Graphics = NULL;

};

#endif // __VNC_CLIENT