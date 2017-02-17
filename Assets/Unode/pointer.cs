using UnityEngine;
using System.Collections;

#if TEST


public class pointer : MonoBehaviour {
	private Ray ray;
	private RaycastHit  hit;

	public Vector3 hit_pos;
	public Vector3 size;
	public Vector3 pos;

	public float width=1,height=1;
	
	private vncclient vnc; 

	public GameObject obj;

	void Awake() {
		Cursor.visible = true;	
		obj = GameObject.FindGameObjectWithTag ("pointer");
	}

	void Update () {
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000))
        {
            hit_pos = hit.point;
            pos = resize(hit.collider.gameObject.transform, hit.point, new Vector3(width, height, 0));
            if (Input.GetMouseButtonUp(0))
            {   
                Debug.Log("hit:" + hit.collider.name);
                /*
				vnc = hit.collider.gameObject.GetComponent<vncclient>();
				if(vnc.connected == false){
					vnc.connect();
					vnc.connected = true;
					vnc.SetUpdateFream();
				}
				*/

            }

        }
        /*
		if(Input.GetMouseButtonDown(1)){
			if(!vnc.connected){
				vnc.SetUpdateFream();
			}
		}
		*/

        obj.transform.position = new Vector3(hit_pos.x, hit_pos.y, transform.position.z);
   

}

	Vector3 resize(Transform obj,Vector3 hit,Vector3 src){
		size = new Vector3 (10.0f*obj.localScale.x, 10.0f*obj.localScale.y, 10.0f*obj.localScale.z);
		float x = Mathf.Abs((hit.x-obj.position.x) + size.x/2)*(src.x/size.x);
		float y = Mathf.Abs((hit.y-obj.position.y) - size.y/2)*(src.y/size.y);
		return new Vector3 (x,y,0);
	}

	public void setSize(float w,float h){
		width = w;
		height = h;
	}
}


#endif