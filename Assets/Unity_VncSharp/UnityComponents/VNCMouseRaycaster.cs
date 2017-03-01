using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityVncSharp.Unity
{
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


        void OnGUI()
        {
            if (!manageKeys || vnc == null)
            {
                return;
            }
            Event theEvent = Event.current;

            if (theEvent.isKey)
            {
                if (theEvent.type == EventType.keyDown)
                {
                    if (theEvent.keyCode != KeyCode.None)

                        vnc.OnKey(theEvent.keyCode, true);
                    //   Debug.Log("Down : " + theEvent.keyCode);

                }
                else if (theEvent.type == EventType.keyUp)
                {
                    if (theEvent.keyCode != KeyCode.None)
                        vnc.OnKey(theEvent.keyCode, false);
                    // Debug.Log("Up : " + theEvent.keyCode);
                }
            }
        }


    }
}