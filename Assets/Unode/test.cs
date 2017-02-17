using UnityEngine;
using System.Collections;

#if TEST

public class test : MonoBehaviour {

	public vncclient vnc;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.F1)){
			vnc.connect();
		}
	}
}


#endif