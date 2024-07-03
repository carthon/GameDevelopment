using System;
using _Project.Libraries.Marching_Cubes.Scripts;
using _Project.Libraries.QuickOutline.Scripts;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using RiptideNetworking;
using TMPro;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;
using Server = _Project.Scripts.Network.Server.Server;

namespace _Project.Scripts.Components {
    public class Grabbable : MonoBehaviour {
        public Item itemData;
        public static ushort nextId = 1;
        public bool hasGravity = true;
        private Rigidbody Rb { get; set; }
        [SerializeField]
        private LootTable _lootTable;
        public bool HasItems => !_lootTable.IsEmpty();
        private Collider _planetCollider;
        private Planet _planet;
        public ushort Id { get; private set; }
        private Outline _outline;
        private bool isNearGround { get; set; }
        private bool wasNearGround { get; set; }
        private Collider _collider;
        private RaycastHit hitInfo;

        public void Initialize(ushort id, Rigidbody rb, Item prefab) {
                Id = id;
                Rb = rb;
                //rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                itemData = prefab;
                _collider = GetComponentInChildren<Collider>();
                _outline = UIHandler.AddOutlineToObject(gameObject, Color.green);
                _planet = GameManager.Singleton.defaultPlanet;
                if (!GameManager.grabbableItems.ContainsKey(Id))
                    GameManager.grabbableItems.Add(Id, this);
                else {
                    Logger.Singleton.Log($"Error initializing grabbable {itemData.name} with ID {Id}", Logger.Type.ERROR);
                }
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

        private void FixedUpdate() {
            Vector3 upDir = transform.up;
            Vector3 centre = Rb.position;
            var bounds = _collider.bounds;
            float colliderBounds = bounds.extents.y;
            Vector3 castOrigin = centre + upDir * (colliderBounds * 2);
            wasNearGround = isNearGround;
            isNearGround = Physics.Raycast(castOrigin, -upDir, out hitInfo, colliderBounds * 4, _planet.GroundLayer);
            if (hasGravity && !isNearGround) {
                HandleGravity();
                if (Rb.velocity.magnitude > 1f && Rb.collisionDetectionMode != CollisionDetectionMode.Continuous) {
                    Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
            } else if (!wasNearGround && isNearGround) {
                if (Rb.collisionDetectionMode != CollisionDetectionMode.Discrete)
                    Rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            } else if (Rb.velocity.magnitude > 0) {
                HandleGravity();
            }
        }
        public void HandleGravity() {
            Vector3 gravityUp = (Rb.position - _planet.Center).normalized;
            Vector3 gravityForce = -gravityUp * _planet.Gravity; // La aceleración de la gravedad debe ser negativa para que 'tire' del jugador hacia el centro del planeta
            Rb.AddForce(gravityForce, ForceMode.Acceleration); // Aplicamos la fuerza de gravedad como una aceleración
        }
        public void HandleBounce() {
            Vector3 gravityUp = (Rb.position - _planet.Center).normalized;
            Vector3 gravityForce = gravityUp * (Rb.velocity.magnitude);
            Rb.AddForce(gravityForce, ForceMode.Impulse);
            
        }
        public void SetLootTable(LootTable lootTable) {
            _lootTable = lootTable;
        }
        public LootTable GetLootTable() {
            return _lootTable;
        }
        private void OnTriggerEnter(Collider other) {
            if (_planetCollider is null || !_planetCollider.Equals(other)) {
                _planetCollider = other;
                if (_planetCollider.transform.TryGetComponent(out Planet planet)) {
                    _planet = planet;
                }
            }
        }
        public void SetOutline(bool enabled) => _outline.enabled = enabled;
        public void OnDestroy() {
            GameManager.grabbableItems.Remove(this.Id);
            if (NetworkManager.Singleton.IsServer) {
                Message message = Message.Create(MessageSendMode.reliable, Server.PacketHandler.clientItemDespawn);
                message.AddUShort(Id);
                NetworkManager.Singleton.Server.SendToAll(message);
            }
        }

        private void OnDrawGizmos() {
            Vector3 upDir = transform.up;
            Vector3 centre = Rb.position;
            float colliderBounds = _collider.bounds.extents.y;
            Vector3 castOrigin = centre + upDir * (colliderBounds * 2);
            Gizmos.DrawRay(castOrigin, -upDir * colliderBounds * 2);
        }

        #region ClientMessages
        [MessageHandler((ushort)Server.PacketHandler.clientItemDespawn)]
        private static void DestroyItem(Message message) {
            if(!NetworkManager.Singleton.IsServer)
                if (GameManager.grabbableItems.TryGetValue(message.GetUShort(), out Grabbable grabbable)) {
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
                if (NetworkManager.Singleton.itemsDictionary.TryGetValue(grabbableData.itemId, out Item prefabData)) {
                    Rigidbody rb = null;
                    if (!GameManager.grabbableItems.TryGetValue(grabbableData.grabbableId, out Grabbable grabbable)) {
                        grabbable = Instantiate(prefabData.modelPrefab, grabbableData.position,
                            grabbableData.rotation).AddComponent<Grabbable>();
                    }
                    else {
                        Transform transform = grabbable.transform;
                        transform.position = grabbableData.position;
                        transform.rotation = grabbableData.rotation;
                    }
                    rb = grabbable.GetComponent<Rigidbody>();
                    grabbable.Initialize(grabbableData.grabbableId, rb, prefabData);
                }
            }
        }
        #endregion
    }
}