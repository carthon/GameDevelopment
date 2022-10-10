using System;
using System.Collections.Generic;
using _Project.Scripts.Network;
using RiptideNetworking;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class Grabbable : MonoBehaviour {
        public static Dictionary<ushort, Grabbable> list = new Dictionary<ushort, Grabbable>();
        public Item itemData;
        public static ushort nextId = 1;
        [SerializeField]
        private LootTable _lootTable;
        public bool HasItems => !_lootTable.IsEmpty();
        public ushort Id { get; private set; }

        public void Initialize(ushort id, Item prefab) {
                Id = id;
                itemData = prefab;
                list.Add(Id, this);
                UpdateID();
                if (NetworkManager.Singleton.IsServer)
                    SpawnItemMessage();
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
            if (NetworkManager.Singleton.IsServer) {
                Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.itemDespawn);
                message.AddUShort(Id);
                NetworkManager.Singleton.Server.SendToAll(message);
            }
                
        }
        public void SpawnItemMessage(ushort id = 0) {
            Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.itemSpawn);
            message.AddUShort(Id);
            message.AddString(itemData.id);
            message.AddVector3(transform.position);
            message.AddQuaternion(transform.rotation);
            if (id == 0)
                NetworkManager.Singleton.Server.SendToAll(message);
            else
                NetworkManager.Singleton.Server.Send(message, id);
        }

        #region ClientMessages
        [MessageHandler((ushort)NetworkManager.ServerToClientId.itemDespawn)]
        private static void DestroyItem(Message message) {
            if(!NetworkManager.Singleton.IsServer)
                if (list.TryGetValue(message.GetUShort(), out Grabbable grabbable)) {
                    Destroy(grabbable.gameObject);
                }
        }
        [MessageHandler((ushort)NetworkManager.ServerToClientId.itemSpawn)]
        private static void SpawnItemClient(Message message) {
            if (!NetworkManager.Singleton.IsServer) {
                ushort id = message.GetUShort();
                string modelId = message.GetString();
                Item prefabData = NetworkManager.Singleton.itemsDictionary[modelId];
                Grabbable grabbable = Instantiate(prefabData.modelPrefab, message.GetVector3(), 
                    message.GetQuaternion()).GetComponent<Grabbable>();
                grabbable.Initialize(id, prefabData);
            }
        }
        #endregion
    }
}