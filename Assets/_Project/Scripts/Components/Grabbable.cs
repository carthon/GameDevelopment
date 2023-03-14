using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using RiptideNetworking;
using TMPro;
using UnityEngine;
using Server = _Project.Scripts.Network.Server.Server;

namespace _Project.Scripts.Components {
    [RequireComponent(typeof(Outline))]
    public class Grabbable : MonoBehaviour {
        public Item itemData;
        public static ushort nextId = 1;
        [SerializeField]
        private LootTable _lootTable;
        public bool HasItems => !_lootTable.IsEmpty();
        public ushort Id { get; private set; }
        private Outline _outline;

        public void Initialize(ushort id, Item prefab) {
                Id = id;
                itemData = prefab;
                _outline = GetComponent<Outline>();
                _outline.OutlineColor = Color.green;
                _outline.OutlineWidth = 5f;
                _outline.OutlineMode = Outline.Mode.OutlineVisible;
                _outline.enabled = false;
                if (!GodEntity.grabbableItems.TryGetValue(Id, out Grabbable grabbable))
                    GodEntity.grabbableItems.Add(Id, this);
#if UNITY_EDITOR
            GameObject childCanvas = GUIUtils.createDebugTextWithWorldCanvas(gameObject,new Vector2(0.4f,0.4f),-0.08f);
            childCanvas.GetComponent<TextMeshProUGUI>().text = Id.ToString();
#endif
                if (NetworkManager.Singleton.IsServer) {
                    GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(Id, itemData.id, transform.position, transform.rotation);
                    NetworkMessageBuilder messageBuilder = new NetworkMessageBuilder(MessageSendMode.reliable, (ushort) Server.PacketHandler.clientItemSpawn, grabbableData);
                    messageBuilder.Send(asServer:true);
                }
        }
        public void SetLootTable(LootTable lootTable) {
            _lootTable = lootTable;
        }
        public LootTable GetLootTable() {
            return _lootTable;
        }
        public void SetOutline(bool enabled) => _outline.enabled = enabled;
        public void OnDestroy() {
            GodEntity.grabbableItems.Remove(this.Id);
            if (NetworkManager.Singleton.IsServer) {
                Message message = Message.Create(MessageSendMode.reliable, Server.PacketHandler.clientItemDespawn);
                message.AddUShort(Id);
                NetworkManager.Singleton.Server.SendToAll(message);
            }
        }

        #region ClientMessages
        [MessageHandler((ushort)Server.PacketHandler.clientItemDespawn)]
        private static void DestroyItem(Message message) {
            if(!NetworkManager.Singleton.IsServer)
                if (GodEntity.grabbableItems.TryGetValue(message.GetUShort(), out Grabbable grabbable)) {
                    Destroy(grabbable.gameObject);
                }
        }
        #endregion

        #region ServerMessages
        [MessageHandler((ushort)Server.PacketHandler.clientItemSpawn)]
        private static void SpawnItemClient(Message message) {
            if (!NetworkManager.Singleton.IsServer) {
                GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(message);
                Debug.Log($"Trying to get value : {grabbableData.itemId}");
                if (NetworkManager.Singleton.itemsDictionary.TryGetValue(grabbableData.itemId, out Item prefabData)){
                    if (!GodEntity.grabbableItems.TryGetValue(grabbableData.grabbableId, out Grabbable grabbable)) {
                        grabbable = Instantiate(prefabData.modelPrefab, grabbableData.position,
                            grabbableData.rotation).AddComponent<Grabbable>();
                    }
                    else {
                        Transform transform = grabbable.transform;
                        transform.position = grabbableData.position;
                        transform.rotation = grabbableData.rotation;
                    }
                    grabbable.Initialize(grabbableData.grabbableId, prefabData);
                }
            }
        }
        #endregion
    }
}