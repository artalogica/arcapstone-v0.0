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

    public OVRInput.Controller leftController;
    public OVRInput.Controller rightController;
    private ControllerState stateStore;

    private void Start()
    {
        ForceDotNet.Force();
        socket = new PushSocket();
        socket.Bind("tcp://*:23456");
        stateStore = new ControllerState(leftController, rightController);
    }

    private void Update()
    {
        if (socket != null && stateStore != null){
            stateStore.UpdateState();
            try
            {
                socket.SendFrame(stateStore.ToJSON());
            }
            catch (NetMQException ex)
            {
                Debug.LogError($"Error sending frame: {ex.Message}");
            }
        }
    }

    private void OnDestroy()
    {
        var terminationString = "terminate";
        for (int i = 0; i < 10; i++)
        {
            socket.SendFrame(terminationString);
        }
        socket.Close();
        NetMQConfig.Cleanup();
    }
    private void OnApplicationPause()
    {
        var terminationString = "terminate";
        for (int i = 0; i < 10; i++)
        {
            socket.SendFrame(terminationString);
        }
        socket.Close();
        NetMQConfig.Cleanup();
    }
}