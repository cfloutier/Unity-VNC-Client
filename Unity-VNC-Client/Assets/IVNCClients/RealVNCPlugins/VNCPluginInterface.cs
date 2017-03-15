using UnityEngine;
using System;
using System.Runtime.InteropServices;
using VNCScreen.Drawing;


public class VNCPluginInterface
{
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
        Connecting,
        Connected,
        Error      
    }


#if (UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("RealVNCPlugin")]
#endif
    public static extern int getConnectionState();


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
    public static extern bool NeedPassword();

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

    public void Init()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
		RegisterPlugin();
#endif
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
