using System;
using System.Collections.Generic;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageDataStructures;
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
                    GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(Id, itemData.id, transform.position, transform.rotation);
                    NetworkMessage message = new NetworkMessage(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.clientItemSpawn, grabbableData);
                    message.Send(false);
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
                Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.clientItemDespawn);
                message.AddUShort(Id);
                NetworkManager.Singleton.Server.SendToAll(message);
            }
        }

        #region ClientMessages
        [MessageHandler((ushort)NetworkManager.ServerToClientId.clientItemDespawn)]
        private static void DestroyItem(Message message) {
            if(!NetworkManager.Singleton.IsServer)
                if (GodEntity.grabbableItems.TryGetValue(message.GetUShort(), out Grabbable grabbable)) {
                    Destroy(grabbable.gameObject);
                }
        }
        #endregion

        #region ServerMessages
        [MessageHandler((ushort)NetworkManager.ServerToClientId.clientItemSpawn)]
        private static void SpawnItemClient(Message message) {
            if (!NetworkManager.Singleton.IsServer) {
                GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(message);
                Debug.Log($"Trying to get value : {grabbableData.itemId}");
                if (NetworkManager.Singleton.itemsDictionary.TryGetValue(grabbableData.itemId, out Item prefabData)){
                    Grabbable grabbable;
                    if (!GodEntity.grabbableItems.TryGetValue(grabbableData.grabbableId, out grabbable)) {
                        grabbable = Instantiate(prefabData.modelPrefab, grabbableData.position,
                            grabbableData.rotation).GetComponent<Grabbable>();
                    }
                    else {
                        grabbable.transform.position = grabbableData.position;
                        grabbable.transform.rotation = grabbableData.rotation;
                    }
                    grabbable.Initialize(grabbableData.grabbableId, prefabData);
                }
            }
        }
        #endregion
    }
}