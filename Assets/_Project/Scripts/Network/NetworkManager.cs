using System;
using _Project.Scripts;
using _Project.Scripts.Components;
using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour {
    private static NetworkManager _singleton;
    private static int _clientTick;
    private static int _serverTick;
    public DictionaryOfStringAndItems itemsDictionary;
    public static int ServerTick
    {
        get => _serverTick;
        private set
        {
            if (value == int.MaxValue)
                value = 0;
            _serverTick = value;
        }
    }
    public static int ClientTick
    {
        get => _clientTick;
        private set
        {
            if (value == int.MaxValue)
                value = 0;
            _clientTick = value;
        }
    }
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

    public enum ClientToServerId : ushort {
        username = 1,
        input,
        itemSwap,
        itemDrop,
        itemEquip,
        updateClient
    }
    public enum ServerToClientId : ushort {
        playerSpawned = 1,
        playerMovement,
        playerDespawn,
        itemDespawn,
        itemSpawn,
        inventoryChange,
        itemEquip,
        playerData
    }

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
    public static void GrabbableToClient(ushort id = 0){
        foreach (Grabbable grabbable in GodEntity.grabbableItems.Values) {
            Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.itemSpawn);
            grabbable.AddItemSpawnData(message);
            if (id == 0)
                Singleton.Server.SendToAll(message);
            else
                Singleton.Server.Send(message, id);
        }
    }
    public static void PlayersDataToClient(ushort id = 0) {
        foreach (PlayerNetworkManager player in PlayerNetworkManager.list.Values) {
            Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerData);
            message.AddUShort(player.Id);
            player.AddRelevantData(message);
            if (id == 0)
                Singleton.Server.SendToAll(message);
            else
                Singleton.Server.Send(message, id);
        }
    }
    #endregion
    public Server Server { get; private set; }
    public Client Client { get; private set; }  
    public void Start() {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
    }
    private void FixedUpdate() {
        if (IsClient) {
            Client.Tick();
            ClientTick++;
        }
        if (IsServer) {
            Server.Tick();
            ServerTick++;
        }
        Physics.Simulate(Time.fixedDeltaTime);
    }
    public void InitializeServer() {
        IsServer = true;
        Server = new Server();
        Server.Start(port, maxClientCount);
        Server.ClientDisconnected += PlayerLeft;
    }
    public void InitializeClient() {
        IsClient = true;
        Client = new Client();
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
            if(!IsServer) SceneManager.LoadScene(0, LoadSceneMode.Single);
        }
    }
    public void StopServer() {
        if (IsServer) {
            Server.Stop();
            IsServer = false;
            Server.ClientDisconnected -= PlayerLeft;
        }
    }
    private void DidConnect(object sender, EventArgs args) {
        UIHandler.Instance.syncPlayerData.onClick.AddListener(() => {GodEntity.Singleton.PlayerInstance.SyncWorldData(true);});
    }
    private void FailedToConnect (object sender, EventArgs args){  }
    private void DidDisconnect(object sender, EventArgs args) { 
        UIHandler.Instance.syncPlayerData.onClick.RemoveAllListeners();
    }
    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e) {
        if (PlayerNetworkManager.list.TryGetValue(e.Id, out PlayerNetworkManager player)) {
            Destroy(player.gameObject);
            Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.playerDespawn);
            message.AddUShort(e.Id);
            Server.SendToAll(message);
        }
    }
    private void OnApplicationQuit() {
        StopServer();
        StopClient();
    }
}
