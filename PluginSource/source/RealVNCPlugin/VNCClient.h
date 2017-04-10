#ifndef __VNC_CLIENT
#define __VNC_CLIENT

#include "UnityTextureHandler.h"
#include "ConnectionThread.h"

namespace rfb
{
	namespace unity {
		enum ConnectionState
		{
			Iddle,
			Connecting,
			Connected,
			Error
		};

		class VNCClient
		{
		public:
			VNCClient();
			~VNCClient();

			void Connect(const char * host, int port);
			void Disconnect();

			inline UnityTextureHandler * getTextureHandler() { return texture; }

			int GetWidth();
			int GetHeight();
			bool NeedPassword();
			ConnectionState GetConnectionState();


			void Update();

		protected:
			void setConnectionState(ConnectionState state);
			void stopConnectionThread();
			ConnectionState m_connectionState = Iddle;
			ConnectionThread * m_ConnectionThread;
			UnityTextureHandler * texture;

			friend class ConnectionThread;
		};
	}
}

#endif // __VNC_CLIENT