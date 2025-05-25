using System;
using _Project.Libraries.Marching_Cubes.Scripts;
using _Project.Libraries.QuickOutline.Scripts;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Network.Server;
using _Project.Scripts.Utils;
using RiptideNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.Components {
    public class Grabbable : MonoBehaviour, IEntity {
        public Item itemData;
        public static ushort nextId = 1;
        public bool hasGravity = true;
        private Vector3 _lastGroundPosition;
        private Rigidbody Rb { get; set; }
        private ItemStack _itemStack;
        public bool HasItems { get => _itemStack.GetCount() > 0; }
        private Collider _planetCollider;
        private Planet _planet;
        private Chunk _initialChunk;
        public ushort Id { get; private set; }
        private Outline _outline;
        private bool isNearGround { get; set; }
        private bool wasNearGround { get; set; }
        private Collider _collider;
        private RaycastHit hitInfo;
        private readonly INetworkSender _networkSender;

        public void Initialize(ushort id, Rigidbody rb, Item prefab) {
                Id = id;
                Rb = rb;
                itemData = prefab;
                _collider = GetComponentInChildren<Collider>();
                GrabbableProxy.Attach(_collider, this);
                _outline = UIHandler.AddOutlineToObject(gameObject, Color.green);
                _planet = GameManager.Singleton.defaultPlanet;
                if (_planet is not null)
                    _initialChunk = _planet.FindChunkAtPosition(transform.position);
                if (!GameManager.grabbableItems.ContainsKey(Id))
                    GameManager.grabbableItems.Add(Id, this);
                else {
                    Logger.Singleton.Log($"Error initializing grabbable {itemData.name} with ID {Id}", Logger.Type.ERROR);
                }
#if UNITY_EDITOR
            GameObject childCanvas = GUIUtils.createDebugTextWithWorldCanvas(gameObject,new Vector2(0.4f,0.4f),-0.08f);
            childCanvas.GetComponent<TextMeshProUGUI>().text = Id.ToString();
#endif
        }

        private void FixedUpdate() {
            int groundLayer = LayerMask.NameToLayer("Ground");
            float gravity = 9.8f;
            Vector3 center = Vector3.down * float.MaxValue;
            if (_planet is not null) {
                gravity = _planet.PlanetData.Gravity;
                center = _planet.PlanetData.Center;
                Chunk chunk = _planet.FindChunkAtPosition(transform.position);
                groundLayer = _planet.GroundLayer;
                if (chunk is null || !chunk.IsActive)
                    return;
                if (!chunk.Equals(_initialChunk)) {
                    HandleChunkTransfer(chunk);
                }
            }
            Vector3 upDir = transform.up;
            Vector3 centre = Rb.position;
            var bounds = _collider.bounds;
            float colliderBounds = bounds.extents.y;
            Vector3 castOrigin = centre + upDir * (colliderBounds * 2);
            wasNearGround = isNearGround;
            isNearGround = Physics.Raycast(castOrigin, -upDir, out hitInfo, colliderBounds * 4, groundLayer);
            _lastGroundPosition = hitInfo.point;
            if (hasGravity && !isNearGround) {
                HandleGravity(center, gravity);
                if (Rb.velocity.magnitude > 1f && Rb.collisionDetectionMode != CollisionDetectionMode.Continuous) {
                    Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
            } else if (!wasNearGround && isNearGround) {
                if (Rb.collisionDetectionMode != CollisionDetectionMode.Discrete)
                    Rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            } else if (Rb.velocity.magnitude > 0) {
                HandleGravity(center, gravity);
            }
        }
        public void HandleChunkTransfer(Chunk newChunk) {
            _initialChunk.RemoveEntity(this);
            newChunk.AddEntity(this);
            _initialChunk = newChunk;
        }
        public void HandleGravity(Vector3 center, float gravity) {
            Vector3 gravityUp = (Rb.position - center).normalized;
            Vector3 gravityForce = -gravityUp * gravity; // La aceleración de la gravedad debe ser negativa para que 'tire' del jugador hacia el centro del planeta
            Rb.AddForce(gravityForce, ForceMode.Acceleration); // Aplicamos la fuerza de gravedad como una aceleración
        }
        public void HandleBounce(Vector3 center) {
            Vector3 gravityUp = (Rb.position - _planet.PlanetData.Center).normalized;
            Vector3 gravityForce = gravityUp * (Rb.velocity.magnitude);
            Rb.AddForce(gravityForce, ForceMode.Impulse);
            
        }
        public void SetItemStack(ItemStack itemStack) { this._itemStack = itemStack; }
        public ItemStack GetItemStack() => _itemStack;
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
            GameManager.grabbableItems.Remove(Id);
            GrabbableProxy.Detach(_collider);
            Chunk chunk = GetPlanet()?.FindChunkAtPosition(transform.position);
            chunk?.RemoveEntity(this);
        }

        private void OnDrawGizmos() {
            Vector3 upDir = transform.up;
            Vector3 centre = Rb.position;
            float colliderBounds = _collider.bounds.extents.y;
            Vector3 castOrigin = centre + upDir * (colliderBounds * 2);
            Gizmos.DrawRay(castOrigin, -upDir * colliderBounds * 2);
        }

        public Planet GetPlanet() => _planet;
        public GameObject GetGameObject() => gameObject;
    }
}