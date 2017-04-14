#ifndef __VNC_CLIENT
#define __VNC_CLIENT

#include "UnityTextureHandler.h"
#include "ConnectionThread.h"

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

			inline UnityTextureHandler * getTextureHandler() { return m_pTexture; }

			int GetWidth();
			int GetHeight();	

			ConnectionState GetConnectionState();

			void Update();

		protected:
			void setConnectionState(ConnectionState state);
			void stopConnectionThread();
			ConnectionState m_connectionState = Iddle;
			ConnectionThread * m_ConnectionThread;
			UnityTextureHandler * m_pTexture;

			friend class ConnectionThread;
			friend class CConn;
		};
	}
}

#endif // __VNC_CLIENT