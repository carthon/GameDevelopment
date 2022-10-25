using System.Collections.Generic;
using _Project.Scripts.Components;
using UnityEngine;

namespace _Project.Scripts {
    public class GodEntity : MonoBehaviour {
        public UIHandler uiHandler;
        public Transform spawnPoint;
        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;
        [Header("WorldData")]
        public static Dictionary<ushort, Grabbable> grabbableItems = new Dictionary<ushort, Grabbable>();
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
        public static bool SpawnItem(Item item, int count, Vector3 position, Quaternion rotation) {
            bool success = false;
            GameObject itemRendered = Instantiate(item.modelPrefab, position, rotation);
            var pickable = itemRendered.GetComponent<Grabbable>();
            if (pickable) {
                var lootTable = new LootTable();
                lootTable.AddToLootTable(item, count);
                pickable.SetLootTable(lootTable);
                pickable.Initialize(Grabbable.nextId, item);
                Grabbable.nextId++;
                success = true;
            }
            return success;
        }
        public static bool SpawnItem(ItemStack itemStack, Vector3 position, Quaternion rotation) => SpawnItem(itemStack.Item, itemStack.GetCount(), position, rotation);
        public static bool SpawnItem(Item item, int count, Transform transform) => SpawnItem(item, count, transform.position, transform.rotation);
    }
}