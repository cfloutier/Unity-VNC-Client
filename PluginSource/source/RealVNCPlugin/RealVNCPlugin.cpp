// Example low level rendering Unity plugin

#include "UnityPlugin/PlatformBase.h"
#include "UnityPlugin/RenderAPI.h"

#include <assert.h>
#include <math.h>
#include <vector>
#include "windows.h"
#include "VNCClient.h"

#include "UnityLog.h"
#include <rfb/LogWriter.h>
#include "UnityTextureHandler.h"

using namespace rfb::unity;
using namespace rfb;

// --------------------------------------------------------------------------
VNCClient * pClient;

static RenderAPI* s_CurrentAPI = NULL;
static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;
static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;

// once the texture size is known unity should call the texture handler to really build the texture in the graphic card memory
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetTextureFromUnity(void* textureHandle)
{
	pClient->getTextureHandler()->build(textureHandle, s_CurrentAPI, s_DeviceType, s_UnityInterfaces, s_Graphics);
	pClient->onTextureBuilt();
}


// once the texture size is known unity should call the texture handler to really build the texture in the graphic card memory
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API RequestScreenUpdate()
{
	if (pClient != 0)
	{
		pClient->RequestScreenUpdate();
	}
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

	LogWriter::setLogParams("PluginConnection:SimpleLogger:110");
//	LogWriter::setLogParams("UnityTextureHandler:SimpleLogger:110");
//	LogWriter::setLogParams("BufferUpdate:SimpleLogger:110");
//	LogWriter::setLogParams("CConn:SimpleLogger:110");
	  
//	LogWriter::setLogParams("VNCClient:UnityDebugLogger:110");
//	LogWriter::setLogParams("UnityTextureHandler:UnityDebugLogger:110");

	
//	LogWriter::setLogParams("VNCClient:UnityDebugLogger:110");
//	LogWriter::setLogParams("DesktopWindow:UnityDebugLogger:110");
//	LogWriter::setLogParams("Clipboard:UnityDebugLogger:110");
//	LogWriter::setLogParams("MsgWindow:UnityDebugLogger:110");
//	LogWriter::setLogParams("CConnection:UnityDebugLogger:110");
	
	//LogWriter::setLogParams("*:UnityDebugLogger:110");
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	if (pClient != 0)
		pClient->Update();
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
	if (pClient != NULL)
	{
		pClient->Disconnect();
		delete pClient;
		pClient = NULL;
	}

	UnityLog::Clear();
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API MouseEvent(int x, int y, bool button0, bool button1, bool button2)
{
	if (pClient)
		pClient->MouseEvent(x, y, button0, button1, button2);
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
	if (pClient != NULL)
		Disconnect();

	pClient = new VNCClient();
	pClient->Connect(host, port);
}

extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API getDesktopWidth()
{
	if (pClient == NULL)
	{
		UNITYLOG("No VNC CLient !!");
		return 0;
	}
	return pClient->GetWidth();
}

extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API getDesktopHeight()
{
	if (pClient == NULL)
	{
		UNITYLOG("No VNC CLient !!");
		return 0;
	}
	return pClient->GetHeight();
}

extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API  getConnectionState()
{
	return (int)pClient->GetConnectionState();
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Authenticate(char * password)
{

}

extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API IsConnected()
{
	return false;
}
