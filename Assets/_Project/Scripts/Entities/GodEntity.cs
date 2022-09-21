using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

namespace _Project.Scripts {
    public class GodEntity : MonoBehaviour {
        public GameObject playerPrefab;
        public UIHandler uiHandler;
        public Transform spawnPoint;
        public NetworkManager NetworkManager;
        private NetworkObject _player;
        public static GodEntity Instance { get; private set; }

        private void Awake() {
            Instance = this;
            Application.targetFrameRate = -1;
            NetworkManager = GetComponent<NetworkManager>();
        }
        public void SetUpPlayer() {
            _player = NetworkManager.GetPrefab(0, false);
        }
        public UIHandler GetUIHandler() {
            return uiHandler;
        }
        public NetworkObject GetPlayer() {
            return _player;
        }
    }
}