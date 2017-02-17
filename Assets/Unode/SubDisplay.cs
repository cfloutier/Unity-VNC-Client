using UnityEngine;
using System.Collections;

#if TEST

public class SubDisplay : MonoBehaviour {

	public vncclient vnc;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(vnc.rect){
			gameObject.GetComponent<Renderer>().material.mainTexture = vnc.img;
		}
	}
}


#endif