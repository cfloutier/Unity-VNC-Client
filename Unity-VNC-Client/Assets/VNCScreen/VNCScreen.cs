// Unity 3D Vnc Client - Unity 3D VNC Client Library
// Copyright (C) 2017 Christophe Floutier
//
// Based on VncSharp - .NET VNC Client Library
// Copyright (C) 2008 David Humphrey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.Collections.Generic;

using System.Threading;
using UnityEngine;
using VNCScreen.Drawing;
using UnityVncSharp;

namespace VNCScreen
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


    /// <summary>
    /// The Main Component. 
    /// It must be placed on a Quad Mesh with a MeshCollider
    /// The password, host, port and display must be configured in the editor fields
    /// 
    /// To control the screen, use a VNCMouseRaycaster for instance.
    /// 
    /// This code is mainly adapted from RemoteDesktop code from the VncSharp
    /// </summary>
    public class VNCScreen : MonoBehaviour
    {
        public string host;
        public int port = 5900;
        public bool viewOnly = false;
        public int display = 0;
        public string password;

        public Material disconnectedScreen;
        public Material connectedMaterial;

        private Size screenSize;
        public Size ScreenSize { get { return screenSize; } }

        public bool connectOnStartUp;
        bool passwordPending = false;            // After Connect() is called, a password might be required.
        bool fullScreenRefresh = false;		     // Whether or not to request the entire remote screen be sent.

        private Material m;

        Thread mainThread;

        // Use this for initialization
        void Start()
        {
            mainThread = System.Threading.Thread.CurrentThread;

            setDisconnectedMaterial();
            if (connectOnStartUp)
            {
                Connect();
            }
        }

        void setDisconnectedMaterial()
        {
            if (disconnectedScreen != null)
            {
                GetComponent<Renderer>().sharedMaterial = disconnectedScreen;
            }
        }

        IVncClient vnc;                           // The Client object handling all protocol-level interaction

        public enum RuntimeState
        {
            Disconnected,
            Disconnecting,

            Connecting,
            WaitFirstBuffer,
            Connected,
            Error
        }

        public enum VNCPlugin
        {
            VNCSharp,
            RealVnc
        }

        public VNCPlugin plugin;

        IVncClient buildVNC()
        {
            switch (plugin)
            {
                default:
                case VNCPlugin.VNCSharp:
                    return new VNCSharpClient();
                case VNCPlugin.RealVnc:
                    {
                        return gameObject.AddComponent<RealVncClient>();
                    }
            }
        }

        public RuntimeState state = RuntimeState.Disconnected;
        public delegate void OnStateChanged(RuntimeState state);
        public event OnStateChanged onStateChanged_event;

        /// <summary>
        /// True if the RemoteDesktop is connected and authenticated (if necessary) with a remote VNC Host; otherwise False.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return state == RuntimeState.Connected || state == RuntimeState.WaitFirstBuffer;
            }
        }

        public void Connect()
        {
            // Ignore attempts to use invalid port numbers
            if (port < 1 | port > 65535) port = 5900;
            if (display < 0) display = 0;
            if (host == null) throw new ArgumentNullException("host");

            StartCoroutine(Connection());
        }

        IEnumerator Connection()
        {
            if (IsConnected)
            {
                Disconnect();
            }

            while (state != RuntimeState.Disconnected)
            {
                yield return new WaitForEndOfFrame();
            }

            // Start protocol-level handling and determine whether a password is needed
            vnc = buildVNC();
            vnc.ConnectionLost += new EventHandler(OnConnectionLost);
            vnc.onConnection += Vnc_onConnection;
            connectionReceived = false;
            SetState(RuntimeState.Connecting);

            vnc.Connect(host, display, port, false);

            Debug.Log("[VNCScreen] Connection In progress " + host + ":" + port);

            while (!connectionReceived)
                yield return new WaitForFixedUpdate();

            if (needPassword)
            {
                // Server needs a password, so call which ever method is refered to by the GetPassword delegate.
                if (string.IsNullOrEmpty(password))
                {
                    // No password could be obtained (e.g., user clicked Cancel), so stop connecting
                    SetState(RuntimeState.Error);
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

        bool connectionReceived;
        bool needPassword;

        private void Vnc_onConnection(Exception errorConnection, bool needPassword)
        {
            if (errorConnection != null)
            {
                OnConnectionLost(this, new ErrorEventArg(errorConnection));
                return;
            }
            this.connectionReceived = true;
            this.needPassword = needPassword;
        }

        /// <summary>
        /// Authenticate with the VNC Host using a user supplied password.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="VncSharp.RemoteDesktop.IsConnected" />.</exception>
        /// <exception cref="System.NullReferenceException">Thrown if the password is null.</exception>
        /// <param name="password">The user's password.</param>
        public void Authenticate(string password)
        {
            if (!passwordPending)
                throw new InvalidOperationException("Authentication is only required when Connect() returns True and the VNC Host requires a password.");

            if (password == null)
                throw new NullReferenceException("password");

            passwordPending = false;  // repeated calls to Authenticate should fail.
            vnc.Authenticate(password, OnPassword);
        }

        void OnPassword(bool ok)
        {
            if (ok)
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
                state = RuntimeState.Disconnecting;

                Disconnect();
            }

            if (e is ErrorEventArg)
            {
                var error = e as ErrorEventArg;

                if (error.Exception != null)
                {
                    SetState(RuntimeState.Error);
                    Debug.LogException(error.Exception);
                }
                else if (!string.IsNullOrEmpty(error.Reason))
                {
                    Debug.Log("[VNCScreen]" + error.Reason); 
                }
            }
            else
            {
                Debug.Log("VncDesktop_ConnectionLost");
            }
        }

        List<RuntimeState> stateChanges = new List<RuntimeState>();

        private void SetState(RuntimeState newState)
        {
            var isMainThread = mainThread.Equals(Thread.CurrentThread);
            if (!isMainThread)
            {
                stateChanges.Add(newState);
                return;
            }


            switch (newState)
            {
                case RuntimeState.Disconnected:
                    setDisconnectedMaterial();
                    break;
                case RuntimeState.Disconnecting:
                    setDisconnectedMaterial();
                    break;
                case RuntimeState.Connected:
                    break;
                case RuntimeState.WaitFirstBuffer:
                    setDisconnectedMaterial();
                    break;
                case RuntimeState.Connecting:
                    setDisconnectedMaterial();
                    break;
                case RuntimeState.Error:
                    setDisconnectedMaterial();
                    break;
                default:
                    break;
            }
            state = newState;

            if (onStateChanged_event != null)
                onStateChanged_event(state);
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
            if (state != RuntimeState.Connected) return;

            fullScreenRefresh = true;
        }

        /// <summary>
        /// After protocol-level initialization and connecting is complete, the local GUI objects have to be set-up, and requests for updates to the remote host begun.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already in the Connected state.  See <see cref="VncSharp.RemoteDesktop.IsConnected" />.</exception>		
        protected bool Initialize()
        {
            // Finish protocol handshake with host now that authentication is done.  
            vnc.Initialize();
            SetState(RuntimeState.WaitFirstBuffer);

            screenSize = vnc.BufferSize;

            // Tell the user of this control the necessary info about the desktop in order to setup the display
            // Create a texture
            Texture2D tex = vnc.getTexture();

            if (connectedMaterial == null)
                connectedMaterial = GetComponent<Renderer>().sharedMaterial;

            // Set texture onto our material
            m = Instantiate(connectedMaterial) as Material;
            m.mainTexture = tex;
            GetComponent<Renderer>().sharedMaterial = m;

            // Refresh scroll properties
            //AutoScrollMinSize = desktopPolicy.AutoScrollMinSize;

            // Start getting updates from the remote host (vnc.StartUpdates will begin a worker thread).          
            vnc.StartUpdates();

            return true;
        }

        public void Disconnect()
        {
            vnc.ConnectionLost -= new EventHandler(OnConnectionLost);

            vnc.Disconnect();
            SetState(RuntimeState.Disconnected);
            OnConnectionLost("Disconnected");
        }


        // Update is called once per frame
        void Update()
        {
            if (IsConnected)
            {
                GetComponent<Renderer>().sharedMaterial.mainTexture = vnc.getTexture();

                if (vnc.updateDesktopImage())
                {
                    if (state == RuntimeState.WaitFirstBuffer)
                        SetState(RuntimeState.Connected);
                }

                if (state == RuntimeState.Connected)
                {
                    vnc.RequestScreenUpdate(fullScreenRefresh);

                    // Make sure the next screen update is incremental
                    fullScreenRefresh = false;
                }

            }

            for (int i = 0; i < stateChanges.Count; i++)
            {
                SetState(stateChanges[i]);
            }

            stateChanges.Clear();
        }

        void OnApplicationQuit()
        {
            if (IsConnected)
                Disconnect();
        }

        public void UpdateMouse(Vector2 pos, bool button0, bool button1, bool button2)
        {
            if (!IsConnected) return;

            Size s = vnc.BufferSize;
            Point point = new Point((int)(pos.x * s.Width), (int)((1 - pos.y) * s.Height));

            UpdateMouse(point, button0, button1, button2);
        }

        public void UpdateMouse(Point pos, bool button0, bool button1, bool button2)
        {
            vnc.UpdateMouse(pos, button0, button1, button2);
        }

        /// <summary>
        /// Sends a keyboard combination that would otherwise be reserved for the client PC.
        /// </summary>
        /// <param name="keys">SpecialKeys is an enumerated list of supported keyboard combinations.</param>
        /// <remarks>Keyboard combinations are Pressed and then Released, while single keys (e.g., SpecialKeys.Ctrl) are only pressed so that subsequent keys will be modified.</remarks>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not in the Connected state.</exception>
        public void SendSpecialKeys(SpecialKeys keys)
        {
            SendSpecialKeys(keys, true);
        }

        /// <summary>
        /// Sends a keyboard combination that would otherwise be reserved for the client PC.
        /// </summary>
        /// <param name="keys">SpecialKeys is an enumerated list of supported keyboard combinations.</param>
        /// <remarks>Keyboard combinations are Pressed and then Released, while single keys (e.g., SpecialKeys.Ctrl) are only pressed so that subsequent keys will be modified.</remarks>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not in the Connected state.</exception>
        public void SendSpecialKeys(SpecialKeys keys, bool release)
        {
            if (state != RuntimeState.Connected) return;

            // For all of these I am sending the key presses manually instead of calling
            // the keyboard event handlers, as I don't want to propegate the calls up to the 
            // base control class and form.
            switch (keys)
            {
                case SpecialKeys.Ctrl:
                    PressKeys(new uint[] { 0xffe3 }, true, release);  // CTRL, but don't release
                    break;
                case SpecialKeys.Alt:
                    PressKeys(new uint[] { 0xffe9 }, true, release);  // ALT, but don't release
                    break;
                case SpecialKeys.CtrlAltDel:
                    PressKeys(new uint[] { 0xffe3, 0xffe9, 0xffff }, true, release); // CTRL, ALT, DEL
                    break;
                case SpecialKeys.AltF4:
                    PressKeys(new uint[] { 0xffe9, 0xffc1 }, true, release); // ALT, F4
                    break;
                case SpecialKeys.CtrlEsc:
                    PressKeys(new uint[] { 0xffe3, 0xff1b }, true, release); // CTRL, ESC
                    break;
                // TODO: are there more I should support???
                default:
                    break;
            }
        }

        /// <summary>
        /// Given a list of keysym values, sends a key press for each, then a release.
        /// </summary>
        /// <param name="keys">An array of keysym values representing keys to press/release.</param>
        /// <param name="release">A boolean indicating whether the keys should be Pressed and then Released.</param>
        private void PressKeys(uint[] keys, bool pressed, bool released)
        {
            //        System.Diagnostics.Debug.Assert(keys != null, "keys[] cannot be null.");

            for (int i = 0; i < keys.Length; ++i)
            {
                PressKey(keys[i], pressed, released);
            }
        }

        private void PressKey(uint key, bool pressed, bool released)
        {
            if (IsConnected)
            {
                //    Debug.Log("Press Key " + key + " - " + pressed + " - " + released);

                if (pressed)
                    vnc.WriteKeyboardEvent(key, true);
                if (released)
                    vnc.WriteKeyboardEvent(key, false);
            }
        }

        public void OnKey(KeyCode key, bool pressed)
        {
            uint code = KeyTranslator.convertToXKCode(key);
            PressKey(code, pressed, !pressed);
        }




    }



#if UNITY_EDITOR





#endif

}
