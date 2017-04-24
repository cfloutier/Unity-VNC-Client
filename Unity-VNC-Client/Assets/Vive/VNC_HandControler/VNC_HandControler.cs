using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
using UnityEngine.UI;

using VNCScreen;

[ExecuteInEditMode]
public class VNC_HandControler : MonoBehaviour
{
    EVRButtonId mainButton = EVRButtonId.k_EButton_SteamVR_Trigger;
    EVRButtonId rightButton = EVRButtonId.k_EButton_SteamVR_Touchpad;
    EVRButtonId midButton = EVRButtonId.k_EButton_Grip;

    bool isMainPressed()
    {

        return controller.GetAxis(mainButton).x > 0.9f;
    }



    public Color colorHover = Color.cyan;
    public Color colorNormal = Color.yellow;
    public Color colorDown = Color.red;

    // Use this for initialization
    void Start ()
    {
        Transform[] tr = GetComponentsInChildren<Transform>();
        foreach(var t in tr)
        {
            t.gameObject.hideFlags = 0;
        }
	}

    void Awake()
    {
        line = GetComponentInChildren<StraightLine>();      
        startLine = line.from;
        endLine = line.to;

        StartCoroutine(StartUp());
    }


    StraightLine line;
    SteamVR_Controller.Device controller;

    Transform endLine;
    Transform startLine;

    Hand hand;
    IEnumerator StartUp()
    {
        hand = GetComponentInParent<Hand>();
        Debug.AssertFormat(hand != null, "VNCHandControler should be place child of a Hand");

        while (hand.controller == null || hand.controller.index < 0)
        {
            yield return new WaitForEndOfFrame();
        }

        controller = hand.controller;
    }

    bool down = false;


    public Vector2 minMaxSizeDot = new Vector2(0.01f, 0.1f);

    Vector3 direction = Vector3.forward;

    bool handContainObject()
    {
        if (hand == null) return false;

        if (hand.AttachedObjects.Count > 1)
            return true;

        if (hand.hoveringInteractable != null)
            return true;

        return false;
    }

    VNCScreen.VNCScreen vnc = null;
    RaycastHit hit = new RaycastHit();
    public float maxDistance = 2;
    Collider touchedCollider = null;

    // Update is called once per frame
    void Update ()
    {
        if (controller == null)
            return;

        if (handContainObject())
        {
            line.gameObject.SetActive(false);
            return;
        }
        else
            line.gameObject.SetActive(true);

        Ray ray = new Ray(startLine.position, startLine.forward);
       
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            endLine.position = hit.point;

            Collider c = hit.collider;
            if (touchedCollider != c)
            {
                touchedCollider = c;
                vnc = c.GetComponent<VNCScreen.VNCScreen>();
            }  
        }
        else
        {
            touchedCollider = null;
            vnc = null;
            endLine.position = startLine.position + startLine.forward * maxDistance;
        }

        if (down)
        {
            if (!controller.GetPress(mainButton))
            {
                down = false;
                line.color = Color.red;
                line.sizeDot = minMaxSizeDot.x;
            }
        }
        else
        {
            if (controller.GetPress(mainButton))
            {
                down = true;
                line.color = Color.yellow;
                line.sizeDot = minMaxSizeDot.x;
            }
        }

        if (!down)
        {
            Vector2 axis = controller.GetAxis(EVRButtonId.k_EButton_SteamVR_Trigger);
            line.sizeDot = Mathf.Lerp(minMaxSizeDot.x, minMaxSizeDot.y, 1 - axis.x);
        }

        if (vnc != null)
        {
            Vector3 hit_pos = hit.point;

            //   transform.position = hit_pos;

            Vector2 pos = hit.collider.transform.InverseTransformPoint(hit.point);
            pos += new Vector2(0.5f, 0.5f);
            if (debugText != null)
            {
                debugText.text = "pos " + pos;
            }
            if (down)
                line.color = colorDown;
            else
                line.color = colorHover;

            vnc.UpdateMouse(pos, down, controller.GetPress(rightButton), controller.GetPress(midButton));
        }
        else
        {
            debugText.text = "no VNC ";
            line.color = colorNormal;
        }
    }

    public Text debugText;
}
