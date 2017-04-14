#ifndef __CONNECTIONTHREAD_
#define __CONNECTIONTHREAD_

#include <rfb_win32/Threading.h>

namespace rfb
{
	namespace unity {

		class VNCClient;
		class CConn;
		class ConnectionThread : public Thread
		{
		public:
			ConnectionThread();

			void Connect(VNCClient *client, const char* host, int port);

			void QuitAndDelete();

		protected:

			void run();

			char* m_host;
			int m_port;
			VNCClient *m_client;

			friend class VNCClient;
			CConn * m_pCurrentConnection;
		};
	}
}
#endif // __CONNECTIONTHREAD_