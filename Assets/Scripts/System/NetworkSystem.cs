using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.PresentSetting;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace GameSystem
{
    public class NetworkSystem : SubSystem<NetworkSystemSetting>
    {
        private static NetworkDiscovery _discovery;
        public static NetworkDiscovery discovery
        {
            get
            {
                if (_discovery == null)
                {
                    _discovery = NetworkManager.singleton.GetComponent<NetworkDiscovery>();
                }
                return _discovery;
            }
        }
        public static void StartHost()
        {
            discovery.Initialize();
            discovery.StartAsServer();
            NetworkManager.singleton.StartHost();
        }
        public static void StartClient()
        {
            discovery.Initialize();
            discovery.StartAsClient();
        }

        //private static IEnumerator Waiting()
        //{
        //    yield return new WaitForSeconds(2);
        //    if (discovery.broadcastsReceived != null && discovery.broadcastsReceived.Count == 0)
        //    {
        //        discovery.StopBroadcast();
        //        StartHost();
        //    }
        //    else
        //    {
        //        discovery.StopBroadcast();
        //    }
        //}

        public static void QuitNetworking()
        {
            if (discovery.isServer)
            {
                discovery.StopBroadcast();
                NetworkManager.singleton.StopHost();
            }
            else
            {
                NetworkManager.singleton.StopClient();
            }
        }

        //Property
        public static bool IsServer { get { return PenguinController.localPenguin == null ? false : PenguinController.localPenguin.isServer; } }

        public static List<PenguinController> playerList = new List<PenguinController>();
    }
}
