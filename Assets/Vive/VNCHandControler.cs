using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class VNCHandControler : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
		
	}

    void Awake()
    {

        StartCoroutine(StartUp());
    }


    IEnumerator StartUp()
    {
        Hand hand = GetComponentInParent<Hand>();
        Debug.AssertFormat(hand != null, "VNCHandControler shoudl be place child of a Hand");

       

        yield return new WaitForEndOfFrame();

    }
	
	// Update is called once per frame
	void Update ()
    {


		
	}
}
