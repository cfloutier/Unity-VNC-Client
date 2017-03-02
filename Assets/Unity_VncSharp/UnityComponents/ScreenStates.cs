using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityVncSharp.Unity;

public class ScreenStates : MonoBehaviour
{
    public VNCScreen screen;
    void Start()
    {
        if (screen == null)
            screen = GetComponent<VNCScreen>();

        if (screen == null)
            screen = GetComponentInParent<VNCScreen>();


        Debug.Assert(screen != null);

        screen.onStateChanged_event += Screen_onStateChanged_event;
    }

    private void Screen_onStateChanged_event(VNCScreen.RuntimeState state)
    {
        switch (state)
        {
            case VNCScreen.RuntimeState.Disconnected:
                break;
            case VNCScreen.RuntimeState.Disconnecting:
                break;
            case VNCScreen.RuntimeState.Connected:
                break;
            case VNCScreen.RuntimeState.Connecting:
                break;
            case VNCScreen.RuntimeState.Error:
                break;
            default:
                break;
        }

    }
}
