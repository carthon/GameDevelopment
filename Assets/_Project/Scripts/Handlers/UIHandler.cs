using System;
using System.Collections.Generic;
using _Project.Scripts.UI;
using RiptideNetworking;
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

    [SerializeField] private Button _startClient;
    [SerializeField] private Button _startServer;
    [SerializeField] private InputField usernameField;
    public bool UpdateVisuals { get; set; }

    public void Awake() {
        _inventories = new List<InventoryUI>();
        _hotbarUi = GetComponentInChildren<HotbarUI>();
        Instance = this;
    }

    private void Update() {
    }

    public void StartStopClient() {
        NetworkManager networkManager = NetworkManager.Singleton;
        
        if (!networkManager.Client.IsConnected) networkManager.InitializeClient();
        else networkManager.StopClient();
    }

    public void UpdateButtonsText() {
        NetworkManager networkManager = NetworkManager.Singleton;
        _startServer.GetComponentInChildren<Text>().text = networkManager.Server.IsRunning ? "Stop Server" : "Start Server";
        _startClient.GetComponentInChildren<Text>().text = networkManager.Client.IsConnected ? "Stop Client" : "Start Client";
    }

    public void StartStopServer() {
        NetworkManager networkManager = NetworkManager.Singleton;
        
        if (networkManager.Server.IsRunning) networkManager.StopServer();
        else networkManager.InitializeServer();
        UpdateButtonsText();
    }
    public void SendName() {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager.IsClient) {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ClientToServerId.name);
            message.AddString(usernameField.text);
            networkManager.Client.Send(message);
        }
    }

    public void Tick(float delta) {
        _hotbarUi.Tick(delta);
    }
    public void AddInventory(Inventory inventory) {
        var index = _inventories.FindIndex(inventoryUi => inventoryUi.IsConfigured);
        InventoryUI inventoryUi = null;
        if (index == -1 || _inventories.Count < 1) {
            inventoryUi = Instantiate(inventoryUI, inventorySpawner).GetComponent<InventoryUI>();
            _inventories.Add(inventoryUi);
            index = _inventories.Count - 1;
        }
        _inventories[index].SetUpInventory(inventory);
    }

}