#ifndef DEBUG_LOG_
#define DEBUG_LOG_

#define UNITYLOG DebugLog::Log

class DebugLog
{
public :
	static void Init();
	static void Release();
	static void Log(const char * format, ...);
	static void Clear();

};

#endif
