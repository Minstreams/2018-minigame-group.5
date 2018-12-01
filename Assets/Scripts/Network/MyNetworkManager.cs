using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkManager : NetworkManager
{

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnClientDisconnect");
        base.OnClientDisconnect(conn);
        GameSystem.TheMatrix.SendGameMessage(GameSystem.GameMessage.Quit);
    }

    private void OnServerInitialized()
    {
        Debug.Log("OnServerInitialized");
    }

    private void OnConnectedToServer()
    {
        Debug.Log("OnConnectedToServer");
    }
    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("OnServerConnect");
        base.OnServerConnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("OnServerAddPlayer [id:" + playerControllerId + "]");
        base.OnServerAddPlayer(conn, playerControllerId);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnServerDisconnect");
        base.OnServerDisconnect(conn);
    }
}
