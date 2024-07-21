using System;
using System.Collections.Concurrent;
using _Project.Scripts.Components.LocomotionComponent;
using _Project.Scripts.Constants;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageDataStructures;
using RiptideNetworking;
using TMPro;
using UnityEditor;
using UnityEngine;
using BodyPart = _Project.Scripts.DataClasses.BodyPart;
using Client = _Project.Scripts.Network.Client.Client;
using Logger = _Project.Scripts.Utils.Logger;
using Server = _Project.Scripts.Network.Server.Server;

namespace _Project.Scripts.Components {
    public class Player : MonoBehaviour, IEntity {

        private Locomotion _locomotion;
        private AnimatorHandler _animator;
        private InventoryManager _inventoryManager;
        private EquipmentHandler _equipmentHandler;
        private Grabler _grabler;
        private bool[] _actions;
        [SerializeField] private Transform model;
        [SerializeField] private Transform _headPivot;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _headFollow;
        private TextMeshProUGUI _usernameDisplay;
        private float grabDistance = 4f;
        private float grabRadius = 0.1f;
        [SerializeField] private Planet _planet;
        [SerializeField] private bool isSpectator;
        private Collider _collider;

        public Transform inventorySpawnTransform;
        public Planet Planet { get => _planet; set => _planet = value; }
        public Planet GetPlanet() => _planet;
        public GameObject GetGameObject() => gameObject;
        public ushort Id { get; set; }
        public bool IsLocal { get; set; }
        public string Username { get; set; }
        private Quaternion HeadRotation { get => _headPivot.rotation; }
        public Transform HeadPivot { get => _headPivot; set => _headPivot = value; }
        public Transform HeadFollow { get => _headFollow; }
        public Transform Head { get => _head; set => _head = value; }
        public InventoryManager InventoryManager => _inventoryManager;
        public Locomotion Locomotion => _locomotion;
        public EquipmentHandler EquipmentHandler => _equipmentHandler;
        public AnimatorHandler AnimatorHandler => _animator;

        public bool CanRotate { get; set; }
        public bool CanMove { get; set; }
        
        public Grabbable GetNearGrabbable() =>
            _grabler.GetPickableInRange(new Ray(_headPivot.position, _headPivot.forward), grabRadius, grabDistance);
        
        public MovementMessageStruct GetMovementState(int currentTick) => new MovementMessageStruct(Id, 
            Locomotion.Rb.position, Locomotion.Rb.velocity, Locomotion.RelativeDirection, Locomotion.Rb.rotation,
            HeadRotation, currentTick, _actions);
        public void SetActions(bool[] actions) => _actions = actions;

    
        private void OnDestroy() {
            enabled = false;
        }
        private void Update() {
            _animator.UpdateAnimatorValues(_locomotion.RelativeDirection.z, _locomotion.RelativeDirection.x);
            Vector3 position = transform.position;
            float planetSize = _planet.NumChunks * 100;
            //UIHandler.Instance.UpdateWatchedVariables("density", $"DensityAtPosition:{_planet.GetDensityAtPoint(position)}");
            UIHandler.Instance.UpdateWatchedVariables("continentalness", $"Continentalness:{_planet.GetContinentalnessAtPoint(position)}");
            UIHandler.Instance.UpdateWatchedVariables("planetheight", $"Planet height {position.magnitude / planetSize}");
            UIHandler.Instance.UpdateWatchedVariables("2DPosition", $"2DPosition {SphericalToEquirectangular(position)}");
            _locomotion.IgnoreGround = isSpectator;
        }
        Vector2 SphericalToEquirectangular(Vector3 position)
        {
            // Normalizar la posición para obtener coordenadas unitarias en la esfera
            Vector3 normalizedPos = position.normalized;

            // Calcular la longitud (lambda) y la latitud (phi)
            double lambda = Math.Atan2(normalizedPos.z, normalizedPos.x); // Longitud
            double phi = Math.Asin(normalizedPos.y); // Latitud

            // Convertir los ángulos a coordenadas 2D (u, v)
            double u = (lambda + Math.PI) / (2 * Math.PI); // Normalizar longitud a [0, 1]
            double v = (phi + (Math.PI / 2)) / Math.PI; // Normalizar latitud a [0, 1]

            return new Vector2((float) u, (float) v);
        }
        private void OnTriggerEnter(Collider other) {
            if (_collider is null || !_collider.Equals(other)) {
                _collider = other;
                if (_collider.transform.TryGetComponent(out Planet planet)) {
                    _planet = planet;
                    _locomotion.GravityCenter = _planet.Center;
                    _locomotion.Gravity = _planet.Gravity;
                    _locomotion.Stats.groundLayer = _planet.GroundLayer;
                }
            }
        }
        
        public void InitializeComponents() {
            _locomotion = GetComponent<Locomotion>();
            _animator = GetComponent<AnimatorHandler>();
            _inventoryManager = GetComponent<InventoryManager>();
            _equipmentHandler = GetComponent<EquipmentHandler>();
            _inventoryManager.Player = this;
            _grabler = GetComponent<Grabler>();
            _grabler.LinkedInventoryManager = _inventoryManager;
            _inventoryManager.Add(new Inventory("PlayerInventory", this,9));
            _grabler.CanPickUp = true;
            _planet = GameManager.Singleton.defaultPlanet;
            CanRotate = true;
            CanMove = true;
            _usernameDisplay = GetComponentInChildren<TextMeshProUGUI>();
            _animator.Initialize();
            _locomotion.SetUp(_planet.Center, _planet.Gravity);
            _actions = new bool[typeof(ActionsEnum).GetFields().Length];
        }
        public void OnSpawn() {
            InitializeComponents();
            _usernameDisplay.text = Username;
        }
        public void UpdateEquipment(ItemStack itemStack, BodyPart equipmentSlot, bool activeState){
            if (activeState) {
                _equipmentHandler.LoadItemModel(itemStack, equipmentSlot);
            }
            else
                _equipmentHandler.UnloadItemModel(equipmentSlot);
        }
        /**<summary>
     *  <param name="moveInput">Vector3 con la dirección de movimiento</param>
     * <p>Se ejecuta como servidor y cliente: calcula la dirección del jugador en base a la posición de la cabeza</p>
     * </summary>
     */
        public void HandleLocomotion( float delta, Vector3 moveInput) {
            Transform cameraPivot = _headPivot.transform;
            if (!CanMove) moveInput = Vector3.zero;
            _locomotion.HandleMovement(delta, moveInput, CanRotate ? cameraPivot : transform);
        }
        /**<summary>
     *  <param name="actions">Lista de bools</param>
     * <p>Se ejecuta como servidor y cliente: actualiza el estado de las animaciones del jugador</p>
     * </summary>
     */
        public void HandleAnimations(bool[] actions) {
            _locomotion.IsMoving = actions[(int)ActionsEnum.MOVING];
            _locomotion.IsJumping = actions[(int)ActionsEnum.JUMPING];
            _locomotion.IsSprinting = actions[(int)ActionsEnum.SPRINTING];
            _locomotion.IsCrouching = actions[(int)ActionsEnum.CROUCHING];
            _locomotion.IsDoubleJumping = actions[(int)ActionsEnum.DOUBLEJUMPING];
            _animator.SetBool(AnimatorHandler.IsSprinting, actions[(int)ActionsEnum.SPRINTING]);
            _animator.SetBool(AnimatorHandler.IsCrouching, actions[(int)ActionsEnum.CROUCHING]);
            _animator.SetBool(AnimatorHandler.IsPicking, actions[(int)ActionsEnum.PICKING]);
            _animator.SetBool(AnimatorHandler.IsSearching, actions[(int)ActionsEnum.SEARCHING]);
            _animator.SetBool(AnimatorHandler.IsAttacking, actions[(int)ActionsEnum.ATTACKING], actions[(int)ActionsEnum.ATTACKING]);
            _animator.SetBool(AnimatorHandler.IsFalling, !_locomotion.IsGrounded);
            CanRotate = !actions[(int)ActionsEnum.SEARCHING];
            CanMove = !actions[(int)ActionsEnum.SEARCHING];
            if (CanRotate) {
                RotateCharacterModel();
            }
            HandleActions(actions);
        }
        public void RotateCharacterModel() {
            // Obtener la dirección hacia la cual el headPivot está mirando, pero solo en el plano horizontal.
            float rotationSpeed = !_locomotion.IsMoving && !InputHandler.Singleton.IsInInventory ? CameraHandler.Singleton.CameraData.playerLookInputLerpSpeed * Time.deltaTime : 1f;

            // Calcula el "arriba" local basado en la orientación del planeta.
            Vector3 localUp = (_locomotion.Rb.position - Planet.Center).normalized;

            // Obtiene la dirección a la cual la cabeza está mirando, pero transformada al plano local del personaje.
            Vector3 forwardOnPlanetSurface = _locomotion.lookForwardDirection;

            // Calcula la rotación que alinea el eje "arriba" del personaje con el "arriba" local del planeta.
            Quaternion groundAlignmentRotation = Quaternion.FromToRotation(transform.up, localUp);

            // Combina la rotación que alinea al personaje con el suelo y la rotación de la cabeza.
            Quaternion targetRotation = groundAlignmentRotation * Quaternion.LookRotation(forwardOnPlanetSurface, localUp);

            // Aplica la rotación al modelo del personaje.
            model.rotation = Quaternion.Slerp(model.rotation, targetRotation, rotationSpeed);
        }
        private void HandleActions(bool[] actions) {
            if (actions[(int) ActionsEnum.PICKING]) HandlePicking();
            if (actions[(int) ActionsEnum.ATTACKING] && !actions[(int) ActionsEnum.SEARCHING]) HandleClick();
        }
        private void HandlePicking() {
            Grabbable lookingAtGrabbable = GetNearGrabbable();
            if (!(lookingAtGrabbable is null)) {
                LootTable leftovers = _grabler.TryPickItems(lookingAtGrabbable);
                if (!leftovers.IsEmpty()) {
                    Debug.Log("Sobran items!");
                }
            }
        }
        private void HandleClick() {
            EquipmentDisplayer equippedItem = EquipmentHandler.GetEquipmentSlotByBodyPart(BodyPart.RightArm);
            if (!equippedItem.CurrentEquipedItem.IsEmpty()) equippedItem.CurrentEquipedItem.Item.TryDoMainAction();
        }
        public void UpdatePlayerMovementState(MovementMessageStruct movementMessage,  bool isInstant = true, float speed = 1f) {
            _locomotion.Rb.position = (isInstant) ? movementMessage.position : Vector3.Lerp(transform.position, movementMessage.position, speed);
            _locomotion.Rb.velocity = movementMessage.velocity;
            if(!IsLocal) {
                _locomotion.Rb.rotation = movementMessage.rotation;
                Locomotion.RelativeDirection = movementMessage.relativeDirection;
                HandleAnimations(movementMessage.actions);
                HeadPivot.rotation = movementMessage.headPivotRotation;
            }
        }
        /**<summary>
     *  <param name="itemStack">Item equipado</param>
     *  <param name="equipmentSlot">Slot en el que se equipa</param>
     *  <param name="activeState">Estado de actividad</param>
     * <p>Si se ejecuta como servidor: notifica a todos los clientes del equipamiento del enviado</p>
     * <p>Si se ejecuta como cliente: notifica al servidor del cambio de equipamiento para que lo replique al resto de clientes
     * si en este caso también es servidor, entonces simplemente replica los cambios</p>
     * </summary>
     */
        public void NotifyEquipment(ItemStack itemStack, BodyPart equipmentSlot, bool activeState, ushort ofClientId = 0) {
            EquipmentMessageStruct equipmentData = new EquipmentMessageStruct(itemStack, (int) equipmentSlot, activeState, ofClientId);
            ushort messageId = NetworkManager.Singleton.IsServer ? (ushort) Server.PacketHandler.clientReceiveEquipment 
                : (ushort) Client.PacketHandler.serverItemEquip;
            NetworkMessageBuilder messageBuilder = new NetworkMessageBuilder(MessageSendMode.reliable, messageId, equipmentData);
            messageBuilder.Send(asServer:NetworkManager.Singleton.IsServer);
        }
        public override string ToString() {
            return $"World Position:{transform.position} | Id:{Id}";
        }
    }
}
