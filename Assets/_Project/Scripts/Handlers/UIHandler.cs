using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.UI;
using RiptideNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Client = _Project.Scripts.Network.Client.Client;

public class UIHandler : MonoBehaviour {
    public static UIHandler Instance;
    public GameObject inventoryUI;
    public Transform inventorySpawner;
    [SerializeField] private List<InventoryUI> _inventories;
    public DragItemHandlerUI dragItemHandlerUI;
    public HotbarUI _hotbarUi;
    public float cameraDepth;
    
    [SerializeField] private Button _startClient;
    [SerializeField] private Text _startClientText;
    [SerializeField] private Button _startServer;
    [SerializeField] private Text _startServerText;
    [SerializeField] private InputField usernameField;
    [SerializeField] private InputField serverIp;
    [SerializeField] private InputField port;
    private Grabbable _outlinedGrabbable;
    private Outline _hitMouseOutline;

    private Dictionary<String,String> _watchedVariables;
    
    public Transform consoleContentTransform;
    public Queue<TextMeshProUGUI> consoleMessages = new Queue<TextMeshProUGUI>();
    public bool UpdateVisuals { get; set; }

    public void Awake() {
        _inventories = new List<InventoryUI>();
        _hotbarUi = GetComponentInChildren<HotbarUI>();
        _watchedVariables = new Dictionary<string, string>();
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
        GUILayout.Label($"IsClient: {NetworkManager.Singleton.IsClient.ToString()}");
        GUILayout.Label($"IsServer: {NetworkManager.Singleton.IsServer.ToString()}");
        if (isClient && NetworkManager.Singleton.Client.Player) {
            GUILayout.Label($"IsLocal: {NetworkManager.Singleton.Client.Player.IsLocal.ToString()}");
            GUILayout.Label($"ClientId: {NetworkManager.Singleton.Client.Player.Id.ToString()}");
        }
        GUILayout.Label($"CurrentTick: {NetworkManager.Singleton.Tick.ToString()}");
        NetworkManager.Singleton.debugServerPosition = GUILayout.Toggle(NetworkManager.Singleton.debugServerPosition,"Enable server preview");
        foreach (string watchedVariable in _watchedVariables.Values) {
            GUILayout.Label(watchedVariable);
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    public void UpdateWatchedVariables(string key, string value) {
        if (_watchedVariables.ContainsKey(key))
            _watchedVariables[key] = value;
        else {
            _watchedVariables.Add(key, value);
        }
    }
    public void StartStopServer() {
        NetworkManager networkManager = NetworkManager.Singleton;
        
        if (networkManager.Server != null) networkManager.StopServer();
        else networkManager.InitializeServer();
    }
    public void SendName() {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager.IsClient) {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) Client.PacketHandler.serverUsername);
            message.AddString(usernameField.text);
            networkManager.Client.Send(message);
        }
    }

    public void Tick(float delta) {
        _hotbarUi.Tick(delta);
    }
    public void HandleMouseSelection() {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Camera cam = CameraHandler.Singleton.MainCamera;
        UpdateWatchedVariables("Mouse", $"{mousePos.ToString()}");
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            if (hit.transform.TryGetComponent(out Outline outline)) {
                if (_hitMouseOutline != null && !outline.transform.Equals(_hitMouseOutline.transform)) {
                    _hitMouseOutline.enabled = false;
                }
                _hitMouseOutline = outline;
                _hitMouseOutline.enabled = true;
                if (outline.transform.Equals(_hitMouseOutline.transform)) {
                    UIHandler.Instance.TriggerInventory(0, InputHandler.Singleton.Clicked);
                }
            } else {
                if (_hitMouseOutline != null) {
                    _hitMouseOutline.enabled = false;
                    _hitMouseOutline = null;
                }
            }
        }
        else {
            if (_hitMouseOutline != null) {
                _hitMouseOutline.enabled = false;
                _hitMouseOutline = null;
            }
        }
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
    }
    public void TriggerInventory(int slot, bool state) {
        _inventories[slot].gameObject.SetActive(state);
    }
    public void UpdateInventorySlot(int inventory, int slot) {
        _inventories[inventory].UpdateSlot(slot, null);
    }

    public void SwapSlots(int inventory1, int inventory2, int slot, int slot2) {
        UpdateInventorySlot(inventory1, slot);
        UpdateInventorySlot(inventory2, slot2);
    }
}