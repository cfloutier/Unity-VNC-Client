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

using UnityEngine;

namespace VNCScreen
{
    public class ScreenStates : MonoBehaviour
    {
        public VNCScreen screen;
        public GameObject waitingWheel;
        public GameObject errormark;

        public bool hideScreenWhenUnused = true;

        void Start()
        {
            if (screen == null)
                screen = GetComponent<VNCScreen>();

            if (screen == null)
                screen = GetComponentInChildren<VNCScreen>();

            if (screen == null)
                screen = GetComponentInParent<VNCScreen>();

            Debug.Assert(screen != null);

            screen.onStateChanged_event += onStateChanged;
            onStateChanged(screen.state);
        }

        void ShowScreen(bool show)
        {
            if (hideScreenWhenUnused)
            {
                Renderer r = screen.GetComponent<Renderer>();

                r.enabled = show;
            }
        }


        private void onStateChanged(VNCScreen.RuntimeState state)
        {
            switch (state)
            {
                case VNCScreen.RuntimeState.Disconnected:
                    ShowScreen(false);
                    if (waitingWheel != null) waitingWheel.SetActive(false);
                    errormark.SetActive(false);
                    break;
                case VNCScreen.RuntimeState.Disconnecting:
                    ShowScreen(false);
                    if (waitingWheel != null) waitingWheel.SetActive(true);
                    if (errormark != null) errormark.SetActive(false);
                    break;
                case VNCScreen.RuntimeState.Connected:
                    ShowScreen(true);
                    if (waitingWheel != null) waitingWheel.SetActive(false);
                    if (errormark != null) errormark.SetActive(false);

                    break;
                case VNCScreen.RuntimeState.Connecting:
                    ShowScreen(false);
                    if (waitingWheel != null) waitingWheel.SetActive(true);
                    if (errormark != null) errormark.SetActive(false);
                    break;
                case VNCScreen.RuntimeState.Error:
                    ShowScreen(false);
                    if (waitingWheel != null) waitingWheel.SetActive(false);
                    if (errormark != null) errormark.SetActive(true);

                    break;
                default:
                    break;
            }

        }
    }
}