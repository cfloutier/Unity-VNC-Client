#include "DebugLog.h"

#include "stdio.h"
#include <list>
#include <string>
#include <windows.h>

#include "UnityPlugin/PlatformBase.h"
#include "UnityPlugin/RenderAPI.h"

using namespace std;

list<string> logs;
list<string>::const_iterator logs_iterator;

// Global variable
CRITICAL_SECTION CriticalSection;



void Debug::Log(const char * format, ...)
{
	char logString[1024];
	va_list args;
	va_start(args, format);
	vsnprintf(logString, 1024, format, args);
	// Request ownership of the critical section.
	EnterCriticalSection(&CriticalSection);

	logs.push_back(std::string(logString, logString + strlen(logString)));

	// Release ownership of the critical section.
	LeaveCriticalSection(&CriticalSection);

}

// --------------------------------------------------------------------------
// GetDebugLog, to access to the oldest log
extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetDebugLog(void* log, int size)
{
	if (logs.size() > 0)
	{
		// Request ownership of the critical section.
		EnterCriticalSection(&CriticalSection);

		string res = logs.front();
		logs.pop_front();
		// Release ownership of the critical section.
		LeaveCriticalSection(&CriticalSection);
		memset(log, 0, size);
		memcpy_s(log, size, res.c_str(), res.length());
		return true;
	}
	return false;
}


// --------------------------------------------------------------------------
// UnitySetInterfaces

void Debug::Init()
{


	if (!InitializeCriticalSectionAndSpinCount(&CriticalSection,
		0x00000400))
		return;

}

void Debug::Release()
{

	// Release resources used by the critical section object.
	DeleteCriticalSection(&CriticalSection);
}







