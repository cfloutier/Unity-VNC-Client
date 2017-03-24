#include "ConnectionThread.h"
#include "DebugLog.h"
#include "VNCClient.h"
using namespace rfb;
using namespace win32;

ConnectionThread::ConnectionThread() : Thread("Fake Connecting thread")
{

}

void ConnectionThread::QuitAndDelete()
{
	exit = true;
	setDeleteAfterRun();
}

void ConnectionThread::run()
{
	if (m_client == NULL)
		return;

	m_client->setConnectionState(Connecting);
	for (int i = 0; i < 10; i++)
	{
		UNITYLOG("ConnectionThread %d", i);

		if (exit)
			break;

		Sleep(100);
	}

	m_client->setConnectionState(Connected);
}

void ConnectionThread::Connect(VNCClient * client,  const char* host, int port)
{
	m_host = host;
	m_port = port;
	m_client = client;
	start();
}


