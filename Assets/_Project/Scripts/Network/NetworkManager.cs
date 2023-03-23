using System;
using System.Collections.Generic;
using System.Text;
using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;
using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.Network {
    public class NetworkManager : MonoBehaviour {
        private static NetworkManager _singleton;
        public DictionaryOfStringAndItems itemsDictionary;
        public static Dictionary<ushort, Player> playersList = new Dictionary<ushort, Player>();
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
        
        public Server.Server Server { get; private set; }
        public Client.Client Client { get; private set; }  
        
        public int Tick { get => _currentTick; set => _currentTick = value; }

        void Awake() {
            _singleton = this;
        }
        private void OnValidate() {
            Item[] items = Resources.LoadAll<Item>("Items");
            Debug.Log($"Loaded {items.Length} items");
            itemsDictionary = new DictionaryOfStringAndItems();
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
                if (IsServer) {
                    Server.Tick(_currentTick);
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var player in playersList.Values) {
                        stringBuilder.Append($"Player{player.Id}:{player.GetMovementState(Tick).ToString()}");
                    }
                    UIHandler.Instance.UpdateWatchedVariables("PlayersInfo", stringBuilder.ToString());
                }
                if (IsClient) {
                    Client.Tick(_currentTick);
                    NetworkMessageBuilder.MessagesSent = 0;
                    NetworkMessageBuilder.MessagesReceived = 0;
                }
                Physics.Simulate(Singleton.minTimeBetweenTicks);
                _currentTick++;
            }
        }
        public void InitializeServer() {
            IsServer = true;
            Server = new Server.Server();
            Server.Start(port, maxClientCount);
            Server.ClientDisconnected += PlayerLeft;
        }
#if !UNITY_SERVER
        public void InitializeClient() {
            IsClient = true;
            Client = new Client.Client { IsServerOwner = IsServer };
            Client.Connected += DidConnect;
            Client.Disconnected += DidDisconnect;
            Client.ConnectionFailed += FailedToConnect;
            Client.Connect($"{hostAddress}:{port}");
        }
        public void StopClient() {
            if (IsClient) {
                IsClient = false;
                Client.Connected -= DidConnect;
                Client.Disconnected -= DidDisconnect;
                Client.ConnectionFailed -= FailedToConnect;
                Client.Disconnect();
            }
        }
#endif
        public void StopServer() {
            if (IsServer) {
                Server.Stop();
                IsServer = false;
                Server.ClientDisconnected -= PlayerLeft;
            }
        }
        private void DidConnect(object sender, EventArgs args) { Logger.Singleton.Log("Connected succesfully!", Logger.Type.INFO); }
        private void FailedToConnect (object sender, EventArgs args){ Logger.Singleton.Log("Error trying to connect...!", Logger.Type.INFO); }
        private void DidDisconnect(object sender, EventArgs args) { Logger.Singleton.Log("Disconnected succesfully!", Logger.Type.INFO); }
        private void MessageReceived(object sender, EventArgs args) { NetworkMessageBuilder.MessagesReceived++; }
        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e) {
            if (playersList.TryGetValue(e.Id, out Player player)) {
                Destroy(player.gameObject);
                playersList.Remove(e.Id);
                Message message = Message.Create(MessageSendMode.reliable, Network.Server.Server.PacketHandler.clientPlayerDespawn);
                message.AddUShort(e.Id);
                Server.SendToAll(message);
                Logger.Singleton.Log($"Player {e.Id} disconnected", Logger.Type.INFO);
            }
        }
        private void OnApplicationQuit() {
            StopServer();
#if !UNITY_SERVER
            StopClient();
#endif
        }
    }
}
