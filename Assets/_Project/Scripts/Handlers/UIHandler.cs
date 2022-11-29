using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using _Project.Scripts;
using _Project.Scripts.Network.Client;
using _Project.Scripts.UI;
using RiptideNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour {
    public static UIHandler Instance;
    public GameObject inventoryUI;
    public Transform inventorySpawner;
    public ItemPickerUI ItemPickerUI;
    [SerializeField] private List<InventoryUI> _inventories;
    public DragItemHandlerUI dragItemHandlerUI;
    public HotbarUI _hotbarUi;
    public bool ShowingInventory { get; private set; }
    
    [SerializeField] private Button _startClient;
    [SerializeField] private Text _startClientText;
    [SerializeField] private Button _startServer;
    [SerializeField] public Button syncPlayerData;
    [SerializeField] private Text _startServerText;
    [SerializeField] private InputField usernameField;
    [SerializeField] private InputField serverIp;
    [SerializeField] private InputField port;
    public bool UpdateVisuals { get; set; }

    public void Awake() {
        _inventories = new List<InventoryUI>();
        _hotbarUi = GetComponentInChildren<HotbarUI>();
        Instance = this;
    }
    private void Start() {
        serverIp.text = NetworkManager.Singleton.hostAddress;
        port.text = NetworkManager.Singleton.port.ToString();
        _startClientText = _startClient.GetComponentInChildren<Text>();
        _startServerText = _startServer.GetComponentInChildren<Text>();
    }
    private void Update() {
        NetworkManager networkManager = NetworkManager.Singleton;
        bool isServer = networkManager.Server != null && networkManager.IsServer;
        bool isClient = networkManager.Client != null && networkManager.IsClient;
        _startServerText.text = isServer ? "Stop Server" : "Start Server";
        _startClientText.text = isClient ? "Stop Client" : "Start Client";
        if (usernameField.enabled == isClient) {
            usernameField.enabled = !isClient;
            port.enabled = !isClient;
            serverIp.enabled = !isClient;
        }
    }

    public void StartStopClient() {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager.Client == null || !networkManager.Client.IsConnected) {
            if (!ValidateConnectionValues()) return;
            networkManager.InitializeClient();
            SendName();
        }
        else networkManager.StopClient();
    }
    private bool ValidateConnectionValues() {
        NetworkManager networkManager = NetworkManager.Singleton;
        bool valid = true;
        if (Regex.Match(serverIp.text, "^((25[0-5]|(2[0-4]|1[0-9]|[1-9]|)[0-9])(\\.(?!$)|$)){4}$").Success)
            networkManager.hostAddress = serverIp.text;
        else {
            serverIp.text = "Ip no válida";
            valid = false;
        }
        if (Regex.Match(serverIp.text, "\\d+").Success)
            networkManager.port = ushort.Parse(port.text);
        else {
            port.text = "Puerto no válido";
            valid = false;
        }
        return valid;
    }
    public void OnGUI() {
        NetworkManager networkManager = NetworkManager.Singleton;
        bool isServer = networkManager.Server != null || networkManager.IsServer;
        bool isClient = networkManager.Client != null || networkManager.IsClient;
        _startServerText.text = isServer ? "Stop Server" : "Start Server";
        _startClientText.text = isClient ? "Stop Client" : "Start Client";
        GUILayout.BeginArea(new Rect(Vector2.right * (Screen.width - 200), new Vector2(200,500)));
        GUILayout.BeginVertical("box");
        GUILayout.TextField($"IsClient: {NetworkManager.Singleton.IsClient.ToString()}");
        GUILayout.TextField($"IsServer: {NetworkManager.Singleton.IsServer.ToString()}");
        if (isClient && NetworkManager.Singleton.Client.Player) {
            GUILayout.TextField($"IsLocal: {NetworkManager.Singleton.Client.Player.IsLocal.ToString()}");
            GUILayout.TextField($"ClientId: {NetworkManager.Singleton.Client.Player.Id.ToString()}");
        }
        GUILayout.BeginVertical();
        GUILayout.EndArea();
    }
    public void StartStopServer() {
        NetworkManager networkManager = NetworkManager.Singleton;
        
        if (networkManager.Server != null) networkManager.StopServer();
        else networkManager.InitializeServer();
    }
    public void SendName() {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager.IsClient) {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ClientToServerId.serverUsername);
            message.AddString(usernameField.text);
            networkManager.Client.Send(message);
        }
    }

    public void Tick(float delta) {
        _hotbarUi.Tick(delta);
    }
    public void AddInventory(Inventory inventory, InventoryManager inventoryManager) {
        var index = _inventories.FindIndex(inventoryUi => inventoryUi.IsConfigured);
        InventoryUI inventoryUi = null;
        if (index == -1 || _inventories.Count < 1) {
            inventoryUi = Instantiate(inventoryUI, inventorySpawner).GetComponent<InventoryUI>();
            _inventories.Add(inventoryUi);
            index = _inventories.Count - 1;
        }
        _inventories[index].SetUpInventory(inventory);
    }
    public void TriggerInventory(int slot) {
        _inventories[slot].gameObject.SetActive(!_inventories[slot].gameObject.activeSelf);
        ShowingInventory = _inventories[slot].gameObject.activeSelf;
    }
    public void UpdateInventorySlot(int inventory, int slot) {
        _inventories[inventory].UpdateSlot(slot, null);
    }

    public void SwapSlots(int inventory1, int inventory2, int slot, int slot2) {
        UpdateInventorySlot(inventory1, slot);
        UpdateInventorySlot(inventory2, slot2);
    }
}