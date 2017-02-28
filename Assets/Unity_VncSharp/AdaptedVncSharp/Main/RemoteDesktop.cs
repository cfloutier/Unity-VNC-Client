// VncSharp - .NET VNC Client Library
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
using System.Reflection;
using System.ComponentModel;
using UnityEngine;
using UnityVncSharp.Drawing;
using UnityVncSharp.Drawing.Imaging;

#if NONE


using System.Collections.Generic;

namespace UnityVncSharp
{
    /// <summary>
    /// Event Handler delegate declaration used by events that signal successful connection with the server.
    /// </summary>
    public delegate void ConnectCompleteHandler(object sender, ConnectEventArgs e);

    /// <summary>
    /// When connecting to a VNC Host, a password will sometimes be required.  Therefore a password must be obtained from the user.  A default Password dialog box is included and will be used unless users of the control provide their own Authenticate delegate function for the task.  For example, this might pull a password from a configuration file of some type instead of prompting the user.
    /// </summary>
    public delegate string AuthenticateDelegate();




    /// <summary>
    /// The RemoteDesktop control takes care of all the necessary RFB Protocol and GUI handling, including mouse and keyboard support, as well as requesting and processing screen updates from the remote VNC host.  Most users will choose to use the RemoteDesktop control alone and not use any of the other protocol classes directly.
    /// </summary>
    public class RemoteDesktop
    {
        [Description("Raised after a successful call to the Connect() method.")]
        /// <summary>
        /// Raised after a successful call to the Connect() method.  Includes information for updating the local display in ConnectEventArgs.
        /// </summary>
        public event ConnectCompleteHandler ConnectComplete;

        [Description("Raised when the VNC Host drops the connection.")]
        /// <summary>
        /// Raised when the VNC Host drops the connection.
        /// </summary>
        public event EventHandler ConnectionLost;

        [Description("Raised when the VNC Host sends text to the client's clipboard.")]
        /// <summary>
        /// Raised when the VNC Host sends text to the client's clipboard. 
        /// </summary>
        public event EventHandler ClipboardChanged;

        /// <summary>
        /// Points to a Function capable of obtaining a user's password.  By default this means using the PasswordDialog.GetPassword() function; however, users of RemoteDesktop can replace this with any function they like, so long as it matches the delegate type.
        /// </summary>
        public AuthenticateDelegate GetPassword;

        Bitmap desktop;                          // Internal representation of remote image.

        VncClient vnc;                           // The Client object handling all protocol-level interaction
        public int port = 5900;                      // The port to connect to on remote host (5900 is default)
        bool passwordPending = false;            // After Connect() is called, a password might be required.
        bool fullScreenRefresh = false;          // Whether or not to request the entire remote screen be sent.

        VncDesktopTransformPolicy desktopPolicy;

        
        [Description("The name of the remote desktop.")]
        /// <summary>
        /// The name of the remote desktop, or "Disconnected" if not connected.
        /// </summary>
        public string Hostname
        {
            get
            {
                return vnc == null ? "Disconnected" : vnc.HostName;
            }
        }

        public Texture2D texture
        {
            get
            {
                if (desktop == null)
                    return null;

                return desktop.Texture;
            }
        }


       


      
        /// <summary>
        /// Connect to a VNC Host and determine whether or not the server requires a password.
        /// </summary>
        /// <param name="host">The IP Address or Host Name of the VNC Host.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if host is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if display is negative.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="VncSharp.RemoteDesktop.IsConnected" />.</exception>
        public void Connect(string host)
        {
            // Use Display 0 by default.
            Connect(host, 0);
        }


        /// <summary>
        /// Connect to a VNC Host and determine whether or not the server requires a password.
        /// </summary>
        /// <param name="host">The IP Address or Host Name of the VNC Host.</param>
        /// <param name="viewOnly">Determines whether mouse and keyboard events will be sent to the host.</param>
       /// <exception cref="System.ArgumentNullException">Thrown if host is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if display is negative.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="VncSharp.RemoteDesktop.IsConnected" />.</exception>
        public void Connect(string host, bool viewOnly)
        {
            // Use Display 0 by default.
            Connect(host, 0, viewOnly);
        }

        /// <summary>
        /// Connect to a VNC Host and determine whether or not the server requires a password.
        /// </summary>
        /// <param name="host">The IP Address or Host Name of the VNC Host.</param>
        /// <param name="display">The Display number (used on Unix hosts).</param>
        /// <exception cref="System.ArgumentNullException">Thrown if host is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if display is negative.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="VncSharp.RemoteDesktop.IsConnected" />.</exception>
        public void Connect(string host, int display)
        {
            Connect(host, display, false);
        }

       

        /// <summary>
        /// Connect to a VNC Host and determine whether or not the server requires a password.
        /// </summary>
        /// <param name="host">The IP Address or Host Name of the VNC Host.</param>
        /// <param name="display">The Display number (used on Unix hosts).</param>
        /// <param name="viewOnly">Determines whether mouse and keyboard events will be sent to the host.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if host is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if display is negative.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is already Connected.  See <see cref="VncSharp.RemoteDesktop.IsConnected" />.</exception>
        public void Connect(string host, int display, bool viewOnly)
        {
            // TODO: Should this be done asynchronously so as not to block the UI?  Since an event 
            // indicates the end of the connection, maybe that would be a better design.
            InsureConnection(false);

            if (host == null) throw new ArgumentNullException("host");
            if (display < 0) throw new ArgumentOutOfRangeException("display", display, "Display number must be a positive integer.");

            // Start protocol-level handling and determine whether a password is needed
            vnc = new VncClient();
            vnc.ConnectionLost += new EventHandler(VncClientConnectionLost);
            vnc.ServerCutText += new EventHandler(VncServerCutText);

            passwordPending = vnc.Connect(host, display, VncPort, viewOnly);

            desktopPolicy = new VncClippedDesktopPolicy(vnc, this);

            if (passwordPending)
            {
                // Server needs a password, so call which ever method is refered to by the GetPassword delegate.
                string password = GetPassword();

                if (password == null)
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
        /// Stops the remote host from sending further updates and disconnects.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not already in the Connected state. See <see cref="VncSharp.RemoteDesktop.IsConnected" />.</exception>
        public void Disconnect()
        {
            InsureConnection(true);
            vnc.ConnectionLost -= new EventHandler(VncClientConnectionLost);
            vnc.ServerCutText -= new EventHandler(VncServerCutText);
            vnc.Disconnect();
            SetState(RuntimeState.Disconnected);
            OnConnectionLost("Disconnected");

            updates.Clear();
        }

        protected void Dispose()
        {
            // Make sure the connection is closed--should never happen :)
            if (state != RuntimeState.Disconnected)
            {
                Disconnect();
            }

            // See if either of the bitmaps used need clean-up.  
            if (desktop != null) desktop.Dispose();
        }

        /*	protected void OnPaint(PaintEventArgs pe)
            {
                // If the control is in design mode, draw a nice background, otherwise paint the desktop.
                if (!DesignMode) {
                    switch(state) {
                        case RuntimeState.Connected:
                            System.Diagnostics.Debug.Assert(desktop != null);
                            DrawDesktopImage(desktop, pe.Graphics);
                            break;
                        case RuntimeState.Disconnected:
                            // Do nothing, just black background.
                            break;
                        default:
                            // Sanity check
                            throw new NotImplementedException(string.Format("RemoteDesktop in unknown State: {0}.", state.ToString()));
                    }
                } else {
                    // Draw a static screenshot of a Windows desktop to simulate the control in action
                    System.Diagnostics.Debug.Assert(designModeDesktop != null);
                    DrawDesktopImage(designModeDesktop, pe.Graphics);
                }
                base.OnPaint(pe);
            }

            protected override void OnResize(EventArgs eventargs)
            {
                // Fix a bug with a ghost scrollbar in clipped mode on maximize
                Control parent = Parent;
                while (parent != null) {
                    if (parent is Form) {
                        Form form = parent as Form;
                        if (form.WindowState == FormWindowState.Maximized)
                            form.Invalidate();
                        parent = null;
                    } else {
                        parent = parent.Parent;
                    }
                }

                base.OnResize(eventargs);
            }

        /// <summary>
        /// Draws an image onto the control in a size-aware way.
        /// </summary>
        /// <param name="desktopImage">The desktop image to be drawn to the control's sufrace.</param>
        /// <param name="g">The Graphics object representing the control's drawable surface.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not already in the Connected state.</exception>
        protected void DrawDesktopImage(Image desktopImage, Graphics g)
		{
			g.DrawImage(desktopImage, desktopPolicy.RepositionImage(desktopImage));
		}*/

        /// <summary>
        /// RemoteDesktop listens for ConnectionLost events from the VncClient object.
        /// </summary>
        /// <param name="sender">The VncClient object that raised the event.</param>
        /// <param name="e">An empty EventArgs object.</param>
        protected void VncClientConnectionLost(object sender, EventArgs e)
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

            if (ConnectionLost != null)
            {
                ConnectionLost(sender, e);
            }
        }

        // Handle the VncClient ServerCutText event and bubble it up as ClipboardChanged.
        protected void VncServerCutText(object sender, EventArgs e)
        {
            OnClipboardChanged();
        }

        protected void OnClipboardChanged()
        {
            if (ClipboardChanged != null)
                ClipboardChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Dispatches the ConnectionLost event if any targets have registered.
        /// </summary>
        /// <param name="e">An EventArgs object.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is in the Connected state.</exception>
        protected void OnConnectionLost(string reason)
        {
            if (ConnectionLost != null)
            {
                ConnectionLost(this, new ErrorEventArg(reason));
            }
        }

        /// <summary>
        /// Dispatches the ConnectComplete event if any targets have registered.
        /// </summary>
        /// <param name="e">A ConnectEventArgs object with information about the remote framebuffer's geometry.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not in the Connected state.</exception>
        protected void OnConnectComplete(ConnectEventArgs e)
        {
            if (ConnectComplete != null)
            {
                ConnectComplete(this, e);
            }
        }

        public void UpdateMouse(Point pos, bool button0, bool button1, bool button2)
        {
            byte mask = 0;

            if (button0) mask += 1;
            if (button1) mask += 2;
            if (button2) mask += 4;

            vnc.WritePointerEvent(mask, pos);
        }

        // Handle Mouse Events:		 -------------------------------------------
        // In all cases, the same thing needs to happen: figure out where the cursor
        // is, and then figure out the state of all mouse buttons.
        // TODO: currently we don't handle the case of 3-button emulation with 2-buttons.
        /*	protected void OnMouseMove(MouseEventArgs mea)
            {
                // Only bother if the control is connected.
                if (IsConnected) {
                    // See if the mouse pointer is inside the area occupied by the desktop on screen.
                    Rectangle adjusted = desktopPolicy.GetMouseMoveRectangle();
                    if (adjusted.Contains(PointToClient(MousePosition)))
                        UpdateRemotePointer();
                }
                base.OnMouseMove(mea);
            }

            protected override void OnMouseDown(MouseEventArgs mea)
            {
                // BUG FIX (Edward Cooke) -- Deal with Control.Select() semantics
                if (!Focused) {
                    Focus();
                    Select();
                } else {
                    UpdateRemotePointer();
                }
                base.OnMouseDown(mea);
            }

            // Find out the proper masks for Mouse Button Up Events
            protected override void OnMouseUp(MouseEventArgs mea)
            {
                UpdateRemotePointer();
                base.OnMouseUp(mea);
            }

            // TODO: Perhaps overload UpdateRemotePointer to take a flag indicating if mousescroll has occured??
            protected override void OnMouseWheel(MouseEventArgs mea)
            {
                // HACK: this check insures that while in DesignMode, no messages are sent to a VNC Host
                // (i.e., there won't be one--NullReferenceException)			
                if (!DesignMode && IsConnected) {
                    Point current = PointToClient(MousePosition);
                    byte mask = 0;

                    // mouse was scrolled forward
                    if (mea.Delta > 0) {
                        mask += 8;
                    } else if (mea.Delta < 0) { // mouse was scrolled backwards
                        mask += 16;
                    }

                    vnc.WritePointerEvent(mask, desktopPolicy.GetMouseMovePoint(current));
                }			
                base.OnMouseWheel(mea);
            }

       

            // Handle Keyboard Events:		 -------------------------------------------
            // These keys don't normally throw an OnKeyDown event. Returning true here fixes this.
            protected override bool IsInputKey(Keys keyData)
            {
                switch (keyData) {
                    case Keys.Tab:
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Shift:
                    case Keys.RWin:
                    case Keys.LWin:
                        return true;
                    default:
                        return base.IsInputKey(keyData);
                }
            }

            // Thanks to Lionel Cuir, Christian and the other developers at 
            // Aulofee.com for cleaning-up my keyboard code, specifically:
            // ManageKeyDownAndKeyUp, OnKeyPress, OnKeyUp, OnKeyDown.
            private void ManageKeyDownAndKeyUp(KeyEventArgs e, bool isDown)
            {
                UInt32 keyChar;
                bool isProcessed = true;
                switch(e.KeyCode)
                {
                    case Keys.Tab:				keyChar = 0x0000FF09;		break;
                    case Keys.Enter:			keyChar = 0x0000FF0D;		break;
                    case Keys.Escape:			keyChar = 0x0000FF1B;		break;
                    case Keys.Home:				keyChar = 0x0000FF50;		break;
                    case Keys.Left:				keyChar = 0x0000FF51;		break;
                    case Keys.Up:				keyChar = 0x0000FF52;		break;
                    case Keys.Right:			keyChar = 0x0000FF53;		break;
                    case Keys.Down:				keyChar = 0x0000FF54;		break;
                    case Keys.PageUp:			keyChar = 0x0000FF55;		break;
                    case Keys.PageDown:			keyChar = 0x0000FF56;		break;
                    case Keys.End:				keyChar = 0x0000FF57;		break;
                    case Keys.Insert:			keyChar = 0x0000FF63;		break;
                    case Keys.ShiftKey:			keyChar = 0x0000FFE1;		break;

                    // BUG FIX -- added proper Alt/CTRL support (Edward Cooke)
                    case Keys.Alt:              keyChar = 0x0000FFE9;       break;
                    case Keys.ControlKey:       keyChar = 0x0000FFE3;       break;
                    case Keys.LControlKey:      keyChar = 0x0000FFE3;       break;
                    case Keys.RControlKey:      keyChar = 0x0000FFE4;       break;

                    case Keys.Menu:				keyChar = 0x0000FFE9;		break;
                    case Keys.Delete:			keyChar = 0x0000FFFF;		break;
                    case Keys.LWin:				keyChar = 0x0000FFEB;		break;
                    case Keys.RWin:				keyChar = 0x0000FFEC;		break;
                    case Keys.Apps:				keyChar = 0x0000FFEE;		break;
                    case Keys.F1:
                    case Keys.F2:
                    case Keys.F3:
                    case Keys.F4:
                    case Keys.F5:
                    case Keys.F6:
                    case Keys.F7:
                    case Keys.F8:
                    case Keys.F9:
                    case Keys.F10:
                    case Keys.F11:
                    case Keys.F12:
                        keyChar = 0x0000FFBE + ((UInt32)e.KeyCode - (UInt32)Keys.F1);
                        break;
                    default:
                        keyChar = 0;
                        isProcessed = false;
                        break;
                }

                if(isProcessed)
                {
                    vnc.WriteKeyboardEvent(keyChar, isDown);
                    e.Handled = true;
                }
            }

            // HACK: the following overrides do a double check on DesignMode so 
            // that if still in design mode, no messages are sent for 
            // mouse/keyboard events (i.e., there won't be Host yet--
            // NullReferenceException)			
            protected override void OnKeyPress(KeyPressEventArgs e)
            {
                base.OnKeyPress (e);
                if (DesignMode || !IsConnected)
                    return;

                if (e.Handled)
                    return;

                if(Char.IsLetterOrDigit(e.KeyChar) || Char.IsWhiteSpace(e.KeyChar) || Char.IsPunctuation(e.KeyChar) ||
                    e.KeyChar == '~' || e.KeyChar == '`' || e.KeyChar == '<' || e.KeyChar == '>' ||
                    e.KeyChar == '|' || e.KeyChar == '=' || e.KeyChar == '+' || e.KeyChar == '$' || e.KeyChar == '^')
                {
                    vnc.WriteKeyboardEvent((UInt32)e.KeyChar, true);
                    vnc.WriteKeyboardEvent((UInt32)e.KeyChar, false);
                }
                else if(e.KeyChar == '\b')
                {
                    UInt32 keyChar = ((UInt32)'\b') | 0x0000FF00;
                    vnc.WriteKeyboardEvent(keyChar, true);
                    vnc.WriteKeyboardEvent(keyChar, false);
                }
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                if (DesignMode || !IsConnected)
                    return;

                ManageKeyDownAndKeyUp(e, true);
                if(e.Handled)
                    return;

                base.OnKeyDown(e);
            }

            protected override void OnKeyUp(KeyEventArgs e)
            {
                if (DesignMode || !IsConnected)
                    return;

                ManageKeyDownAndKeyUp(e, false);
                if (e.Handled)
                    return;

                base.OnKeyDown(e);
            }*/

        /// <summary>
        /// Sends a keyboard combination that would otherwise be reserved for the client PC.
        /// </summary>
        /// <param name="keys">SpecialKeys is an enumerated list of supported keyboard combinations.</param>
        /// <remarks>Keyboard combinations are Pressed and then Released, while single keys (e.g., SpecialKeys.Ctrl) are only pressed so that subsequent keys will be modified.</remarks>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not in the Connected state.</exception>
        public void SendSpecialKeys(SpecialKeys keys)
        {
            this.SendSpecialKeys(keys, true);
        }

        /// <summary>
        /// Sends a keyboard combination that would otherwise be reserved for the client PC.
        /// </summary>
        /// <param name="keys">SpecialKeys is an enumerated list of supported keyboard combinations.</param>
        /// <remarks>Keyboard combinations are Pressed and then Released, while single keys (e.g., SpecialKeys.Ctrl) are only pressed so that subsequent keys will be modified.</remarks>
        /// <exception cref="System.InvalidOperationException">Thrown if the RemoteDesktop control is not in the Connected state.</exception>
        public void SendSpecialKeys(SpecialKeys keys, bool release)
        {
            InsureConnection(true);
            // For all of these I am sending the key presses manually instead of calling
            // the keyboard event handlers, as I don't want to propegate the calls up to the 
            // base control class and form.
            switch (keys)
            {
                case SpecialKeys.Ctrl:
                    PressKeys(new uint[] { 0xffe3 }, release);  // CTRL, but don't release
                    break;
                case SpecialKeys.Alt:
                    PressKeys(new uint[] { 0xffe9 }, release);  // ALT, but don't release
                    break;
                case SpecialKeys.CtrlAltDel:
                    PressKeys(new uint[] { 0xffe3, 0xffe9, 0xffff }, release); // CTRL, ALT, DEL
                    break;
                case SpecialKeys.AltF4:
                    PressKeys(new uint[] { 0xffe9, 0xffc1 }, release); // ALT, F4
                    break;
                case SpecialKeys.CtrlEsc:
                    PressKeys(new uint[] { 0xffe3, 0xff1b }, release); // CTRL, ESC
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
        private void PressKeys(uint[] keys, bool release)
        {
            System.Diagnostics.Debug.Assert(keys != null, "keys[] cannot be null.");

            for (int i = 0; i < keys.Length; ++i)
            {
                vnc.WriteKeyboardEvent(keys[i], true);
            }

            if (release)
            {
                // Walk the keys array backwards in order to release keys in correct order
                for (int i = keys.Length - 1; i >= 0; --i)
                {
                    vnc.WriteKeyboardEvent(keys[i], false);
                }
            }
        }
    }
}

#endif