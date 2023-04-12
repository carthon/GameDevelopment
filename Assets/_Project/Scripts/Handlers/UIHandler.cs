using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _Project.Scripts.Components;
using _Project.Scripts.DiegeticUI.InterfaceControllers;
using _Project.Scripts.Network;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Client = _Project.Scripts.Network.Client.Client;
using Outline = QuickOutline.Scripts.Outline;

#if !UNITY_SERVER
namespace _Project.Scripts.Handlers {
    public class UIHandler : MonoBehaviour {
        public static UIHandler Instance;
    
        [SerializeField] private string usernameField;
        [SerializeField] private string serverIp;
        [SerializeField] private string port;
        
        public InterfaceAbstractBaseState CurrentState;
        public InterfaceAbstractBaseState LastState;
        private InterfaceStateFactory _stateFactory;
        public GameObject slotSelectionVisualizer;
        public Transform itemGrabberTransform;
        
        public string StateToString;

        private Dictionary<String,String> _watchedVariables;

        public void Awake() {
            Instance = this;
        }
        private void Start() {
            serverIp = NetworkManager.Singleton.hostAddress;
            port = NetworkManager.Singleton.port.ToString();
            NetworkManager.Singleton.Client = new Client();
            _watchedVariables = new Dictionary<string, string>();
            _stateFactory = new InterfaceStateFactory(this);
        }
        private void Update() {
            CurrentState?.UpdateStates();
            StateToString = $"{CurrentState?.StateName()} + {LastState?.StateName()}";
        }

        private void OnClientReady() {
            CurrentState = _stateFactory.DefaultState();
        }
        private bool ValidateConnectionValues() {
            NetworkManager networkManager = NetworkManager.Singleton;
            bool valid = true;
            if (Regex.Match(serverIp, "^((25[0-5]|(2[0-4]|1[0-9]|[1-9]|)[0-9])(\\.(?!$)|$)){4}$").Success)
                networkManager.hostAddress = serverIp;
            else {
                serverIp = "Ip no válida";
                valid = false;
            }
            if (Regex.Match(serverIp, "\\d+").Success)
                networkManager.port = ushort.Parse(port);
            else {
                port = "Puerto no válido";
                valid = false;
            }
            return valid;
        }
        public void OnGUI() {
            NetworkManager networkManager = NetworkManager.Singleton;
            bool isServer = networkManager.Server != null && networkManager.IsServer;
            bool isClient = networkManager.Client != null && networkManager.IsClient;
            string serverText = isServer ? "Stop Server" : "Start Server";
            string clientText = isClient ? "Stop Client" : "Start Client";
            
            GUILayout.BeginArea(new Rect(Vector2.right * (Screen.width - 200), new Vector2(200,500)));
            if (GUILayout.Button(serverText)) {
                ToggleServer();
            }
            if (GUILayout.Button(clientText)) {
                ToggleClient();
            }
            if(!isClient) serverIp = GUILayout.TextField(serverIp);
            if(!isClient) port = GUILayout.TextField(port);
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
        public void ToggleClient() {
            NetworkManager networkManager = NetworkManager.Singleton;
            if(!networkManager.Client.IsConnected) {
                if (!ValidateConnectionValues()) return;

                networkManager.InitializeClient();
                SendConnectionMessage();
                networkManager.Client.OnClientReady += OnClientReady;
            } else
                networkManager.StopClient();
        }
        public void ToggleServer() {
            NetworkManager networkManager = NetworkManager.Singleton;
        
            if (networkManager.Server != null) networkManager.StopServer();
            else networkManager.InitializeServer();
        }
        public void SendConnectionMessage() {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager.IsClient) {
                Message message = Message.Create(MessageSendMode.reliable, (ushort) Client.PacketHandler.serverUsername);
                message.AddString(usernameField);
                networkManager.Client.Send(message);
            }
        }
        public static Outline AddOutlineToObject(GameObject gameObject, Color color = default, bool enabled = false) {
            if(!gameObject.TryGetComponent(out Outline outline))
                outline = gameObject.AddComponent<Outline>();
            outline.OutlineColor = color;
            outline.OutlineWidth = 5f;
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.enabled = enabled;
            return outline;
        }
    }
}
#endif