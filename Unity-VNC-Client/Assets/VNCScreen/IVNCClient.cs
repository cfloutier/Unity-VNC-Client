// Unity 3D Vnc Client - Unity 3D VNC Client Library
// Copyright (C) 2017 Christophe Floutier
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
using VNCScreen.Drawing;
using UnityEngine;


namespace VNCScreen
{
    public delegate void OnConnection(Exception error, bool needPassword);
    /// <summary>
    /// Delegate definition of an Event Handler used to indicate a Framebuffer Update has been received.
    /// </summary>
    public delegate void VncUpdateHandler(IDesktopUpdater update);
    /// <summary>
    /// Delegate definition to check response from server on password
    /// </summary>
    public delegate void OnPassword(bool valid);


    /// <summary>
    /// This interface is used as abstraction layer between the vncSharp client and the plugin 
    /// </summary>
    public interface IVncClient
    {
        /// <summary>
        /// Raised when the connection to the remote host is lost.
        /// </summary>
        event EventHandler ConnectionLost;
      
        /// <summary>
        /// Raised when the connection to the remote host is set or not
        /// </summary>
        event OnConnection onConnection;

        /// <summary>
        /// Connect to a VNC Host and determine which type of Authentication it uses. If the host uses Password Authentication, a call to Authenticate() will be required.
        /// </summary>
        /// <param name="host">The IP Address or Host Name of the VNC Host.</param>
        /// <param name="display">The Display number (used on Unix hosts).</param>
        /// <param name="port">The Port number used by the Host, usually 5900.</param>
        /// <param name="viewOnly">True if mouse/keyboard events are to be ignored.</param>
        /// <returns>Returns True if the VNC Host requires a Password to be sent after Connect() is called, otherwise False.</returns>
        void Connect(string host, int display, int port, bool viewOnly);

        /// <summary>
        /// Use a password to authenticate with a VNC Host. NOTE: This is only necessary if Connect() returns TRUE.
        /// </summary>
        /// <param name="password">The password to use.</param>
        /// <returns>Returns True if Authentication worked, otherwise False.</returns>
        void Authenticate(string password, OnPassword onPassword);

        /// <summary>
        /// Requests that the remote host send a screen update.
        /// </summary>
        /// <param name="refreshFullScreen">TRUE if the entire screen should be refreshed, FALSE if only a partial region needs updating.</param>
        /// <remarks>RequestScreenUpdate needs to be called whenever the client screen needs to be updated to reflect the state of the remote 
        ///	desktop.  Typically you only need to have a particular region of the screen updated and can still use the rest of the 
        /// pixels on the client-side (i.e., when moving the mouse pointer, only the area around the pointer changes).  Therefore, you should
        /// almost always set refreshFullScreen to FALSE.  If the client-side image becomes corrupted, call RequestScreenUpdate with
        /// refreshFullScreen set to TRUE to get the complete image sent again.
        /// </remarks>
        void RequestScreenUpdate(bool refreshFullScreen);

        /// <summary>
        /// Finish setting-up protocol with VNC Host.  Should be called after Connect and Authenticate (if password required).
        /// </summary>
        void Initialize();

        Size BufferSize { get; }

        /// <summary>
        /// Begin getting updates from the VNC Server.  This will continue until StopUpdates() is called.  NOTE: this must be called after Connect().
        /// </summary>
        void StartUpdates();

        /// <summary>
        /// Stops sending requests for updates and disconnects from the remote host.  You must call Connect() again if you wish to re-establish a connection.
        /// </summary>
        void Disconnect();


        /// <summary>
        /// Send mouse updates
        /// </summary>
        /// <param name="pos">mousePosition</param>
        /// <param name="button0">mainButton pressed ? - left</param>
        /// <param name="button1">secondary pressed ? - right</param>
        /// <param name="button2">third pressed ? - middle wheel</param>
        void UpdateMouse(Point pos, bool button0, bool button1, bool button2);


        /// <summary>
        /// Send key event
        /// </summary>
        /// <param name="keysym">key code (x11) </param>
        /// <param name="pressed">pressed or released</param>
        void WriteKeyboardEvent(uint keysym, bool pressed);

        /// <summary>
        /// Update the Desktop Image 
        /// </summary>
        bool updateDesktopImage();


        Texture2D getTexture();
    }

    

}
