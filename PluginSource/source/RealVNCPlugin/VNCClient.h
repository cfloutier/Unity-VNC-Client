#ifndef __VNC_CLIENT
#define __VNC_CLIENT

#include "UnityTextureHandler.h"
#include "ConnectionThread.h"
#include <rfb_win32/CPointer.h>

using namespace rfb::win32;


namespace rfb
{
	namespace unity 
	{
		enum ConnectionState
		{
			Iddle,  
			Connecting,  // connection has been asked, waiting for response
			PasswordNeeded, // if password is needed, it must be send by unity part
			BufferSizeChanged, // when connection is validated, the buffer size is known, unity should send a new buffer
								// this state can also be set when the distant screen size changes
			Connected,	// everything is ok, update can be called 
			Error		// an error occured
		};

		class VNCClient
		{
		public:
			VNCClient();
			~VNCClient();

			void Connect(const char * host, int port);
			void onTextureBuilt();
			void Disconnect();

			void MouseEvent(int x, int y, bool bt0, bool bt1, bool bt2);

			inline UnityTextureHandler * getTextureHandler() { return m_pTexture; }

			int GetWidth();
			int GetHeight();	

			ConnectionState GetConnectionState();

			void Update();

			void setSize(int x, int y);
			void serverCutText(const char* str, int len);

		protected:
			// for mouse positions
			int lastMask = -1;
			int lastX = -1;
			int lastY = -1;

			void setConnectionState(ConnectionState state);
			void stopConnectionThread();
			ConnectionState m_connectionState = Iddle;
			ConnectionThread * m_ConnectionThread;
			UnityTextureHandler * m_pTexture;
			PluginConnection *  m_pCurrentConnection;

			rfb::win32::CPointer ptr;


			friend class ConnectionThread;
			friend class PluginConnection;
		};
	}
}

#endif // __VNC_CLIENT