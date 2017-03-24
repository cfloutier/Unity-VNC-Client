#ifndef __FAKE_THREAD
#define __FAKE_THREAD

#include <rfb_win32/Threading.h>

using namespace rfb;

class VNCClient;
class ConnectionThread : public Thread
{
public :
	ConnectionThread();

	void Connect(VNCClient *client, const char* host, int port);
	
	void QuitAndDelete();

protected:
	
	void run(); 

	const char* m_host;
	int m_port;
	VNCClient *m_client;

	bool exit = false;
};
 
#endif