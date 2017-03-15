#include "VNCClient.h"
#include <assert.h>
#include <math.h>
#include <vector>
#include "windows.h"


VNCClient::VNCClient()
{

}

void VNCClient::Connect(char * host, int port)
{
	m_host = host;
	m_port = port;
}

void VNCClient::Update()
{
	if (modifier != NULL)
	{
		modifier->Update();
	}
}

ConnectionState VNCClient::GetConnectionState()
{
	return m_connectionState;
}

void VNCClient::Disconnect()
{
	delete this;
}

void VNCClient::InitTextureHandler(
	void* textureHandle,
	RenderAPI* CurrentAPI ,
	UnityGfxRenderer DeviceType ,
	IUnityInterfaces* UnityInterfaces ,
	IUnityGraphics* Graphics 	)
{
	modifier = new TextureModifer(textureHandle, 
		CurrentAPI,
		DeviceType,
		UnityInterfaces,
		Graphics);
}

int VNCClient::GetWidth()
{
	return m_TextureWidth;
}

int VNCClient::GetHeight()
{
	return m_TextureHeight;
}

  