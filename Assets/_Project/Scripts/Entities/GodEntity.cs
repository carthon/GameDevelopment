using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Network;
using _Project.Scripts.Network.Client;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts {
    public class GodEntity : MonoBehaviour {
        public Transform spawnPoint;
        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;
        [Header("WorldData")]
        public static Dictionary<ushort, Grabbable> grabbableItems = new Dictionary<ushort, Grabbable>();
        public GameObject PlayerPrefab { get; private set; }
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
            if (pickable == null) pickable = itemRendered.AddComponent<Grabbable>();
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
        public static Player Spawn(ushort id, string username, Vector3 position, int currentTick) {
            NetworkManager net = NetworkManager.Singleton;
            Player playerNetwork = Instantiate(Singleton._playerPrefab, position, Quaternion.identity).GetComponent<Player>();
        
            playerNetwork.name = $"Player {id} {(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            playerNetwork.Id = id;
            playerNetwork.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
            Logger.Singleton.Log($"[{(NetworkManager.Singleton.IsClient ? "CLIENT" : "SERVER")}] Spawned player {playerNetwork.Id} at tick : {currentTick}");
            if (net.IsClient) {
                playerNetwork.IsLocal = id == net.Client.Id;
            }
            if(net.IsServer) {
                foreach (Player otherPlayer in NetworkManager.playersList.Values) {
                    Network.Server.Server.NotifySpawn(otherPlayer, currentTick, id);
                }
                Network.Server.Server.NotifySpawn(playerNetwork, currentTick);
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