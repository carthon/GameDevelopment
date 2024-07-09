using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Network;
using _Project.Scripts.Network.Client;
using _Project.Scripts.Network.Server;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.Handlers {
    [ExecuteInEditMode]
    [RequireComponent(typeof(ChunkRenderer))]
    public class GameManager : MonoBehaviour {
        public Transform spawnPoint;
        [SerializeField] public Planet defaultPlanet;
        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;
        [Header("WorldData")]
        public static Dictionary<ushort, Grabbable> grabbableItems = new Dictionary<ushort, Grabbable>();
        public ChunkRenderer ChunkRenderer;
        public GameConfiguration gameConfiguration;
        public GameObject PlayerPrefab { get; private set; }
        private static GameManager _singleton;
        public static GameManager Singleton
        {
            get => _singleton;
            private set {
                if (_singleton == null)
                    _singleton = value;
                else if(_singleton != null) {
                    Debug.Log($"{nameof(GameManager)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }
        private void Awake() {
            PlayerPrefab = _playerPrefab;
            Application.targetFrameRate = -1;
            Initialize();
        }
        public void Initialize() {
            Singleton ??= this;
            ChunkRenderer = GetComponent<ChunkRenderer>();
            ChunkRenderer ??= gameObject.AddComponent<ChunkRenderer>();
        }
        public static bool SpawnItem(Item item, int count, Vector3 position, Quaternion rotation, Entity spawner) {
            bool success = false;
            GameObject itemRendered = Instantiate(item.modelPrefab, position, rotation);
            itemRendered.layer = LayerMask.NameToLayer("Item");
            var pickable = itemRendered.GetComponent<Grabbable>();
            var rb = itemRendered.GetComponent<Rigidbody>();
            if (rb is null) rb = itemRendered.AddComponent<Rigidbody>();
            if (pickable is null) pickable = itemRendered.AddComponent<Grabbable>();
            if (pickable && rb) {
                var lootTable = new LootTable();
                lootTable.AddToLootTable(item, count);
                pickable.SetLootTable(lootTable);
                pickable.Initialize(Grabbable.nextId, rb, item);
                Grabbable.nextId++;
                success = true;
            }
            return success;
        }
        public static bool SpawnItem(ItemStack itemStack, Vector3 position, Quaternion rotation, Entity spawner) => SpawnItem(itemStack.Item, itemStack.GetCount(), position, rotation, spawner);
        public static bool SpawnItem(Item item, int count, Transform transform, Entity spawner) => SpawnItem(item, count, transform.position, transform.rotation, spawner);
        public static Player Spawn(ushort id, string username, Vector3 position, int currentTick) {
            NetworkManager net = NetworkManager.Singleton;
            Player playerNetwork = Instantiate(Singleton._playerPrefab, position, Quaternion.identity).GetComponent<Player>();
        
            playerNetwork.name = $"Player {id} {(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            playerNetwork.Id = id;
            playerNetwork.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
            Logger.Singleton.Log($"Spawned player {playerNetwork.Id} at tick : {currentTick}", Logger.Type.DEBUG);
            if (net.IsClient) {
                playerNetwork.IsLocal = id == net.Client.Id;
            }
            if(net.IsServer) {
                foreach (Player otherPlayer in NetworkManager.playersList.Values) {
                    Server.NotifySpawn(otherPlayer, currentTick, id);
                }
                Server.NotifySpawn(playerNetwork, currentTick);
            }
            playerNetwork.OnSpawn();
            if(playerNetwork.IsLocal) {
                Client.Singleton.SetUpClient(playerNetwork);
            }
            NetworkManager.playersList.Add(id, playerNetwork);
            return playerNetwork;
        }
    }
}