#ifndef DEBUG_LOG_
#define DEBUG_LOG_

#define UNITYLOG UnityLog::Log

#include <time.h>
#include <rfb/Logger.h>

using namespace rfb;

class UnityLog
{
public :
	static void Init();
	static void Release();
	static void Log(const char * format, ...);
	static void Clear();
};

class UnityDebugLogger : public Logger
{
public:
	UnityDebugLogger();

	virtual void write(int level, const char *logname, const char *message);
};

#endif
