using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
using UnityVncSharp.Unity;


[ExecuteInEditMode]
public class VNC_HandControler : MonoBehaviour
{
    EVRButtonId mainButton = EVRButtonId.k_EButton_SteamVR_Trigger;

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

        endLine = line.to;
        startLine = line.from;

        StartCoroutine(StartUp());
    }
    StraightLine line;
    SteamVR_Controller.Device controller;
    Transform endLine;
    Transform startLine;

    IEnumerator StartUp()
    {
        Hand hand = GetComponentInParent<Hand>();
        Debug.AssertFormat(hand != null, "VNCHandControler shoudl be place child of a Hand");

        while (hand.controller == null || hand.controller.index < 0)
        {
            yield return new WaitForEndOfFrame();
        }

        controller = hand.controller;
    }

    bool down = false;


    public Vector2 minMaxSizeDot = new Vector2(0.01f, 0.1f);

    Vector3 direction = Vector3.forward;

    VNCScreen vnc = null;
    RaycastHit hit = new RaycastHit();
    public float maxDistance = 2;
    Collider touchedCollider = null;
    // Update is called once per frame
    void Update ()
    {
        if (controller == null)
            return;

        Ray ray = new Ray(startLine.position, startLine.forward);
       
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            endLine.position = hit.point;


            Collider c = hit.collider;
            if (touchedCollider != c)
            {
          //      line.color = Color.yellow;

                touchedCollider = c;
                vnc = c.GetComponent<VNCScreen>();
            }
            else
            {
            //    line.color = Color.cyan;
            }
        }
        else
        {
            touchedCollider = null;
            vnc = null;
      //      line.color = Color.cyan;
            endLine.position = startLine.position + startLine.forward * maxDistance;
        }

        //    controller.GetPressDown()

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

    }
}
