using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneRealms.Scripts.Systems
{
    public class NetworkManagerHelper : NetworkBehaviour
    {
        public static NetworkManagerHelper Instance { private set; get; }
        
        public event Action OnClientConnectCallback;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnect;
            NetworkManager.OnServerStarted += () =>
            {
                NetworkManager.SceneManager.OnLoadEventCompleted += OnClientsLoadCompleted;
            };
        }
        
        private void OnClientsLoadCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            Debug.Log($" SceneName {sceneName} clients: {clientsCompleted.ToArray()}");
        }

        private void OnClientConnect(ulong client)
        {
            if (IsServer)
            {
                Debug.Log($"New client connected: {client}");
                if (ConnectedClients >= 2)
                {
                    Debug.Log("2 players connected");
                }
                
                OnClientConnectCallback?.Invoke();
            }
        }

        public void LoadMainGameScene()
        {
            NetworkManager.SceneManager.LoadScene("TestScene", LoadSceneMode.Single);
        }


        public new bool IsServer { get { return base.IsServer; } }
        public new bool IsHost { get { return base.IsHost; } }
        public new bool IsClient { get { return base.IsClient; } }
        public int ConnectedClients { get {return NetworkManager.ConnectedClients.Count;}}

    }
}