using System.Collections.Concurrent;
using _Project.Scripts.Constants;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageDataStructures;
using RiptideNetworking;
using TMPro;
using UnityEngine;
using Client = _Project.Scripts.Network.Client.Client;
using Server = _Project.Scripts.Network.Server.Server;

namespace _Project.Scripts.Components {
    public class Player : MonoBehaviour {

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
        public Transform inventorySpawnTransform;
        private TextMeshProUGUI _usernameDisplay;

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
            _grabler.GetPickableInRange(new Ray(_headPivot.position, _headPivot.forward), grabDistance);
        public Grabbable LastGrabbable { get; set; }
        
        public MovementMessageStruct GetMovementState(int currentTick) => new MovementMessageStruct(Id, 
            Locomotion.Rb.position, Locomotion.Rb.velocity, Locomotion.RelativeDirection, Locomotion.Rb.rotation,
            HeadRotation, currentTick, _actions);
        public void SetActions(bool[] actions) => _actions = actions;

        [SerializeField] private float grabDistance = 5f;
    
        private void OnDestroy() {
            enabled = false;
        }
        private void Update() {
            _animator.UpdateAnimatorValues(_locomotion.RelativeDirection.z, _locomotion.RelativeDirection.x);
        }
    
        public void InitializeComponents() {
            _locomotion = GetComponent<Locomotion>();
            _animator = GetComponent<AnimatorHandler>();
            _inventoryManager = GetComponent<InventoryManager>();
            _equipmentHandler = GetComponent<EquipmentHandler>();
            _equipmentHandler.Player = this;
            _inventoryManager.Player = this;
            _grabler = GetComponent<Grabler>();
            _grabler.LinkedInventoryManager = _inventoryManager;
            _inventoryManager.Add(new Inventory("PlayerInventory", 9));
            _grabler.CanPickUp = true;
            CanRotate = true;
            CanMove = true;
            _usernameDisplay = GetComponentInChildren<TextMeshProUGUI>();
            _animator.Initialize();
            _locomotion.SetUp();
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
            if(CanMove) _locomotion.HandleMovement(delta, moveInput, CanRotate ? cameraPivot : transform);
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
            _animator.SetBool(AnimatorHandler.IsSprinting, actions[(int)ActionsEnum.SPRINTING]);
            _animator.SetBool(AnimatorHandler.IsCrouching, actions[(int)ActionsEnum.CROUCHING]);
            _animator.SetBool(AnimatorHandler.IsPicking, actions[(int)ActionsEnum.PICKING]);
            _animator.SetBool(AnimatorHandler.IsSearching, actions[(int)ActionsEnum.SEARCHING]);
            _animator.SetBool(AnimatorHandler.IsFalling, !_locomotion.IsGrounded);
            CanRotate = !actions[(int)ActionsEnum.SEARCHING];
            CanMove = !actions[(int)ActionsEnum.SEARCHING];
            if (CanRotate) {
                Quaternion newRotation = Quaternion.Euler(0.0f, _headPivot.rotation.eulerAngles.y, 0.0f);
                float rotationSpeed = !_locomotion.IsMoving ? CameraHandler.Singleton.CameraData.playerLookInputLerpSpeed * Time.deltaTime : 1f;
                var modelRotation = model.rotation;
                //float rotationAngle = Quaternion.Angle(modelRotation, newRotation);
                modelRotation = Quaternion.Lerp(modelRotation, newRotation, rotationSpeed);
                model.rotation = modelRotation;
                // if (!_locomotion.IsMoving) {
                //     _animator.SetBool("isRotatingLeft", rotationAngle > 0);
                //     _animator.SetFloat("rotateSpeed", rotationAngle);
                // }
            }
            HandleActions(actions);
        }
        public void HandleActions(bool[] actions) {
            if (actions[(int) ActionsEnum.PICKING]) HandlePicking();
        }
        public void HandlePicking() {
            Grabbable lookingAtGrabbable = GetNearGrabbable();
            if (!(lookingAtGrabbable is null)) {
                LootTable leftovers = _grabler.TryPickItems(lookingAtGrabbable);
                if (!leftovers.IsEmpty()) {
                    Debug.Log("Sobran items!");
                }
            }
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
