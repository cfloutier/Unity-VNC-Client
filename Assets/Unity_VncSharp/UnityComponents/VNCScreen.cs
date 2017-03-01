using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityVncSharp.Drawing;
using UnityVncSharp.Drawing.Imaging;

namespace UnityVncSharp.Unity
{
    /// <summary>
    /// SpecialKeys is a list of the various keyboard combinations that overlap with the client-side and make it
    /// difficult to send remotely.  These values are used in conjunction with the SendSpecialKeys method.
    /// </summary>
    public enum SpecialKeys
    {
        CtrlAltDel,
        AltF4,
        CtrlEsc,
        Ctrl,
        Alt
    }



    public class VNCScreen : MonoBehaviour
    {
        public string host;
        public int port = 5900;
        public bool viewOnly = false;
        public int display = 0;
        public string password;

        private Size screenSize;

        public bool connectOnStartUp;
        bool passwordPending = false;            // After Connect() is called, a password might be required.
        bool fullScreenRefresh = false;		     // Whether or not to request the entire remote screen be sent.

        private Material m;

        // Use this for initialization
        void Start()
        {
            if (connectOnStartUp)
            {
                Connect();
            }
        }

        Bitmap theBitmap;                          // Internal representation of remote image.
        VncClient vnc;                           // The Client object handling all protocol-level interaction

        private enum RuntimeState
        {
            Disconnected,
            Disconnecting,
            Connected,
            Connecting
        }

        RuntimeState state = RuntimeState.Disconnected;

        /// <summary>
        /// True if the RemoteDesktop is connected and authenticated (if necessary) with a remote VNC Host; otherwise False.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return state == RuntimeState.Connected;
            }
        }

        /// <summary>
        /// Insures the state of the connection to the server, either Connected or Not Connected depending on the value of the connected argument.
        /// </summary>
        /// <param name="connected">True if the connection must be established, otherwise False.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is in the wrong state.</exception>
        private void InsureConnection(bool connected)
        {
            // Grab the name of the calling routine:
            string methodName = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;

            if (connected)
            {
                Debug.Assert(state == RuntimeState.Connected ||
                                                state == RuntimeState.Disconnecting, // special case for Disconnect()
                                                string.Format("RemoteDesktop must be in RuntimeState.Connected before calling {0}.", methodName));
                if (state != RuntimeState.Connected && state != RuntimeState.Disconnecting)
                {
                    throw new InvalidOperationException("RemoteDesktop must be in Connected state before calling methods that require an established connection.");
                }
            }
            else
            { // disconnected
                Debug.Assert(state == RuntimeState.Disconnected,
                                                string.Format("RemoteDesktop must be in RuntimeState.Disconnected before calling {0}.", methodName));
                if (state != RuntimeState.Disconnected && state != RuntimeState.Disconnecting)
                {
                    throw new InvalidOperationException("RemoteDesktop cannot be in Connected state when calling methods that establish a connection.");
                }
            }
        }

        public void Connect()
        {
            // Ignore attempts to use invalid port numbers
            if (port < 1 | port > 65535) port = 5900;
            if (display < 0) display = 0;
            if (host == null) throw new ArgumentNullException("host");

            if (IsConnected)
            {
                Disconnect();
            }

            InsureConnection(false);

            // Start protocol-level handling and determine whether a password is needed
            vnc = new VncClient();
            vnc.ConnectionLost += new EventHandler(OnConnectionLost);

            passwordPending = vnc.Connect(host, display, port, viewOnly);

            if (passwordPending)
            {
                // Server needs a password, so call which ever method is refered to by the GetPassword delegate.


                if (string.IsNullOrEmpty(password))
                {
                    // No password could be obtained (e.g., user clicked Cancel), so stop connecting
                    return;
                }
                else
                {
                    Authenticate(password);
                }
            }
            else
            {
                // No password needed, so go ahead and Initialize here
                Initialize();
            }           
        }

        /// <summary>
        /// Authenticate with the VNC Host using a user supplied password.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="VncSharp.RemoteDesktop.IsConnected" />.</exception>
        /// <exception cref="System.NullReferenceException">Thrown if the password is null.</exception>
        /// <param name="password">The user's password.</param>
        public void Authenticate(string password)
        {
            InsureConnection(false);
            if (!passwordPending) throw new InvalidOperationException("Authentication is only required when Connect() returns True and the VNC Host requires a password.");
            if (password == null) throw new NullReferenceException("password");

            passwordPending = false;  // repeated calls to Authenticate should fail.
            if (vnc.Authenticate(password))
            {
                Initialize();
            }
            else
            {
                OnConnectionLost("Wrong Password");
            }
        }

        void OnConnectionLost(string reason)
        {
            OnConnectionLost(this, new ErrorEventArg(reason));
        }


        /// <summary>
        /// received when a connection lost has been detected, the disconnect function will also call this fucntion
        /// </summary>
        /// <param name="sender">The VncClient object that raised the event.</param>
        /// <param name="e">An empty EventArgs object.</param>
        protected void OnConnectionLost(object sender, EventArgs e)
        {
            // If the remote host dies, and there are attempts to write
            // keyboard/mouse/update notifications, this may get called 
            // many times, and from main or worker thread.
            // Guard against this and invoke Disconnect once.
            if (state == RuntimeState.Connected)
            {
                SetState(RuntimeState.Disconnecting);
                Disconnect();
            }

            if (e is ErrorEventArg)
            {
                var error = e as ErrorEventArg;

                if (error.Exception != null)
                {
                    Debug.LogException(error.Exception);
                }
                else if (!string.IsNullOrEmpty(error.Reason))
                {
                    Debug.Log(error.Reason);
                }
            }
            else
            {
                Debug.Log("VncDesktop_ConnectionLost");
            }
        }

        List<IDesktopUpdater> updates = new List<IDesktopUpdater>();

        public void popUpdates()
        {
            for (int i = 0; i < updates.Count; i++)
            {
                IDesktopUpdater u = updates[i];
                if (u != null)
                    u.Draw(theBitmap);
            }

            updates.Clear();

            if (state == RuntimeState.Connected)
            {
                vnc.RequestScreenUpdate(fullScreenRefresh);

                // Make sure the next screen update is incremental
                fullScreenRefresh = false;
            }
        }
        private void SetState(RuntimeState newState)
        {
            state = newState;
        }

        /// <summary>
        /// Get a complete update of the entire screen from the remote host.
        /// </summary>
        /// <remarks>You should allow users to call FullScreenUpdate in order to correct
        /// corruption of the local image.  This will simply request that the next update be
        /// for the full screen, and not a portion of it.  It will not do the update while
        /// blocking.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not in the Connected state.  See <see cref="VncSharp.RemoteDesktop.IsConnected" />.</exception>
        public void FullScreenUpdate()
        {
            InsureConnection(true);
            fullScreenRefresh = true;
        }


        /// <summary>
        /// Changes the input mode to view-only or interactive.
        /// </summary>
        /// <param name="viewOnly">True if view-only mode is desired (no mouse/keyboard events will be sent).</param>
        public void SetInputMode(bool viewOnly)
        {
            vnc.SetInputMode(viewOnly);
        }

        /// <summary>
        /// After protocol-level initialization and connecting is complete, the local GUI objects have to be set-up, and requests for updates to the remote host begun.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already in the Connected state.  See <see cref="VncSharp.RemoteDesktop.IsConnected" />.</exception>		
        protected void Initialize()
        {
            // Finish protocol handshake with host now that authentication is done.
            InsureConnection(false);
            vnc.Initialize();
            SetState(RuntimeState.Connected);

            InsureConnection(true);

            screenSize = new Size(vnc.BufferInfos.Width, vnc.BufferInfos.Height);
            theBitmap = new Bitmap(screenSize.Width, screenSize.Height);
            
            // Tell the user of this control the necessary info about the desktop in order to setup the display
            // Create a texture
            Texture2D tex = theBitmap.Texture;

            // Set texture onto our material
            m = Instantiate(GetComponent<Renderer>().sharedMaterial) as Material;
            m.mainTexture = tex;
            GetComponent<Renderer>().sharedMaterial = m;

            // Refresh scroll properties
            //AutoScrollMinSize = desktopPolicy.AutoScrollMinSize;

            // Start getting updates from the remote host (vnc.StartUpdates will begin a worker thread).
            vnc.VncUpdate += VncUpdate;
            vnc.StartUpdates();
        }

        public void Disconnect()
        {
            InsureConnection(true);
            vnc.ConnectionLost -= new EventHandler(OnConnectionLost);

            vnc.Disconnect();
            SetState(RuntimeState.Disconnected);
            OnConnectionLost("Disconnected");

            updates.Clear();
        }

        protected void VncUpdate(IDesktopUpdater e)
        {
            updates.Add(e);
        }

      
        // Update is called once per frame
        void Update()
        {
            popUpdates();
            m.mainTexture = theBitmap.Texture;
        }

        void OnApplicationQuit()
        {
            Disconnect();
        }



        public void UpdateMouse(Vector2 pos, bool button0, bool button1, bool button2)
        {
            if (!IsConnected) return;

            Texture2D  t = theBitmap.Texture;
            if (t == null)
                return;

            Point point = new Point((int)(pos.x * t.width), (int)((1 - pos.y) * t.height));

            UpdateMouse(point, button0, button1, button2);
        }


        public void UpdateMouse(Point pos, bool button0, bool button1, bool button2)
        {
            byte mask = 0;

            if (button0) mask += 1;
            if (button1) mask += 2;
            if (button2) mask += 4;

            vnc.WritePointerEvent(mask, pos);
        }



        int convertToVKCode(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.None: return 0;
                case KeyCode.Backspace: return 0x08;
                case KeyCode.Delete: return 0x2E;
                case KeyCode.Tab: return 0x09;
                case KeyCode.Clear: return 0x0C;
                case KeyCode.Return: return 0x0D;
                case KeyCode.Pause: return 0x13;
                case KeyCode.Escape: return 0x1B;
                case KeyCode.Space: return 0x20;
                case KeyCode.Keypad0: return 0x60;
                case KeyCode.Keypad1: return 0x61;
                case KeyCode.Keypad2: return 0x62;
                case KeyCode.Keypad3: return 0x63;
                case KeyCode.Keypad4: return 0x64;
                case KeyCode.Keypad5: return 0x65;
                case KeyCode.Keypad6: return 0x66;
                case KeyCode.Keypad7: return 0x67;
                case KeyCode.Keypad8: return 0x68;
                case KeyCode.Keypad9: return 0x69;
                case KeyCode.KeypadPeriod: return 0x6E;
                case KeyCode.KeypadDivide: return 0x6F;
                case KeyCode.KeypadMultiply: return 0x6A;
                case KeyCode.KeypadMinus: return 0x6D;
                case KeyCode.KeypadPlus: return 0x6B;
                case KeyCode.KeypadEnter: return 0xD; // same as return
                                                      //     case KeyCode.KeypadEquals: return -1; // unkown
                case KeyCode.UpArrow: return 0x26;
                case KeyCode.DownArrow: return 0x28;
                case KeyCode.RightArrow: return 0x27;
                case KeyCode.LeftArrow: return 0x25;
                case KeyCode.Insert: return 0x2D;
                case KeyCode.Home: return 0x24;
                case KeyCode.End: return 0x23;
                case KeyCode.PageUp: return 0x21;
                case KeyCode.PageDown: return 0x22;
                case KeyCode.F1: return 0x70;
                case KeyCode.F2: return 0x72;
                case KeyCode.F3: return 0x73;
                case KeyCode.F4: return 0x74;
                case KeyCode.F5: return 0x75;
                case KeyCode.F6: return 0x76;
                case KeyCode.F7: return 0x77;
                case KeyCode.F8: return 0x78;
                case KeyCode.F9: return 0x79;
                case KeyCode.F10: return 0x7A;
                case KeyCode.F11: return 0x7B;
                case KeyCode.F12: return 0x7C;
                case KeyCode.F13: return 0x7D;
                case KeyCode.F14: return 0x7E;
                case KeyCode.F15: return 0x7F;
                case KeyCode.Alpha0: return 0x30;
                case KeyCode.Alpha1: return 0x31;
                case KeyCode.Alpha2: return 0x32;
                case KeyCode.Alpha3: return 0x33;
                case KeyCode.Alpha4: return 0x34;
                case KeyCode.Alpha5: return 0x35;
                case KeyCode.Alpha6: return 0x36;
                case KeyCode.Alpha7: return 0x37;
                case KeyCode.Alpha8: return 0x38;
                case KeyCode.Alpha9: return 0x39;
                case KeyCode.Exclaim: return 0xDF;  // OEM 8 : !
                                                    //    case KeyCode.DoubleQuote: return;
                                                    //    case KeyCode.Hash: return;
                                                    //    case KeyCode.Dollar: return;
                                                    //    case KeyCode.Ampersand: return;
                                                    //    case KeyCode.Quote: return 0Xde;
                                                    //    case KeyCode.LeftParen: return;
                                                    //    case KeyCode.RightParen: return;
                                                    //    case KeyCode.Asterisk: return;
                                                    //    case KeyCode.Plus: return;
                case KeyCode.Comma: return 0xBC;
                case KeyCode.Minus: return 0xBD;
                case KeyCode.Period: return 0xBE;
                case KeyCode.Slash: return 0xBF;
                case KeyCode.Colon: return 0xBF;
                case KeyCode.Semicolon: return 0xBA;
                //              case KeyCode.Less: return;
                //              case KeyCode.Equals: return 0xBB;
                //              case KeyCode.Greater: return;
                //              case KeyCode.Question: return;
                case KeyCode.At: return 0xA5; // alt left
                case KeyCode.LeftBracket: return 0xDB;
                case KeyCode.Backslash: return 0xE2;
                case KeyCode.RightBracket: return 0xDD;
                //               case KeyCode.Caret: return;
                //   case KeyCode.Underscore: return;
                case KeyCode.BackQuote: return 0xC0; // or 0xDF should be exclain : unity error
                case KeyCode.A: return 0x41;
                case KeyCode.B: return 0x42;
                case KeyCode.C: return 0x43;
                case KeyCode.D: return 0x44;
                case KeyCode.E: return 0x45;
                case KeyCode.F: return 0x46;
                case KeyCode.G: return 0x47;
                case KeyCode.H: return 0x48;
                case KeyCode.I: return 0x48;
                case KeyCode.J: return 0x48;
                case KeyCode.K: return 0x48;
                case KeyCode.L: return 0x48;
                case KeyCode.M: return 0x48;
                case KeyCode.N: return 0x48;
                case KeyCode.O: return 0x48;
                case KeyCode.P: return 0x48;
                case KeyCode.Q: return 0x48;
                case KeyCode.R: return 0x48;
                case KeyCode.S: return 0x48;
                case KeyCode.T: return 0x48;
                case KeyCode.U: return 0x48;
                case KeyCode.V: return 0x48;
                case KeyCode.W: return 0x48;
                case KeyCode.X: return 0x48;
                case KeyCode.Y: return 0x48;
                case KeyCode.Z: return 0x48;

                case KeyCode.Numlock: return 0X90;
                case KeyCode.CapsLock: return 0X14;
                case KeyCode.ScrollLock: return 0X90;
                case KeyCode.RightShift: return 0xA0;   //Not good because not taken at all by the OnGUI process
                case KeyCode.LeftShift: return 0xA1;
                case KeyCode.RightControl: return 0xA2;
                case KeyCode.LeftControl: return 0xA3;
                case KeyCode.RightAlt: return 0xA4;
                case KeyCode.LeftAlt: return 0xA5;
                case KeyCode.LeftCommand: return 0xA3; // same as LeftApple, LeftControl
                                                       //*    case KeyCode.LeftApple: return;
                case KeyCode.LeftWindows: return 0x5B;
                case KeyCode.RightCommand: return 0xA2; // right control, right apple
                //case KeyCode.RightApple:  return;
                case KeyCode.RightWindows: return 0x5C;
                case KeyCode.AltGr: return 0xA5;
                // case KeyCode.Help: return;
                case KeyCode.Print: return 44;
                case KeyCode.SysReq: return 44;
                //     case KeyCode.Break: return;
                case KeyCode.Menu: return 0xA4; // alt
                default:
                    Debug.LogError("Invalid Key");
                    return -1;
            }

        }



    }





}
