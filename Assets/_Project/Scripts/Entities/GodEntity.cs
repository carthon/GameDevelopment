using UnityEngine;

namespace _Project.Scripts {
    public class GodEntity : MonoBehaviour {
        public UIHandler uiHandler;
        public Transform spawnPoint;
        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;
        public GameObject PlayerPrefab { get; private set; }
        public PlayerNetworkManager PlayerInstance { get; set; }
        private static GodEntity _singleton;
        public static GodEntity Singleton
        {
            get => _singleton;
            private set {
                if (_singleton == null)
                    _singleton = value;
                else if(_singleton != null) {
                    Debug.Log($"{nameof(GodEntity)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        private void Awake() {
            Singleton = this;
            PlayerPrefab = _playerPrefab;
            Application.targetFrameRate = -1;
        }
        public UIHandler GetUIHandler() {
            return uiHandler;
        }
    }
}