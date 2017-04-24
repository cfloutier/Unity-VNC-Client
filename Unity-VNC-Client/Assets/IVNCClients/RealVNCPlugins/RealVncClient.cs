using UnityEngine;


using VNCScreen;
using VNCScreen.Drawing;
using System;
using System.Collections;
using System.Threading;

public class RealVncClient : MonoBehaviour, IVncClient
{
    VNCPluginInterface pluginInterface = null;

    public RealVncClient()
    {
        pluginInterface = new VNCPluginInterface();
    }

    Size TextureSize = new Size(256, 256);

    /// <summary>
    /// Raised when the connection to the remote host is lost.
    /// </summary>
    public event EventHandler ConnectionLost;

    /// <summary>
    /// Raised when the connection to the remote host is set or not
    /// </summary>
    public event OnConnection onConnection;


    public void Connect(string host, int display, int port, bool viewOnly)
    {
        connectionInfos = new ConnectionOptions();
        connectionInfos.host = host;
        connectionInfos.display = display;
        connectionInfos.port = port;
        connectionInfos.viewOnly = viewOnly;

        StartCoroutine(Connection());
    }

    class ConnectionOptions
    {
        public string host;
        public int display;
        public int port;
        public bool viewOnly;
    }

    ConnectionOptions connectionInfos;
    VNCPluginInterface.ConnectionState state;
    bool needNewtexture = true;
    private IEnumerator Connection()
    {
        VNCPluginInterface.Connect(connectionInfos.host, connectionInfos.port, connectionInfos.display, connectionInfos.viewOnly);

        while (true)
        {
            bool exit = false;
            try
            {
                state = VNCPluginInterface.getEnumConnectionState();
                switch (state)
                {
                    case VNCPluginInterface.ConnectionState.Iddle:
                        break;
                    case VNCPluginInterface.ConnectionState.Connecting:

                        break;
                    case VNCPluginInterface.ConnectionState.PasswordNeeded:
                        onConnection(null, true);
                        break;

                    case VNCPluginInterface.ConnectionState.Connected:
                        onConnection(null, false);
                        exit = true;
                        break;

                    case VNCPluginInterface.ConnectionState.BufferSizeChanged:
                        onConnection(null, false);
                        needNewtexture = true;
                        TextureSize = new Size(
                            VNCPluginInterface.getDesktopWidth(),
                            VNCPluginInterface.getDesktopHeight());
                        break;

                    case VNCPluginInterface.ConnectionState.Error:
                        if (ConnectionLost != null)
                            ConnectionLost(this, new ErrorEventArg("Error From plugin"));

                        Debug.LogError("Connection Error");
                        exit = true;
                        break;
                }
            }
            catch (Exception e)
            {
                if (!(e is ThreadAbortException))
                {
                    onConnection(e, false);
                }
            }
            if (exit)
                break;

            yield return new WaitForSeconds(0.25f);
        }
    }

    public void Start()
    {
        pluginInterface.Init();
    }

    public void Update()
    {
        pluginInterface.LogFromPlugin();
    }

    void OnApplicationQuit()
    {
        pluginInterface.Release();
    }

    public void Authenticate(string password, OnPassword onPassword)
    {
        onPassword(true);
    }

    public void RequestScreenUpdate(bool refreshFullScreen)
    {

    }

    public void Initialize()
    {


    }

    public Size BufferSize
    {
        get
        {
            return TextureSize;
        }
    }

    Texture2D texture;

    public void StartUpdates()
    {


    }

    public void Disconnect()
    {

    }

    public void UpdateMouse(Point pos, bool button0, bool button1, bool button2)
    {
        VNCPluginInterface.MouseEvent(pos.X, pos.Y, button0, button1, button2);
    }

    public void WriteKeyboardEvent(uint keysym, bool pressed)
    {

    }

    /// <summary>
    /// Update the Desktop Image 
    /// </summary>
    public bool updateDesktopImage()
    {
        pluginInterface.CallPluginAtEndOfFrames();
        return true;
    }

    public Texture2D getTexture()
    {
        state = VNCPluginInterface.getEnumConnectionState();
        switch (state)
        {
            case VNCPluginInterface.ConnectionState.Connected:
                needNewtexture = false;
                break;

            case VNCPluginInterface.ConnectionState.BufferSizeChanged:
                needNewtexture = true;
                TextureSize = new Size(
                    VNCPluginInterface.getDesktopWidth(),
                    VNCPluginInterface.getDesktopHeight());
                break;
            
            case VNCPluginInterface.ConnectionState.Error:
                if (ConnectionLost != null)
                    ConnectionLost(this, new ErrorEventArg("Error From plugin"));
                break;
            default:
                Debug.LogWarning("Stange behaviour");
                break;
        }

        if (needNewtexture)
        {
            needNewtexture = false;
            texture = pluginInterface.CreateTextureAndPassToPlugin(BufferSize);
        }


        return texture;
    }
}
