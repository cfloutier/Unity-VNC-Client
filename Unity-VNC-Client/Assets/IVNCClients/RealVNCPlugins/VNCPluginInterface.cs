using UnityEngine;
using System;
using System.Runtime.InteropServices;
using VNCScreen.Drawing;


public class VNCPluginInterface
{

#if UNITY_WEBGL && !UNITY_EDITOR
	[DllImport ("__Internal")]
	private static extern void RegisterPlugin();
#endif

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RealVNCPlugin")]
#endif
    public static extern void Connect(string host, int display, int port, bool viewOnly);




#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RealVNCPlugin")]
#endif
    private static extern void Disconnect();

    public enum ConnectionState
    {
        Iddle,
        Connecting,  // connection has been asked, waiting for response
        PasswordNeeded, // if password is needed, it must be send by unity part
        BufferSizeChanged, // when connection is validated, the buffer size is known, unity should send a new buffer
                           // this state can also be set when the distant screen size changes
        Connected,  // everything is ok, update can be called 
        Error		// an error occured
    }


#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RealVNCPlugin")]
#endif
    public static extern int getConnectionState();

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RealVNCPlugin")]
#endif
    private static extern bool GetDebugLog(IntPtr log, int size);

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RealVNCPlugin")]
#endif
    public static extern void MouseEvent(int x, int y, bool button0, bool button1, bool button2);

    private GCHandle m_LogHandle;

    public static ConnectionState getEnumConnectionState()
    {
        return (ConnectionState) getConnectionState();
    }

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RealVNCPlugin")]
#endif
    public static extern int getDesktopWidth();

#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RealVNCPlugin")]
#endif
    public static extern int getDesktopHeight();
   

    // We'll also pass native pointer to a texture in Unity.
    // The plugin will fill texture data from native code.
#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RealVNCPlugin")]
#endif
    private static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h);


#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RealVNCPlugin")]
#endif
    private static extern IntPtr GetRenderEventFunc();

#if UNITY_WEBGL && !UNITY_EDITOR
	[DllImport ("__Internal")]
	private static extern void RegisterPlugin();
#endif

    private int m_LogBufferSize = 2048;
    private byte[] m_Log;

    public void Init()
    {
        m_Log = new byte[m_LogBufferSize];
        m_LogHandle = GCHandle.Alloc(m_Log, GCHandleType.Pinned);

#if UNITY_WEBGL && !UNITY_EDITOR
		RegisterPlugin();
#endif
    }

    public void Release()
    {
        LogFromPlugin();
        m_LogHandle.Free();
        Disconnect();
    }

    public void LogFromPlugin()
    {
        while (GetDebugLog(m_LogHandle.AddrOfPinnedObject(), m_LogBufferSize))
        {
            Debug.Log("[Plugin] - " + System.Text.Encoding.UTF8.GetString(m_Log));
        }
    }

    public Texture2D CreateTextureAndPassToPlugin(Size TextureSize)
    {
        // Create a texture
        Texture2D tex = new Texture2D(TextureSize.Width, TextureSize.Height, TextureFormat.ARGB32, false);
        // Set point filtering just so we can see the pixels clearly
        tex.filterMode = FilterMode.Point;
        // Call Apply() so it's actually uploaded to the GPU
        tex.Apply();

        // Pass texture pointer to the plugin
        SetTextureFromUnity(tex.GetNativeTexturePtr(), tex.width, tex.height);

        return tex;
    }


    public void CallPluginAtEndOfFrames()
    {
        GL.IssuePluginEvent(GetRenderEventFunc(), 1);
    }

}
