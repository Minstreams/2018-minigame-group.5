using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyDiscovery : NetworkDiscovery
{
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        base.OnReceivedBroadcast(fromAddress, data);
        Debug.Log("Receive boardcast! [address:" + fromAddress + "][data:" + data + "]");
        NetworkManager.singleton.networkAddress = fromAddress;
        NetworkManager.singleton.StartClient();
    }

    private void OnConnectedToServer()
    {
        Debug.Log("OnConnectedToServer");
    }
}
