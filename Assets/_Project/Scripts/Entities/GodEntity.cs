using UnityEngine;

namespace _Project.Scripts {
    public class GodEntity : MonoBehaviour {
        public GameObject playerPrefab;
        public UIHandler uiHandler;
        public Transform spawnPoint;
        private GameObject _player;
        public static GodEntity Instance { get; private set; }

        private void Awake() {
            Instance = this;
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