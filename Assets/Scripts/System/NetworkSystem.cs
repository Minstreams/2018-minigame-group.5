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
        private static void StartHost()
        {
            discovery.Initialize();
            discovery.StartAsServer();
            NetworkManager.singleton.StartHost();
        }
        public static void Start()
        {
            discovery.Initialize();
            discovery.StartAsClient();
            StartCoroutine(Waiting());
        }

        private static IEnumerator Waiting()
        {
            yield return new WaitForSeconds(2);
            Debug.Log(discovery.broadcastsReceived);
            if (discovery.broadcastsReceived != null && discovery.broadcastsReceived.Count == 0)
            {
                discovery.StopBroadcast();
                StartHost();
            }
        }

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
    }
}
