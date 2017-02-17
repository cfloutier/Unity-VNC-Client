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
            vncDesktop.GetPassword = GetPassword;
            vncDesktop.port = port;
            vncDesktop.Connect(host, 0);
        }

        private void VncDesktop_ConnectionLost(object sender, EventArgs e)
        {
            Debug.Log("Disconnection");
            if (e is ErrorEventArg)
            {
                var error = e as ErrorEventArg;

                if (!string.IsNullOrEmpty(error.Reason))
                {
                    Debug.Log(error.Reason);
                }
                if (error.Exception != null)
                {
                    Debug.LogException(error.Exception);
                }
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



        }
    }



}
