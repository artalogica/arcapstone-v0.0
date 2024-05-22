using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using AsyncIO;

public class Sender : MonoBehaviour
{
    private DateTime today;
    private PushSocket socket;

    private bool stopThread = false; 
    Thread server_thread_; 
    
    private UnityEngine.Object thisLock_ = new UnityEngine.Object();

    public OVRInput.Controller leftController;
    public OVRInput.Controller rightController;
    private ControllerState stateStore;

    private void Start()
    {
        server_thread_ = new Thread(NetMQServer); 
        server_thread_.Start(); 
        Debug.Log("Becoming a SENDER!"); 


        // ForceDotNet.Force();
        // socket = new PushSocket();
        // socket.Bind("tcp://192.168.1.175:23456");
        // stateStore = new ControllerState(leftController, rightController);
    }

    void NetMQServer(){
         AsyncIO.ForceDotNet.Force(); 
        NetMQConfig.ManualTerminationTakeOver();
        NetMQConfig.ContextCreate(true);

        using (var socket = new PublisherSocket()){
        socket.Bind("tcp://*:23456");

        while(!stopThread){
            Debug.Log("sending stuff : )"); 

            stateStore.UpdateState(); 
            socket.SendFrame(stateStore.ToJSON()); 
            Thread.Sleep(500); 
        }
        
        }

    }
    private void Update()
    {
        // if (socket != null && stateStore != null){
        //     stateStore.UpdateState();
        //     try
        //     {
        //         socket.SendFrame(stateStore.ToJSON());
        //     }
        //     catch (NetMQException ex)
        //     {
        //         Debug.LogError($"Error sending frame: {ex.Message}");
        //     }
        // }
    }

    private void OnDestroy()
    {
        lock (thisLock_)stopThread = true;
        server_thread_.Join();
        Debug.Log("Quit the thread.");
        // var terminationString = "terminate";
        // for (int i = 0; i < 10; i++)
        // {
        //     socket.SendFrame(terminationString);
        // }
        // socket.Close();
        NetMQConfig.Cleanup();
    }
    private void OnApplicationPause()
    {
        lock (thisLock_)stopThread = true;
        server_thread_.Join();
        Debug.Log("Quit the thread.");
        // var terminationString = "terminate";
        // for (int i = 0; i < 10; i++)
        // {
        //     socket.SendFrame(terminationString);
        // }
        // socket.Close();
        NetMQConfig.Cleanup();
    }
}