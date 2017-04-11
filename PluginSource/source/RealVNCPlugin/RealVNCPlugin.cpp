// Example low level rendering Unity plugin

#include "UnityPlugin/PlatformBase.h"
#include "UnityPlugin/RenderAPI.h"

#include <assert.h>
#include <math.h>
#include <vector>
#include "windows.h"
#include "VNCClient.h"

#include "UnityLog.h"
#include "UnityTextureHandler.h"

using namespace rfb::unity;

// --------------------------------------------------------------------------
VNCClient * client;

static RenderAPI* s_CurrentAPI = NULL;
static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;
static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;

// once the texture size is known unity should call the texture handler to really build the texture in the graphic card memory
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetTextureFromUnity(void* textureHandle)
{
	client->getTextureHandler()->build(textureHandle, s_CurrentAPI, s_DeviceType, s_UnityInterfaces, s_Graphics);
}


// --------------------------------------------------------------------------
// UnitySetInterfaces
static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);


// called when the plugin is loaded (once the play button is first pressed in unity)
// Unity plugin load event
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	s_UnityInterfaces = unityInterfaces;
	s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
	s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

	// Run OnGraphicsDeviceEvent(initialize) manually on plugin load
	OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);

	UnityLog::Init();
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	if (client != 0)
		client->Update();
}

// --------------------------------------------------------------------------
// GetRenderEventFunc, an example function we export which is used to get a rendering event callback function.
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
	return OnRenderEvent;
}

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
	// Create graphics API implementation upon initialization
	if (eventType == kUnityGfxDeviceEventInitialize)
	{
		assert(s_CurrentAPI == NULL);
		s_DeviceType = s_Graphics->GetRenderer();
		s_CurrentAPI = CreateRenderAPI(s_DeviceType);
	}

	// Let the implementation process the device related events
	if (s_CurrentAPI)
	{
		s_CurrentAPI->ProcessDeviceEvent(eventType, s_UnityInterfaces);
	}

	// Cleanup graphics API implementation upon shutdown
	if (eventType == kUnityGfxDeviceEventShutdown)
	{
		delete s_CurrentAPI;
		s_CurrentAPI = NULL;
		s_DeviceType = kUnityGfxRendererNull;
	}
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Disconnect()
{
	if (client != NULL)
	{
		client->Disconnect();
		delete client;
		client = NULL;
	}

	UnityLog::Clear();
}


// the plugin is released, called only in the editor when the editor is closed or the app is closed.
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
	UnityLog::Release();
	Disconnect();
	s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}


// VNC Connection, the client will asked for 
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Connect(char * host, int port)
{
	if (client != NULL)
		Disconnect();

	client = new VNCClient();
	client->Connect(host, port);
}

extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API getDesktopWidth()
{
	if (client == NULL)
	{
		UNITYLOG("No VNC CLient !!");
		return 0;
	}
	return client->GetWidth();
}

extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API getDesktopHeight()
{
	if (client == NULL)
	{
		UNITYLOG("No VNC CLient !!");
		return 0;
	}
	return client->GetHeight();
}

extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API  getConnectionState()
{
	return (int)client->GetConnectionState();
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Authenticate(char * password)
{

}

extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API IsConnected()
{
	return false;
}
