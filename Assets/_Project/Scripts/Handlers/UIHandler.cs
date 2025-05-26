using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _Project.Libraries.QuickOutline.Scripts;
using _Project.Scripts.DiegeticUI;
using _Project.Scripts.DiegeticUI.InterfaceControllers;
using _Project.Scripts.Network;
using _Project.Scripts.Network.Client;
using _Project.Scripts.Utils;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

#if !UNITY_SERVER
namespace _Project.Scripts.Handlers {
    public class UIHandler : MonoBehaviour {
        public static UIHandler Instance;
    
        [SerializeField] private string usernameField;
        [SerializeField] private string serverIp;
        [SerializeField] private string port;
        [SerializeField] private string logLevel;
        
        public InterfaceAbstractBaseState CurrentState;
        public InterfaceAbstractBaseState LastState;
        private InterfaceStateFactory _stateFactory;
        public Transform itemGrabberTransform;

        #region DIEGETIC_PARAMS

        public ContainerRenderer currentContainer;
        public GameObject inventoryCellIndicator;
        public MeshRenderer inventoryCellIndicatorMeshRenderer;
        public MeshFilter inventoryCellIndicatorMeshFilter;
        
        #endregion
        
        public string StateToString;

        private Dictionary<String,String> _watchedVariables;
        private RuntimeEnumPopup<Logger.Type> _loggerEnumMenu;

        public void Awake() {
            Instance = this;
        }
        private void Start() {
            serverIp = NetworkManager.Singleton.hostAddress;
            port = NetworkManager.Singleton.port.ToString();
            _watchedVariables = new Dictionary<string, string>();
            inventoryCellIndicatorMeshFilter = inventoryCellIndicator.GetComponentInChildren<MeshFilter>();
            _loggerEnumMenu = new RuntimeEnumPopup<Logger.Type>(Logger.Type.INFO,
                val => Logger.Singleton.LogLevel = val
            );
        }
        private void Update() {
            CurrentState?.UpdateStates();
            StateToString = $"{CurrentState?.StateName()} + {LastState?.StateName()}";
        }

        private void OnClientReady() {
            _stateFactory = new InterfaceStateFactory(currentContainer);
            CurrentState = _stateFactory.DefaultState();
            currentContainer.AttachToInventory(ClientHandler.Singleton.Player.InventoryManager.Inventories[0]);
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
            bool isServer = networkManager.ServerHandler is { IsRunning: true };
            bool isClient = networkManager.ClientHandler is { IsConnected: true };
            string serverText = isServer ? "Stop Server" : "Start Server";
            string clientText = isClient ? "Stop Client" : "Start Client";
            Rect area = new Rect(Vector2.right * (Screen.width - 200), new Vector2(200, 500));
            GUILayout.BeginArea(area);
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
            if (isClient && NetworkManager.Singleton.ClientHandler.Player) {
                GUILayout.Label($"IsLocal: {NetworkManager.Singleton.ClientHandler.Player.IsLocal.ToString()}");
                GUILayout.Label($"ClientId: {NetworkManager.Singleton.ClientHandler.Player.Id.ToString()}");
            }
            NetworkManager.Singleton.debugServerPosition = GUILayout.Toggle(NetworkManager.Singleton.debugServerPosition,"Enable server preview");
            foreach (string watchedVariable in _watchedVariables.Values) {
                GUILayout.Label(watchedVariable);
            }
            GUILayout.EndVertical();
            _loggerEnumMenu.OnGUILayout(area);
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
            if(!networkManager.ClientHandler.IsConnected) {
                if (!ValidateConnectionValues()) return;
                networkManager.InitializeClient(usernameField);
                networkManager.ClientHandler.OnClientReady += OnClientReady;
            } else
                networkManager.StopClient();
        }
        public void ToggleServer() {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager.ServerHandler.IsRunning)
                networkManager.ServerHandler.Stop();
            else
                networkManager.InitializeServer();
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