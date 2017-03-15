#ifndef __FAKE_THREAD
#define __FAKE_THREAD

#include <rfb_win32/Threading.h>

using namespace rfb;

class ConnectionThread : public Thread
{
public :
	ConnectionThread();

	void run();
};

#endif