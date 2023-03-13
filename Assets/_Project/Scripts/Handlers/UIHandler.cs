using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using RiptideNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Client = _Project.Scripts.Network.Client.Client;

#if !UNITY_SERVER
public class UIHandler : MonoBehaviour {
    public static UIHandler Instance;
    private InventoryManager _playerInventory;
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

    public void Awake() {
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
            SendConnectionMessage();
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
    public void SendConnectionMessage() {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager.IsClient) {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) Client.PacketHandler.serverUsername);
            message.AddString(usernameField.text);
            networkManager.Client.Send(message);
        }
    }
    public void ResetMouseSelection() {
        if (_hitMouseOutline != null) {
            _hitMouseOutline.enabled = false;
            _hitMouseOutline = null;
        }
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
                if (InputHandler.Singleton.Clicked && outline.transform.Equals(_hitMouseOutline.transform)) {
                    InputHandler.Singleton.Clicked = false;
                }
            } else {
                ResetMouseSelection();
            }
        }
        else {
            ResetMouseSelection();
        }
    }
}
#endif