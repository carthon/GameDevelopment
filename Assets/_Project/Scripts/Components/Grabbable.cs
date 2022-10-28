using System;
using System.Collections.Generic;
using _Project.Scripts.Network;
using RiptideNetworking;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class Grabbable : MonoBehaviour {
        public Item itemData;
        public static ushort nextId = 1;
        [SerializeField]
        private LootTable _lootTable;
        public bool HasItems => !_lootTable.IsEmpty();
        public ushort Id { get; private set; }

        public void Initialize(ushort id, Item prefab) {
                Id = id;
                itemData = prefab;
                if (!GodEntity.grabbableItems.TryGetValue(Id, out Grabbable grabbable))
                    GodEntity.grabbableItems.Add(Id, this);
                //Mantener esta línea si está en desarrollo
                UpdateID();
                if (NetworkManager.Singleton.IsServer) {
                    Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.itemSpawn);
                    AddItemSpawnData(message);
                    NetworkManager.Singleton.Server.SendToAll(message);
                }
        }
        private void UpdateID() {
            GetComponentInChildren<TextMeshProUGUI>().text = Id.ToString();
        }
        public void SetLootTable(LootTable lootTable) {
            _lootTable = lootTable;
        }
        public LootTable GetLootTable() {
            return _lootTable;
        }
        public void OnDestroy() {
            GodEntity.grabbableItems.Remove(this.Id);
            if (NetworkManager.Singleton.IsServer) {
                Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.itemDespawn);
                message.AddUShort(Id);
                NetworkManager.Singleton.Server.SendToAll(message);
            }
                
        }
        public void AddItemSpawnData(Message message) {
            //Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.itemSpawn);
            message.AddUShort(Id);
            message.AddString(itemData.id);
            message.AddVector3(transform.position);
            message.AddQuaternion(transform.rotation);
        }

        #region ClientMessages
        [MessageHandler((ushort)NetworkManager.ServerToClientId.itemDespawn)]
        private static void DestroyItem(Message message) {
            if(!NetworkManager.Singleton.IsServer)
                if (GodEntity.grabbableItems.TryGetValue(message.GetUShort(), out Grabbable grabbable)) {
                    Destroy(grabbable.gameObject);
                }
        }
        #endregion

        #region ServerMessages
        [MessageHandler((ushort)NetworkManager.ServerToClientId.itemSpawn)]
        private static void SpawnItemClient(Message message) {
            if (!NetworkManager.Singleton.IsServer) {
                ushort id = message.GetUShort();
                string modelId = message.GetString();
                Item prefabData = NetworkManager.Singleton.itemsDictionary[modelId];
                Grabbable grabbable;
                if (!GodEntity.grabbableItems.TryGetValue(id, out grabbable)) {
                    grabbable = Instantiate(prefabData.modelPrefab, message.GetVector3(), 
                        message.GetQuaternion()).GetComponent<Grabbable>();
                }
                else {
                    grabbable.transform.position = message.GetVector3();
                    grabbable.transform.rotation = message.GetQuaternion();
                }
                grabbable.Initialize(id, prefabData);
                Debug.Log("Updating Grabbables");
            }
        }
        #endregion
    }
}