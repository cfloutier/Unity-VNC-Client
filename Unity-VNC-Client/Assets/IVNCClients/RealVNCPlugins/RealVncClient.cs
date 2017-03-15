using UnityEngine;
using VNCScreen;
using VNCScreen.Drawing;
using System;
using System.Threading;

public class RealVncClient : IVncClient
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

    Thread connectingThread;            // To get the connecting state 


    public void Connect(string host, int display, int port, bool viewOnly)
    {
        connectionInfos = new ConnectionOptions();
        connectionInfos.host = host;
        connectionInfos.display = display;
        connectionInfos.port = port;
        connectionInfos.viewOnly = viewOnly;

        if (connectingThread != null && connectingThread.IsAlive)
        {
            connectingThread.Abort();
        }

        connectingThread = new Thread(new ThreadStart(this.ConnectionThread));
        connectingThread.SetApartmentState(ApartmentState.STA);
        connectingThread.IsBackground = true;
        connectingThread.Start();
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

    private void ConnectionThread()
    {
        VNCPluginInterface.Connect(connectionInfos.host, connectionInfos.port, connectionInfos.display, connectionInfos.viewOnly);
        try
        {
            while (true)
            {
                bool exitThread = false;
                state = VNCPluginInterface.getEnumConnectionState();
                switch (state)
                {
                    case VNCPluginInterface.ConnectionState.Iddle:
                        break;
                    case VNCPluginInterface.ConnectionState.Connecting:
                        break;
                    case VNCPluginInterface.ConnectionState.Connected:

                        TextureSize = new Size(
                            VNCPluginInterface.getDesktopWidth(),
                            VNCPluginInterface.getDesktopHeight());

                        bool needPassword = VNCPluginInterface.NeedPassword();
                        onConnection(null, needPassword);
                        exitThread = true;
                        break;
                    case VNCPluginInterface.ConnectionState.Error:
                        Debug.LogError("Connection Error");
                        exitThread = true;
                        break;
                }

                if (exitThread)
                    break;

                Thread.Sleep(300);
            }
        }
        catch (Exception e)
        {
            if (!(e is ThreadAbortException))
            {
                onConnection(e, false);
            }
        }
    }

    public bool Authenticate(string password)
    {
        return true;
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
        texture = pluginInterface.CreateTextureAndPassToPlugin(BufferSize);
    }

    public void Disconnect()
    {

    }

    public void UpdateMouse(Point pos, bool button0, bool button1, bool button2)
    {

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
        return texture;
    }
}
