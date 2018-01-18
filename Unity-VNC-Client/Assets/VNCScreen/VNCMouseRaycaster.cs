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

    /// <summary>
    /// This componetn must be put on a little sphere (for instance) 
    /// and is used as a mouse cursor and keyboard event sender to the VncScreen
    /// 
    /// </summary>
    public class VNCMouseRaycaster : MonoBehaviour
    {
        private Ray ray;
        private RaycastHit hit;

        private Vector3 hit_pos;
        private Vector3 uvPos;

        private VNCScreen vnc;
        private Collider touchedCollider = null;
        private Renderer r;

        public bool manageKeys;

        void Awake()
        {
            showCursor(true);
            r = GetComponent<Renderer>();
            // obj = GameObject.FindGameObjectWithTag("pointer");
        }

        void showCursor(bool visible)
        {
            Cursor.visible = visible;
            if (r != null)
                r.enabled = !visible;
        }


        /// <summary>
        /// Get the mosue position, send a raycast and identify the vnc screen uder mouse cursor
        /// Then, if any, send mouse event to it
        /// </summary>
        void Update()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000))
            {
                Collider c = hit.collider;
                if (touchedCollider != c)
                {
                    touchedCollider = c;
                    vnc = c.GetComponent<VNCScreen>();
                }
            }
            else
            {
                touchedCollider = null;
                vnc = null;
            }

            if (vnc != null)
            {
                hit_pos = hit.point;

                transform.position = hit_pos;
                uvPos = hit.textureCoord2;

                vnc.UpdateMouse(uvPos, Input.GetMouseButton(0), Input.GetMouseButton(2), Input.GetMouseButton(1));
                showCursor(false);
            }
            else
                showCursor(true);
        }

        /// <summary>
        /// Send key event to the active VncClient 
        /// 
        /// I use OnGUI because it is the simpliest way to get key event
        /// there are still many troubles, the shifts keys are not getted and many keys onb my keyboard are mishandled.
        /// 
        /// </summary>
        void OnGUI()
        {
            if (!manageKeys || vnc == null)
            {
                return;
            }
            Event theEvent = Event.current;

            if (theEvent.isKey)
            {
                if (theEvent.type == EventType.KeyDown)
                {
                    if (theEvent.keyCode != KeyCode.None)

                        vnc.OnKey(theEvent.keyCode, true);
                    //   Debug.Log("Down : " + theEvent.keyCode);

                }
                else if (theEvent.type == EventType.KeyUp)
                {
                    if (theEvent.keyCode != KeyCode.None)
                        vnc.OnKey(theEvent.keyCode, false);
                    // Debug.Log("Up : " + theEvent.keyCode);
                }
            }
        }


    }
}