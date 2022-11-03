using System;
using System.Collections.Generic;
using _Project.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;
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
        serverUsername = 1,
        serverInput,
        serverItemSwap,
        serverItemDrop,
        serverItemEquip,
        serverUpdateClient
    }
    public enum ServerToClientId : ushort {
        clientPlayerSpawned = 1,
        clientPlayerMovement,
        clientPlayerDespawn,
        clientItemDespawn,
        clientItemSpawn,
        clientInventoryChange,
        clientReceiveEquipment,
        clientReceivePlayerData
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
    public static void GrabbableToClient(ushort toClientId = 0){
        foreach (Grabbable grabbable in GodEntity.grabbableItems.Values) {
            GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(grabbable.Id, grabbable.itemData.id, grabbable.transform.position, grabbable.transform.rotation);
            NetworkMessage message = new NetworkMessage(MessageSendMode.reliable, (ushort) ServerToClientId.clientItemSpawn, grabbableData);
            message.Send(false, toClientId);
        }
    }
    public static void PlayersDataToClient(ushort id = 0) {
        if (Singleton.IsServer) {
            foreach (PlayerNetworkManager player in PlayerNetworkManager.list.Values) {
                List<EquipmentMessageStruct> equipments = new List<EquipmentMessageStruct>();
                foreach (EquipmentDisplayer equipmentDisplayer in player.EquipmentHandler.EquipmentDisplayers) {
                    equipments.Add(new EquipmentMessageStruct(equipmentDisplayer.CurrentEquipedItem, 
                        (int) equipmentDisplayer.GetBodyPart(), equipmentDisplayer.IsActive));
                }
                PlayerDataMessageStruct playerData = new PlayerDataMessageStruct(equipments, player.Id);
                NetworkMessage message = new NetworkMessage(MessageSendMode.reliable, (ushort) ServerToClientId.clientReceivePlayerData, playerData);
                message.Send(false, id);
            }
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
    private void DidConnect(object sender, EventArgs args) {  }
    private void FailedToConnect (object sender, EventArgs args){  }
    private void DidDisconnect(object sender, EventArgs args) {  }
    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e) {
        if (PlayerNetworkManager.list.TryGetValue(e.Id, out PlayerNetworkManager player)) {
            Destroy(player.gameObject);
            Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.clientPlayerDespawn);
            message.AddUShort(e.Id);
            Server.SendToAll(message);
        }
    }
    private void OnApplicationQuit() {
        StopServer();
        StopClient();
    }
}
