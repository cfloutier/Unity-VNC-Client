#ifndef __UNITYTEXTUREHANDLER_
#define __UNITYTEXTUREHANDLER_

#include "UnityPlugin/PlatformBase.h"
#include "UnityPlugin/RenderAPI.h"

#include <rfb/Threading.h>
#include <rfb/PixelFormat.h>
#include <rfb/Rect.h>
using namespace rdr;

namespace rfb
{
	namespace unity {

		class UnityTextureHandler : Thread
		{
		public:
			UnityTextureHandler();
			~UnityTextureHandler();

			void setTextureSize(int Width, int Height);
			// build from Unity 
			void build(void * handle,
				RenderAPI* CurrentAPI,
				UnityGfxRenderer DeviceType,
				IUnityInterfaces* UnityInterfaces,
				IUnityGraphics* Graphics);

			void Update();

			int width() { return m_width; }
			int height() { return m_height; }

			void fillRect(const Rect& r, Pixel pix);
			void imageRect(const Rect& r, void* pixels);
			void copyRect(const Rect& r, int srcX, int srcY);

			void setColour(int i, int r, int g, int b);
			void setSize(int width, int height);
			PixelFormat getPF() { return m_pixelFormat; };
			U8* getPixelsRW(Rect r, int * stride);

			void run();

			bool isTextureReady()
			{
				return m_ready;
			}


		protected:
			unsigned char* tempBuffer;

			bool m_ready = false;

			bool exitThread = false;
			bool threadIsRunning = false;
			CRITICAL_SECTION CriticalSection;

			void Sinuses();
			void Noise();
			void* startModify();
			void endModify(void * textureHandle);

			void*	m_TextureHandle = 0;
			int m_width = 256;
			int m_height = 256;
			int textureRowPitch;
			int bufferSize;

			RenderAPI* m_CurrentAPI = NULL;
			UnityGfxRenderer m_DeviceType = kUnityGfxRendererNull;
			IUnityInterfaces* m_UnityInterfaces = NULL;
			IUnityGraphics* m_Graphics = NULL;

			PixelFormat m_pixelFormat;
		};
	}
}

#endif // __VNC_CLIENT