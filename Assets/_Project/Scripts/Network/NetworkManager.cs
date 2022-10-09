using System;
using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour {
    private static NetworkManager _singleton;
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
    }
    public enum ServerToClientId : ushort {
        playerSpawned = 1,
        playerMovement,
        playerDespawn,
    }

    void Awake() {
        _singleton = this;
    }
    public Server Server { get; private set; }
    public Client Client { get; private set; }  
    public void Start() {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
    }
    private void FixedUpdate() {
        if (IsServer)
            Server.Tick();
        if (IsClient)
            Client.Tick();
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
            if(!IsServer) SceneManager.LoadScene(0);
        }
    }
    public void StopServer() {
        if (IsServer) {
            Server.Stop();
            IsServer = false;
            Server.ClientDisconnected -= PlayerLeft;
        }
    }
    private void DidConnect(object sender, EventArgs args) { UIHandler.Instance.UpdateButtonsText(); }
    private void FailedToConnect (object sender, EventArgs args){ UIHandler.Instance.UpdateButtonsText(); }
    private void DidDisconnect(object sender, EventArgs args) { UIHandler.Instance.UpdateButtonsText(); }
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
