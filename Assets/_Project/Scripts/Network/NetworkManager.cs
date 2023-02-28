using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Network.Client;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Network.MessageUtils;
using _Project.Scripts.Network.Server;
using _Project.Scripts.Utils;
using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.Network {
    public class NetworkManager : MonoBehaviour {
        private static NetworkManager _singleton;
        public DictionaryOfStringAndItems itemsDictionary;
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

        #region Static Functions
        public static void ReceivePlayersData(ushort id = 0) {
            if (Singleton.IsServer) {
                foreach (Player player in playersList.Values) {
                    PlayerDataMessageStruct playerData = PlayerDataMessage.getPlayerData(player);
                    NetworkMessageBuilder messageBuilder = new NetworkMessageBuilder(MessageSendMode.reliable, (ushort) Network.Server.Server.PacketHandler.clientReceivePlayerData, playerData);
                    messageBuilder.Send(id);
                }
            }
        }
        #endregion
        public Server.Server Server { get; private set; }
        public Client.Client Client { get; private set; }  
        public void Start() {
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
            Logger.Initialize();
            minTimeBetweenTicks = Time.fixedDeltaTime;
        }
        private void Update() {
            _timer += Time.deltaTime;
            while(_timer >= minTimeBetweenTicks) {
                _timer -= minTimeBetweenTicks;
                if (IsServer) {
                    Server.Tick(_currentTick);
                }
                if (IsClient) {
                    Client.Tick(_currentTick);
                }
                UIHandler.Instance.UpdateWatchedVariables("PacketsSent", $"Packets Sent per tick: {NetworkMessageBuilder.MessagesSent}");
                NetworkMessageBuilder.MessagesSent = 0;
                UIHandler.Instance.UpdateWatchedVariables("PacketsReceived", $"Packets Received per tick: {NetworkMessageBuilder.MessagesReceived}");
                NetworkMessageBuilder.MessagesReceived = 0;
                Physics.Simulate(Singleton.minTimeBetweenTicks);
                _currentTick++;
            }
        }
        public void InitializeServer() {
            IsServer = true;
            Server = new Server.Server();
            Server.Start(port, maxClientCount);
            Server.MessageReceived += MessageReceived;
            Server.ClientDisconnected += PlayerLeft;
        }
        public void InitializeClient() {
            IsClient = true;
            Client = new Client.Client { IsServerOwner = IsServer };
            Client.Connected += DidConnect;
            Client.Disconnected += DidDisconnect;
            Client.ConnectionFailed += FailedToConnect;
            //if (Client.IsServerOwner) Client.MessageReceived += MessageReceived;
            Client.Connect($"{hostAddress}:{port}");
        }
        public void StopClient() {
            if (IsClient) {
                IsClient = false;
                Client.Connected -= DidConnect;
                Client.Disconnected -= DidDisconnect;
                Client.ConnectionFailed -= FailedToConnect;
                Client.Disconnect();
                //if(!IsServer) SceneManager.LoadScene(0, LoadSceneMode.Single);
            }
        }
        public void StopServer() {
            if (IsServer) {
                Server.Stop();
                IsServer = false;
                Server.ClientDisconnected -= PlayerLeft;
                Server.MessageReceived -= MessageReceived;
            }
        }
        private void DidConnect(object sender, EventArgs args) {  }
        private void FailedToConnect (object sender, EventArgs args){  }
        private void DidDisconnect(object sender, EventArgs args) {  }
        private void MessageReceived(object sender, EventArgs args) { NetworkMessageBuilder.MessagesReceived++; }
        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e) {
            if (playersList.TryGetValue(e.Id, out Player player)) {
                Destroy(player.gameObject);
                playersList.Remove(e.Id);
                Message message = Message.Create(MessageSendMode.reliable, Network.Server.Server.PacketHandler.clientPlayerDespawn);
                message.AddUShort(e.Id);
                Server.SendToAll(message);
            }
        }
        private void OnApplicationQuit() {
            StopServer();
            StopClient();
        }
        public void SendMovementWithDelay(MovementMessageStruct movementMessageStruct, float delay) {
            StartCoroutine(SendMessage(movementMessageStruct, delay));
        }
        private IEnumerator SendMessage(MovementMessageStruct movementMessageStruct, float delay) {
            yield return new WaitForSeconds(delay);
            NetworkMessageBuilder networkMessageBuilder = new NetworkMessageBuilder(MessageSendMode.reliable, (ushort) Network.Server.Server.PacketHandler.movementMessage, movementMessageStruct);
            networkMessageBuilder.Send();
        }
        public static Dictionary<ushort, Player> playersList = new Dictionary<ushort, Player>();
    }
}
