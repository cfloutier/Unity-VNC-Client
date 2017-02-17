using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;


namespace UnityVncSharp.Unity
{
    public class VNCScreen : MonoBehaviour
    {
        public string host;
        public int port = 5900;
        public string password;

        public bool connectOnStartUp;

        // Use this for initialization
        void Start()
        {
            if (connectOnStartUp)
            {
                connect();
            }


            // Create a texture
            Texture2D tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
            // Set point filtering just so we can see the pixels clearly
            tex.filterMode = FilterMode.Point;
            // Call Apply() so it's actually uploaded to the GPU
            tex.Apply();

            // Set texture onto our material
            GetComponent<Renderer>().material.mainTexture = tex;
        }

        RemoteDesktop vncDesktop;

    




        public void connect()
        {
            /*     try
                 {
                     TcpClient client = new TcpClient();
                     client.Connect(host, port);
                 }
                 catch (Exception e)
                 {
                     Debug.LogException(e);
                 }*/

            if (vncDesktop == null)
                vncDesktop = new RemoteDesktop();

            if (vncDesktop.IsConnected)
            {
                Disconnect();
            }

            vncDesktop.ConnectionLost += VncDesktop_ConnectionLost;
            vncDesktop.ConnectComplete += VncDesktop_ConnectComplete;

            vncDesktop.GetPassword = GetPassword;
            vncDesktop.port = port;
            vncDesktop.Connect(host, 0);
        }

        private void VncDesktop_ConnectComplete(object sender, ConnectEventArgs e)
        {
            if (vncDesktop == null)
            {
                Debug.LogError("Should not happen");
                return;
            }

            // Create a texture
            Texture2D tex = vncDesktop.texture;

            // Set point filtering just so we can see the pixels clearly
           /* tex.filterMode = FilterMode.Point;
            // Call Apply() so it's actually uploaded to the GPU
            tex.Apply();*/

            // Set texture onto our material
            GetComponent<Renderer>().material.mainTexture = tex;

        }

        private void VncDesktop_ConnectionLost(object sender, EventArgs e)
        {
            
            if (e is ErrorEventArg)
            {
                var error = e as ErrorEventArg;
                 
                if (error.Exception != null)
                {
                    Debug.LogException(error.Exception);
                }
                else if (!string.IsNullOrEmpty(error.Reason))
                {
                    Debug.Log(error.Reason);
                }
                
            }
            else
            {
                Debug.Log("VncDesktop_ConnectionLost");
            }
        }

        public void Disconnect()
        {

        }

        public bool isConnected
        {
            get
            {
                if (vncDesktop == null) return false;

                return vncDesktop.IsConnected;
            }

        }



        string GetPassword()
        {
            return password;
        }

        // Update is called once per frame
        void Update()
        {
            if (vncDesktop != null && vncDesktop.IsConnected)
                vncDesktop.popUpdates();

            GetComponent<Renderer>().sharedMaterial.mainTexture = vncDesktop.texture;

        }

        void OnApplicationQuit()
        {
            if (vncDesktop != null && vncDesktop.IsConnected)
                vncDesktop.Disconnect();
        }

    }



}
