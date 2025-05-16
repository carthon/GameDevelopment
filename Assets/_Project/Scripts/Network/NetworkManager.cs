using System;
using System.Collections.Generic;
using System.Text;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.Client;
using _Project.Scripts.Network.Server;
using _Project.Scripts.Utils;
using RiptideNetworking.Utils;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.Network {
    public class NetworkManager : MonoBehaviour {
        private static NetworkManager _singleton;
        public DictionaryOfStringAndItems itemsDictionary;
        public static Dictionary<ushort, Player> playersList = new Dictionary<ushort, Player>();
        public int MessagesSent = 0;
        public int MessagesReceived = 0;
        public static NetworkManager Singleton
        {
            get => _singleton;
            private set {
                if (_singleton == null)
                    _singleton = value;
                else if(_singleton != null) {
                    Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }
        public bool IsServer { get; private set; }
        public bool IsClient { get; private set; }

        [SerializeField] public ushort port;
        [SerializeField] public string hostAddress;
        [SerializeField] private ushort maxClientCount;
        
        [SerializeField] public bool debugServerPosition = true;
        [SerializeField] public GameObject serverDummyPlayerPrefab;
        private GameObject _serverDummyPlayer;
        public GameObject ServerDummy { get => _serverDummyPlayer; set => _serverDummyPlayer = value; }

        private float _timer;
        private int _currentTick;
        public float minTimeBetweenTicks;
        public const int BufferSize = 1024;
        
        public ServerHandler ServerHandler { get; private set; }
        public ClientHandler ClientHandler { get; set; }  
        
        public int Tick { get => _currentTick; set => _currentTick = value; }

        void Awake() {
            Singleton = this;
            var sender = new RiptideNetworkSender(this);
            ServerHandler = new ServerHandler(sender);
            ClientHandler = new ClientHandler(sender);
        }
        private void OnValidate() {
            Item[] items = GameData.Singleton != null ? GameData.Singleton.items : null;
            Debug.Log($"Loaded {items?.Length} items");
            itemsDictionary = new DictionaryOfStringAndItems();
            if (items != null)
                foreach (Item item in items) {
                    itemsDictionary.Add(item.id, item);
                }
        }
        public void Start() {
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
            Logger.Initialize();
            minTimeBetweenTicks = Time.fixedDeltaTime;
#if UNITY_SERVER
            InitializeServer();
#endif
        }
        private void Update() {
            _timer += Time.deltaTime;
            while(_timer >= minTimeBetweenTicks) {
                _timer -= minTimeBetweenTicks;
                if (IsClient || IsServer) {
                    if (IsServer) {
                        ServerHandler.Tick(_currentTick);
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (var player in playersList.Values) {
                            stringBuilder.Append($"Player{player.Id}:{player.GetMovementState(Tick).ToString()}");
                        }
                        UIHandler.Instance.UpdateWatchedVariables("PlayersInfo", stringBuilder.ToString());
                    }
                    if (IsClient) {
                        ClientHandler.Tick(_currentTick);
                    }
                    Physics.Simulate(Singleton.minTimeBetweenTicks);
                }
                _currentTick++;
            }
        }
        public void InitializeServer() {
            IsServer = true;
            ServerHandler.Start(port, maxClientCount);
            ServerHandler.ClientDisconnected += ServerHandler.PlayerLeft;
        }
#if !UNITY_SERVER
        public void InitializeClient() {
            IsClient = true;
            ClientHandler.IsServerOwner = IsServer;
            ClientHandler.Connect($"{hostAddress}:{port}");
        }
        public void StopClient() {
            if (IsClient) {
                IsClient = false;
                ClientHandler.Disconnect();
            }
        }
#endif
        public void StopServer() {
            if (IsServer) {
                ServerHandler.Stop();
                IsServer = false;
            }
        }
        private void MessageReceived(object sender, EventArgs args) { MessagesReceived++; }
        private void OnApplicationQuit() {
            StopServer();
#if !UNITY_SERVER
            StopClient();
#endif
        }
    }
}
