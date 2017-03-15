#ifndef __TEXTURE_MODIFER
#define __TEXTURE_MODIFER

#include "UnityPlugin/PlatformBase.h"
#include "UnityPlugin/RenderAPI.h"

class TextureModifer
{
public :
	TextureModifer(void * handle,

		RenderAPI* CurrentAPI ,
	UnityGfxRenderer DeviceType ,
	IUnityInterfaces* UnityInterfaces,
	IUnityGraphics* Graphics );

	void Update();

	static void OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

protected:
	void randomUpdate();


	void*	m_TextureHandle = 0;

	RenderAPI* m_CurrentAPI = NULL;
	UnityGfxRenderer m_DeviceType = kUnityGfxRendererNull;
	IUnityInterfaces* m_UnityInterfaces = NULL;
	IUnityGraphics* m_Graphics = NULL;



};

#endif // __VNC_CLIENT