using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts;
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
    public GameObject debugTexts;
    public List<TextMeshProUGUI> debugVars;
    public HotbarUI _hotbarUi;

    [SerializeField] private Button _startClient;
    [SerializeField] private Button _startServer;
    [SerializeField] private InputField usernameField;
    [SerializeField] private InputField serverIp;
    public bool UpdateVisuals { get; set; }

    public void Awake() {
        _inventories = new List<InventoryUI>();
        _hotbarUi = GetComponentInChildren<HotbarUI>();
        Instance = this;
        serverIp.text = NetworkManager.Singleton.hostAddress;
    }
    private void OnValidate() {
        debugVars = debugTexts.GetComponentsInChildren<TextMeshProUGUI>().ToList();
        for (int i = debugVars.Count - 1; i >= 0; i--) {
            if (i % 2 == 0)
                debugVars.RemoveAt(i);
        }
    }
    private void Update() {
    }

    public void StartStopClient() {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager.Client == null || !networkManager.Client.IsConnected) {
            networkManager.hostAddress = serverIp.text;
            networkManager.InitializeClient();
            SendName();
        }
        else networkManager.StopClient();
    }

    public void UpdateButtonsText() {
        NetworkManager networkManager = NetworkManager.Singleton;
        _startServer.GetComponentInChildren<Text>().text = networkManager.Server != null ? "Stop Server" : "Start Server";
        _startClient.GetComponentInChildren<Text>().text = networkManager.Client != null ? "Stop Client" : "Start Client";
        debugVars[0].text = NetworkManager.Singleton.IsClient.ToString();
        debugVars[1].text = NetworkManager.Singleton.IsServer.ToString();
        debugVars[2].text = GodEntity.Singleton.PlayerInstance != null ? "False" : "True";
        if (GodEntity.Singleton.PlayerInstance != null) debugVars[3].text = GodEntity.Singleton.PlayerInstance.Id.ToString();
    }

    public void StartStopServer() {
        NetworkManager networkManager = NetworkManager.Singleton;
        
        if (networkManager.Server != null) networkManager.StopServer();
        else networkManager.InitializeServer();
        UpdateButtonsText();
    }
    public void SendName() {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager.IsClient) {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) NetworkManager.ClientToServerId.username);
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