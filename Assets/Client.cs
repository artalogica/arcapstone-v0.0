using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct Data
{
    public bool bool_;
    public int int_;
    public string str;
    public byte[] image;
}

public class ReceiverOneWay
{
    private readonly Thread receiveThread;
    private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);
    //private readonly object lockObj = new object();


    public ReceiverOneWay()
    {
        receiveThread = new Thread((object callback) =>
        {
            using (var socket = new SubscriberSocket())  // <- The PULL socket
            {
                //try and catch 
                socket.Connect("tcp://172.26.189.111:12345");
                var timeout = new System.TimeSpan(0, 0, 3); //3sec

                Debug.Log("Connected to server"); 


                if(socket != null){

                    socket.Subscribe(""); 
                
                    var localEndpoint = socket.Options.LastEndpoint;
                    Debug.Log(localEndpoint); 

                    Data data = new Data(); 

                    bool is_connected = socket.TryReceiveFrameBytes(timeout, out data.image); 

                    while (is_connected && !stopEvent.WaitOne(0))
                    {

                        // data = new Data(); 
                        is_connected = socket.TryReceiveFrameBytes(timeout, out data.image); 
                        data.str = "frame loaded : )"; 
                        //Debug.Log("received image"); 
                        ((Action<Data>)callback)(data);
                        
                    }
                }
                else{
                    Debug.Log("failed to connect to socket"); 
                }
            }
        });
    }

    public void Start(Action<Data> callback)
    {
        //running = true;
        receiveThread.Start(callback);
    }

    public void Stop()
    {
        //running = false;
        stopEvent.Set(); 
        receiveThread.Join();
    }
}

public class Client : MonoBehaviour
{
    private readonly ConcurrentQueue<Action> runOnMainThread = new ConcurrentQueue<Action>();
    private ReceiverOneWay receiver;
    private Texture2D tex;
    public RawImage image;
    private readonly object lockObj = new object();


    public void Start()
    {
        tex = new Texture2D(2, 2, TextureFormat.RGB24, mipChain: false);
        image.texture = tex;

        AsyncIO.ForceDotNet.Force();
        // - You might remove it, but if you have more than one socket
        //   in the following threads, leave it.
        receiver = new ReceiverOneWay();
        // receiver.Start((Data d) => runOnMainThread.Enqueue(() =>
        //     {   
        //         if (d.str != null){
        //             //Debug.Log(d.str);
        //             tex.LoadImage(d.image);
        //         }
                
        //     }
        // ));

        receiver.Start((Data d) =>
        {
            lock (lockObj)
            {
                runOnMainThread.Enqueue(() =>
                {
                    if (d.str != null)
                    {
                        //Debug.Log(d.str);
                        tex.LoadImage(d.image);
                    }

                });
            }
        });
    }

    public void Update()
    {
        lock(lockObj){
            if (!runOnMainThread.IsEmpty)
            {
                Action action;
                while (runOnMainThread.TryDequeue(out action))
                {
                    action.Invoke();
                }
            }
        }
    }

    private void OnDestroy()
    {
        receiver.Stop();
        NetMQConfig.Cleanup();  // Must be here to work more than once
        //NetMQConfig.ContextTerminate(); 
    }
}