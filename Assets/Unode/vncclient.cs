using UnityEngine;
using System.Collections;
using System.Collections.Generic;



#if TEST

using WebSocketSharp;
using MiniMessagePack;

public class vncclient : MonoBehaviour {
	public WebSocket ws;
	public string ws_adress;
	public int ws_port;
	public string ws_path;

	public string vnc_ip;
	public string vnc_port;
	public string password;
	private Unode_v1_3 unode;

	private string ObjectName;

	MiniMessagePacker pakage;
	private Dictionary<string,object>	Msgpack,packed_data;
	public Dictionary<string,object> vnc;
	public Texture2D img;

	public bool rect = false;
	public bool connected = false;
	public float timer = 0;
	public int updateFream = -1;

	public float interval;
	// Use this for initialization

	private object data;

	private pointer point;
	
	void Awake() {

	}
	
	void Start () {
		unode = GameObject.Find ("Unode_v1_3").GetComponent<Unode_v1_3>();
		ws_path = '/'+gameObject.name;
		unode.RegistNodeModule(gameObject.name,"VncClient.js");
		var option = new Dictionary<string, object> {
			{ "port", ws_port },
			{ "path", ws_path }
		};
		unode.SendToNodeModule(gameObject.name,option);
		vnc = new Dictionary<string, object> {
			{ "name", string.Empty },
			{ "width", 0 },
			{ "height", 0 },
		};
		gameObject.AddComponent<pointer>();
		point = gameObject.GetComponent<pointer>();


		ObjectName = gameObject.name;
		tmp_pos = point.pos;


		ws = new WebSocket(ws_adress+":"+ws_port+ws_path);
		ws.Connect ();

		ws.OnOpen += (sender, e) => {
			Debug.Log ("ws.OnOpen:");
		};
		
		ws.OnMessage += (sender, e) => {
			switch(e.Type){
			case Opcode.Binary:
				try{
					Msgpack = unode.MessagePackDecode(e.RawData) as Dictionary<string,object>;

					if(Msgpack.TryGetValue("mode",out data)){
						switch((string)data){
							case "connected":
								Debug.Log ("Vnc server Connected.");
								vnc["name"] = (string)Msgpack["name"];
								vnc["width"] = (long)Msgpack["width"];
								vnc["height"] = (long)Msgpack["height"];
								point.setSize((float)(long)vnc["width"],(float)(long)vnc["height"]);
								img = new Texture2D((int)(long)vnc["width"], (int)(long)vnc["height"]);
								connected = true;
								SetUpdateFream();
								break;
							case "rect":
								rect = true;
								break;
						}
					}else{
						Debug.Log("error"+"["+ObjectName+"]"+":mode::"+e.RawData.Length);
					}
				}catch{
					Debug.Log("error:"+"["+ObjectName+"]"+"Msgpack");
				}
				break;
			case Opcode.Text:
				Debug.Log("TextMesaage:"+e.Data);
				break;
			}
		};
		
		ws.OnError += (object sender, ErrorEventArgs e) => {
			Debug.Log ("OnError"+"["+ObjectName+"]"+ e.Message);
		};
		
		ws.OnClose += (object sender, CloseEventArgs e) => {
			Debug.Log ("OnClosed"+"["+ObjectName+"]"+ e.Reason);
		};	

		StartCoroutine("Updatefream");
	}

	private Vector3 tmp_pos;
	// Update is called once per frame
	void Update () {
		if(connected){
			if(rect){
				//				Debug.Log("Update");
				rect = false;
				img.LoadImage((byte[])Msgpack["data"]);
				//img = loadtexture( (byte[])Msgpack["data"], (long)Msgpack["width"], (long)Msgpack["heigth"]);
				gameObject.GetComponent<Renderer>().material.mainTexture = img;
				if(updateFream == 1)
					updateRequest();
			}
			if(updateFream == 1){

				if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)){
					PointerEvent(-1);
				}
				if(Input.anyKeyDown && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)) == false){
						KeyEvent(1);
				}
			}
		}
	}
	
	private IEnumerator Updatefream(){
		while(true){
			if(connected){



				if(updateFream == 1){
					//Debug.Log("update");
					//updateRequest();
					if (Input.GetMouseButton (0)) {
						PointerEvent(0);
					}else if (Input.GetMouseButton (1)) {
						PointerEvent(1);
					}else if (Input.GetMouseButton (2)) {
						PointerEvent(2);
					}else{
						PointerEvent(-1);
					}
					if(Input.GetAxis("Mouse ScrollWheel") > 0){
						PointerEvent(3);
						PointerEvent(-1);
					}
					if(Input.GetAxis("Mouse ScrollWheel") < 0){
						PointerEvent(4);
						PointerEvent(-1);
					}
					//if(!(Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)))
						

					//if((point.pos != tmp_pos) && !(Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))){
					//	tmp_pos = point.pos;
					//	PointerEvent(-1);
					//}
				}
			}
			yield return new WaitForSeconds (interval);
		}
	}


	void OnApplicationQuit() {
		if (ws != null)
			ws.Close ();
	}

	public void connect(){
		if(connected == false){
			var packed_data = new Dictionary<string, object> {
				{ "mode", "connect" },
				{ "ip", vnc_ip },
				{ "port", vnc_port },
				{ "password", password },
			};
			unode.send(ws,packed_data);
		}
	}

	public void updateRequest(){
		var packed_data = new Dictionary<string, object> {
			{ "mode", "update" }
		};
		unode.send(ws,packed_data);
	}

	public void KeyEvent(int on){
		Debug.Log("key");
		var packed_data = new Dictionary<string, object> {
			{ "mode", "key" },
			{ "on"  , on},
			{ "code",  getKeyCode() },
		};
		unode.send(ws,packed_data);
	}

	public void PointerEvent(long mask){
		var packed_data = new Dictionary<string, object> {
			{ "mode", "pointer" },
			{ "mask",  mask},
			{ "x"   ,  Mathf.Clamp((int)point.pos.x,0,(int)(long)vnc["width"]) },
			{ "y"   ,  Mathf.Clamp((int)point.pos.y,0,(int)(long)vnc["height"]) }
		};
		unode.send(ws,packed_data);
	}

	public void SetUpdateFream(){
		updateFream *= -1;
		updateRequest();
	}

	private Texture2D loadtexture(byte[] image,long width,long height){
		Texture2D texture = new Texture2D((int)width, (int)height);
		texture.LoadImage(image);
		
		return texture;
	}

	public string getKeyCode(){
		if(Input.GetKeyDown(KeyCode.Backspace)){
			return "0xff08";
		}
		if(Input.GetKeyDown(KeyCode.Tab)){
			return "0xff09";
		}
		if(Input.GetKeyDown(KeyCode.Return)){
			return "0xff0d";
		}
		if(Input.GetKeyDown(KeyCode.Escape)){
			return "0xff1b";
		}
		if(Input.GetKeyDown(KeyCode.Delete)){
			return "0xffff";
		}
		if(Input.GetKeyDown(KeyCode.Insert)){
			return "0xff9e";
		}
		if(Input.GetKeyDown(KeyCode.PageUp)){
			return "0xff97";
		}
		if(Input.GetKeyDown(KeyCode.PageDown)){
			return "0xff99";
		}
		if(Input.GetKeyDown(KeyCode.LeftArrow)){
			return "0xff96";
		}
		if(Input.GetKeyDown(KeyCode.RightArrow)){
			return "0xff98";
		}
		if(Input.GetKeyDown(KeyCode.Backslash)){
			return "0x005c";
		}
		if(Input.GetKeyDown(KeyCode.Minus)){
			return "0x002d";
		}
		if(Input.GetKeyDown(KeyCode.Underscore)){
			return "0x00ad";
		}
		if(Input.GetKeyDown(KeyCode.Space)){
			return "0xff80";
		}
		if (Input.GetKey (KeyCode.LeftShift)) { //大文字
			if (Input.GetKeyDown (KeyCode.A)) {
					return "0x0041";
			}
			if (Input.GetKeyDown (KeyCode.B)) {
					return "0x0042";
			}
			if (Input.GetKeyDown (KeyCode.C)) {
					return "0x0043";
			}
			if (Input.GetKeyDown (KeyCode.D)) {
					return "0x0044";
			}
			if (Input.GetKeyDown (KeyCode.E)) {
					return "0x0045";
			}
			if (Input.GetKeyDown (KeyCode.F)) {
					return "0x0046";
			}
			if (Input.GetKeyDown (KeyCode.G)) {
					return "0x0047";
			}
			if (Input.GetKeyDown (KeyCode.H)) {
					return "0x0048";
			}
			if (Input.GetKeyDown (KeyCode.I)) {
					return "0x0049";
			}
			if (Input.GetKeyDown (KeyCode.J)) {
					return "0x004a";
			}
			if (Input.GetKeyDown (KeyCode.K)) {
					return "0x004b";
			}
			if (Input.GetKeyDown (KeyCode.L)) {
					return "0x004c";
			}
			if (Input.GetKeyDown (KeyCode.M)) {
					return "0x004d";
			}
			if (Input.GetKeyDown (KeyCode.N)) {
					return "0x004e";
			}
			if (Input.GetKeyDown (KeyCode.O)) {
					return "0x004f";
			}
			if (Input.GetKeyDown (KeyCode.P)) {
					return "0x0050";
			}
			if (Input.GetKeyDown (KeyCode.Q)) {
					return "0x0051";
			}
			if (Input.GetKeyDown (KeyCode.R)) {
					return "0x0052";
			}
			if (Input.GetKeyDown (KeyCode.S)) {
					return "0x0053";
			}
			if (Input.GetKeyDown (KeyCode.T)) {
					return "0x0054";
			}
			if (Input.GetKeyDown (KeyCode.U)) {
					return "0x0055";
			}
			if (Input.GetKeyDown (KeyCode.V)) {
					return "0x0056";
			}
			if (Input.GetKeyDown (KeyCode.W)) {
					return "0x0057";
			}
			if (Input.GetKeyDown (KeyCode.X)) {
					return "0x0058";
			}
			if (Input.GetKeyDown (KeyCode.Z)) {
					return "0x0059";
			}
			if (Input.GetKeyDown (KeyCode.Z)) {
					return "0x005a";
			}
		} else { //小文字
			if (Input.GetKeyDown (KeyCode.A)) {
				return "0x0061";
			}
			if (Input.GetKeyDown (KeyCode.B)) {
				return "0x0062";
			}
			if (Input.GetKeyDown (KeyCode.C)) {
				return "0x0063";
			}
			if (Input.GetKeyDown (KeyCode.D)) {
				return "0x0064";
			}
			if (Input.GetKeyDown (KeyCode.E)) {
				return "0x0065";
			}
			if (Input.GetKeyDown (KeyCode.F)) {
				return "0x0066";
			}
			if (Input.GetKeyDown (KeyCode.G)) {
				return "0x0067";
			}
			if (Input.GetKeyDown (KeyCode.H)) {
				return "0x0068";
			}
			if (Input.GetKeyDown (KeyCode.I)) {
				return "0x0069";
			}
			if (Input.GetKeyDown (KeyCode.J)) {
				return "0x006a";
			}
			if (Input.GetKeyDown (KeyCode.K)) {
				return "0x006b";
			}
			if (Input.GetKeyDown (KeyCode.L)) {
				return "0x006c";
			}
			if (Input.GetKeyDown (KeyCode.M)) {
				return "0x006d";
			}
			if (Input.GetKeyDown (KeyCode.N)) {
				return "0x006e";
			}
			if (Input.GetKeyDown (KeyCode.O)) {
				return "0x006f";
			}
			if (Input.GetKeyDown (KeyCode.P)) {
				return "0x0070";
			}
			if (Input.GetKeyDown (KeyCode.Q)) {
				return "0x0071";
			}
			if (Input.GetKeyDown (KeyCode.R)) {
				return "0x0072";
			}
			if (Input.GetKeyDown (KeyCode.S)) {
				return "0x0073";
			}
			if (Input.GetKeyDown (KeyCode.T)) {
				return "0x0074";
			}
			if (Input.GetKeyDown (KeyCode.U)) {
				return "0x0075";
			}
			if (Input.GetKeyDown (KeyCode.V)) {
				return "0x0056";
			}
			if (Input.GetKeyDown (KeyCode.W)) {
				return "0x0057";
			}
			if (Input.GetKeyDown (KeyCode.X)) {
				return "0x0078";
			}
			if (Input.GetKeyDown (KeyCode.Z)) {
				return "0x0079";
			}
			if (Input.GetKeyDown (KeyCode.Z)) {
				return "0x007a";
			}
		}

		if (Input.GetKeyDown (KeyCode.Keypad0)) {
			return "0xffb0";
		}
		if (Input.GetKeyDown (KeyCode.Keypad1)) {
			return "0xffb1";
		}
		if (Input.GetKeyDown (KeyCode.Keypad2)) {
			return "0xffb2";
		}
		if (Input.GetKeyDown (KeyCode.Keypad3)) {
			return "0xffb3";
		}
		if (Input.GetKeyDown (KeyCode.Keypad4)) {
			return "0xffb4";
		}
		if (Input.GetKeyDown (KeyCode.Keypad5)) {
			return "0xffb5";
		}
		if (Input.GetKeyDown (KeyCode.Keypad6)) {
			return "0xffb6";
		}
		if (Input.GetKeyDown (KeyCode.Keypad7)) {
			return "0xffb7";
		}
		if (Input.GetKeyDown (KeyCode.Keypad8)) {
			return "0xffb8";
		}
		if (Input.GetKeyDown (KeyCode.Keypad9)) {
			return "0xffb9";
		}

		return string.Empty;

	}
}
#endif