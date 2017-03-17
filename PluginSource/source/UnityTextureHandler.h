#ifndef __TEXTURE_MODIFER
#define __TEXTURE_MODIFER

#include "UnityPlugin/PlatformBase.h"
#include "UnityPlugin/RenderAPI.h"

class UnityTextureHandler
{
public:
	UnityTextureHandler();

	void setTextureSize(int Width, int Height);
	void setUnityStuff(void * handle,
		RenderAPI* CurrentAPI,
		UnityGfxRenderer DeviceType,
		IUnityInterfaces* UnityInterfaces,
		IUnityGraphics* Graphics);

	void Update();

	static void OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

	int GetWidth()	{		return width;	}
	int GetHeight(){		return height;	}

protected:
	void Sinuses();
	void Noise();
	void* startModify();
	void endModify(void * textureHandle);

	void*	m_TextureHandle = 0;
	int width = 256;
	int height = 256;
	int textureRowPitch;

	RenderAPI* m_CurrentAPI = NULL;
	UnityGfxRenderer m_DeviceType = kUnityGfxRendererNull;
	IUnityInterfaces* m_UnityInterfaces = NULL;
	IUnityGraphics* m_Graphics = NULL;

};

#endif // __VNC_CLIENT