    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Threading; 
    using System.Timers; 
    using NetMQ; // for NetMQConfig
    using NetMQ.Sockets; 
    using UnityEngine.UI;

    

    public class receiveFromScript : MonoBehaviour
    {
        // Start is called before the first frame update
        
        Thread client_thread_;
        private Object thisLock_ = new Object();
        bool stop_thread_ = false;
        public RawImage image; 
        Texture2D tex; 

        void Start()
        {
            tex = new Texture2D(2,2,TextureFormat.RGB24, mipChain: false); 
            image.texture = tex; 


            Debug.Log("Start a request thread.");
            client_thread_ = new Thread(NetMQClient);
            client_thread_.Start();
        }

        // Client thread which does not block Update()
        void NetMQClient()
        {

            AsyncIO.ForceDotNet.Force();
            NetMQConfig.ManualTerminationTakeOver();
            NetMQConfig.ContextCreate(true);

            string msg;
            byte[] data; 
            var timeout = new System.TimeSpan(0, 0, 10); //3sec

            Debug.Log("Connect to the server.");
            var requestSocket = new SubscriberSocket (); 
            requestSocket.Connect("tcp://172.26.189.111:12345");
            requestSocket.Subscribe(""); 

            string endpoint = requestSocket.Options.LastEndpoint;

            // Display the endpoint address
            Debug.Log("Connected to: " + endpoint);
            
            //requestSocket.SendFrame("SUB_PORT");

            //bool is_connected = requestSocket.TryReceiveFrameString(timeout, out msg);    
            bool is_connected = requestSocket.TryReceiveFrameBytes(timeout, out data);         

           //Debug.Log("is_connected: " + is_connected); 
            //Debug.Log(msg); 
            
            //byte[] bytebuffer = Convert.FromBase64String(msg); 
            tex.LoadImage(data); 


            while (is_connected && stop_thread_ == false)
            {
                Debug.Log("Request a message.");
                //requestSocket.SendFrame("msg");
                //is_connected = requestSocket.TryReceiveFrameString(timeout, out msg);
                is_connected = requestSocket.TryReceiveFrameBytes(timeout, out data);         
                tex.LoadImage(data); 

                Debug.Log("Sleep");
                Thread.Sleep(1000);
            }

            requestSocket.Close();
            Debug.Log("ContextTerminate.");
            NetMQConfig.ContextTerminate();
        }

        void Update()
        {
            /// Do normal Unity stuff
        }

        void OnApplicationQuit()
        {
            lock (thisLock_)stop_thread_ = true;
            client_thread_.Join();
            Debug.Log("Quit the thread.");
        }

        private void OnDestroy()    
        {
            lock (thisLock_)stop_thread_ = true;
            client_thread_.Join();
            Debug.Log("Quit the thread.");

            NetMQConfig.Cleanup();
        }


    }