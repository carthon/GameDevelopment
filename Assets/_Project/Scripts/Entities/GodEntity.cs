using UnityEngine;

namespace _Project.Scripts {
    public class GodEntity : MonoBehaviour {
        public GameObject playerPrefab;
        public UIHandler uiHandler;
        public Transform spawnPoint;
        private GameObject _player;
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
            Application.targetFrameRate = -1;
        }
        public void SetUpPlayer() {
        }
        public UIHandler GetUIHandler() {
            return uiHandler;
        }
        public GameObject GetPlayer() {
            return _player;
        }
    }
}