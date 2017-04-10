#include "VNCClient.h"

#include <assert.h>
#include <math.h>
#include <vector>
#include <windows.h>


#include "UnityLog.h"

using namespace rfb::unity;

VNCClient::VNCClient()
{
	setConnectionState(Iddle);
	m_ConnectionThread = NULL;
	texture = new UnityTextureHandler();
}

VNCClient::~VNCClient()
{
	if (texture != NULL)
		delete texture;

	stopConnectionThread();
}

void VNCClient::Connect(const char * host, int port)
{
	UNITYLOG("Connect %s:%d", host, port);
	stopConnectionThread();
		
	m_ConnectionThread = new ConnectionThread();
	m_ConnectionThread->Connect(this, host, port);
}

void VNCClient::Update()
{
	if (texture != NULL)
	{
		texture->Update();
	}
}

ConnectionState VNCClient::GetConnectionState()
{
	return m_connectionState;
}

void VNCClient::Disconnect()
{
	stopConnectionThread();
}

int VNCClient::GetWidth()
{
	if (texture == NULL)
	{
		UNITYLOG("Error no texture");
		return -1;
	}
	return texture->width();
}

int VNCClient::GetHeight()
{
	if (texture == NULL)
	{
		UNITYLOG("Error no texture");
		return -1;
	}
	return texture->height();
}

bool VNCClient::NeedPassword()
{
	return false;
}

void VNCClient::stopConnectionThread()
{
	if (m_ConnectionThread != NULL)
	{
		m_ConnectionThread->QuitAndDelete();
	}
	m_ConnectionThread = NULL;
}

void VNCClient::setConnectionState(ConnectionState state)
{
	m_connectionState = state;
	switch (state)
	{
	case Iddle:
		UNITYLOG("setConnectionState Iddle");
		break;
	case Connecting:
		UNITYLOG("setConnectionState Connecting");
		break;
	case Connected:
		UNITYLOG("setConnectionState Connected");
		stopConnectionThread();
		break;
	case Error:
		UNITYLOG("setConnectionState Error");
		stopConnectionThread();
		break;
	default:
		break;
	}
}