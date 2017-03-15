#ifndef __VNC_CLIENT
#define __VNC_CLIENT

#include "TextureModifer.h"

enum ConnectionState
{
	Iddle,
	Connecting,
	Connected,
	Error
};

class VNCClient 
{
public :
	VNCClient();

	void Connect(char * host, int port);
	void Disconnect();

	void InitTextureHandler(void* textureHandle,
		RenderAPI* s_CurrentAPI,
		UnityGfxRenderer s_DeviceType,
		IUnityInterfaces* s_UnityInterfaces,
		IUnityGraphics* s_Graphics);

	int GetWidth();
	int GetHeight();
	ConnectionState GetConnectionState();
	TextureModifer * modifier;

	void Update();

protected:
	ConnectionState m_connectionState = Iddle;
	

	int		m_TextureWidth = 0;

	int		m_TextureHeight = 0;

	char *	m_host;
	int		m_port;
};

#endif // __VNC_CLIENT