using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVncSharp.Unity;

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
        onStateChanged(VNCScreen.RuntimeState.Disconnected);
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
