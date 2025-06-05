using _Project.Scripts.Components.LocomotionComponent;
using _Project.Scripts.Constants;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.DiegeticUI;
using _Project.Scripts.Entities;
using _Project.Scripts.Factories;
using _Project.Scripts.Handlers;
using _Project.Scripts.Handlers.CameraHandler;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using RiptideNetworking;
using TMPro;
using UnityEngine;
using static _Project.Scripts.Network.PacketType;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.Components {
    public class Player : MonoBehaviour, IEntity {

        private Locomotion _locomotion;
        private AnimatorHandler _animator;
        private InventoryManager _inventoryManager;
        private EquipmentHandler _equipmentHandler;
        public ContainerRenderer containerRenderer;
        private Grabler _grabler;
        private ulong _actions;
        [SerializeField] private Transform model;
        [SerializeField] private Transform _headPivot;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _headFollow;
        private TextMeshProUGUI _usernameDisplay;
        private float grabDistance = 4f;
        private float grabRadius = 0.1f;
        [SerializeField] private PlanetData _planetData;
        [SerializeField] private Planet _planet;
        [SerializeField] private bool isSpectator;
        private Collider _collider;

        public Transform inventoryControllerTransform;
        public float inventoryDistance;
        public Planet Planet { get => _planet; set => _planet = value; }
        public PlanetData PlanetData { get => _planetData; set => _planetData = value; }
        public Planet GetPlanet() => _planet;
        public GameObject GetGameObject() => gameObject;
        public ushort Id { get; set; }
        public bool IsLocal { get; set; }
        public string Username { get; set; }
        private Quaternion HeadRotation { get => _headPivot.rotation; }
        public Transform HeadPivot { get => _headPivot; set => _headPivot = value; }
        public Transform HeadFollow { get => _headFollow; }
        public Transform Head { get => _head; set => _head = value; }
        public Transform Model { get => model; }
        public InventoryManager InventoryManager => _inventoryManager;
        public Locomotion Locomotion => _locomotion;
        public EquipmentHandler EquipmentHandler => _equipmentHandler;
        public AnimatorHandler AnimatorHandler => _animator;

        private INetworkSender _networkSender;
        private NetworkManager _networkManager;

        public bool CanRotate { get; set; }
        public bool CanMove { get; set; }
        
        public Grabbable GetNearGrabbable() =>
            _grabler.GetPickableInRange(new Ray(_headPivot.position, _headPivot.forward), grabRadius, grabDistance);
        
        public MovementMessageStruct GetMovementState(int currentTick) => new MovementMessageStruct(Id, 
            Locomotion.Rb.position, Locomotion.Rb.velocity, Locomotion.RelativeDirection, Locomotion.lookForwardDirection, Model.rotation,
            HeadRotation, currentTick, _actions);
        public void SetActions(ulong actions) => _actions = actions;
        public void SetNetworking(INetworkSender networkSender, NetworkManager networkManager) {
            _networkSender = networkSender;
            _networkManager = networkManager;
            _locomotion.Delta = networkManager.NetworkTimer.MinTimeBetweenTicks;
        }
        private void OnDestroy() {
            enabled = false;
        }
        private void Update() {
            _animator.UpdateAnimatorValues(_locomotion.RelativeDirection.z, _locomotion.RelativeDirection.x, 
                Time.deltaTime);
            _locomotion.IgnoreGround = isSpectator;
            //UIHandler.Instance.UpdateWatchedVariables("density", $"DensityAtPosition:{_planet.GetDensityAtPoint(position)}");
        }
        private void OnTriggerEnter(Collider other) {
            if (_collider is null || !_collider.Equals(other)) {
                Debug.Log("Trying to load planet");
                _collider = other;
                if (_collider.transform.TryGetComponent(out Planet planet)) {
                    _planet = planet;
                    _locomotion.GravityCenter = PlanetData.Center;
                    _locomotion.Gravity = PlanetData.Gravity;
                    _locomotion.Stats.groundLayer = _planet.GroundLayer;
                }
            }
        }
        
        public void InitializeComponents() {
            _locomotion = GetComponent<Locomotion>();
            _animator = GetComponent<AnimatorHandler>();
            _inventoryManager = GetComponent<InventoryManager>();
            _equipmentHandler = GetComponent<EquipmentHandler>();
            _inventoryManager.Owner = this;
            _grabler = GetComponent<Grabler>();
            _grabler.LinkedInventoryManager = _inventoryManager;
            _inventoryManager.Add(new Inventory("PlayerInventory", this, 3, 3,_inventoryManager));
            _inventoryManager.OnItemDropped += OnItemDropped;
            _inventoryManager.OnSlotChange += OnInventorySlotChange;
            _grabler.CanPickUp = true;
            _planet = GameManager.Singleton.defaultPlanet;
            CanRotate = true;
            CanMove = true;
            _usernameDisplay = GetComponentInChildren<TextMeshProUGUI>();
            _animator.Initialize();
            if (_planet is not null)
                _planetData = _planet.PlanetData;
            else
                _planetData = new PlanetData {
                    Center = Vector3.down * float.MaxValue,
                    Gravity = 20.9f
                };
            _locomotion.SetUp(_planetData.Center, _planetData.Gravity);
            _actions = (ulong) InputHandler.PlayerActions.None;
            InputHandler.Singleton.OnPickAction += HandlePicking;
        }
        private void OnInventorySlotChange(int inventoryId, InventorySlot inventorySlot) {
            if (!NetworkManager.Singleton.IsServer)
                return;
            _networkSender.SendToServer(MessageSendMode.reliable, (ushort) serverInventoryChange, new InventorySlotMessageStruct(Id, inventoryId, inventorySlot));
        }
        //Entra servidor y cliente
        private void OnItemDropped(ItemStack itemStack, Vector3 position, Quaternion rotation) {
            GrabbableMessageStruct grabbableMessage = new GrabbableMessageStruct(0, itemStack, position, rotation) {
                inventoryId = itemStack.GetInventory()?.Id ?? -1
            };
            if (_networkManager.IsServer) {
                Grabbable grabbable = SpawnFactory.CreateGrabbableInstance(itemStack, position, rotation);
                grabbableMessage = new GrabbableMessageStruct(grabbable);
                _networkSender.SendToClients(MessageSendMode.reliable, (ushort)clientItemSpawn, grabbableMessage);
            }
            else if(!_networkManager.IsHost) {
                _networkSender.SendToServer(MessageSendMode.reliable, (ushort)serverItemDrop, grabbableMessage);
            }
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
        public void HandleLocomotion(InputMessageStruct input) {
            Transform cameraPivot = _headPivot.transform;
            if (!CanMove) input.moveInput = Vector3.zero;
            _locomotion.HandleMovement(input, CanRotate ? cameraPivot : transform);
        }
        /**<summary>
     *  <param name="actions">Lista de bools</param>
     * <p>Se ejecuta como servidor y cliente: actualiza el estado de las animaciones del jugador</p>
     * </summary>
     */
        public void HandleAnimations(ulong actions) {
            _animator.SetBool(AnimatorHandler.IsSprinting, LocomotionUtils.IsSprinting(actions));
            _animator.SetBool(AnimatorHandler.IsCrouching, LocomotionUtils.IsCrouching(actions));
            _animator.SetBool(AnimatorHandler.IsSearching, LocomotionUtils.IsSearching(actions));
            _animator.SetBool(AnimatorHandler.IsAttacking, LocomotionUtils.IsAttacking(actions), 
                !LocomotionUtils.IsMoving(actions));
            _animator.SetBool(AnimatorHandler.IsFalling, !_locomotion.IsGrounded);
            CanMove = !LocomotionUtils.IsInInventory(actions);
            if (CanRotate) {
                RotateCharacterModel();
            }
            if (LocomotionUtils.IsAttacking(actions)) HandleClick();
        }
        public void RotateCharacterModel() {
            // Obtener la dirección hacia la cual el headPivot está mirando, pero solo en el plano horizontal.
            float rotationSpeed = !LocomotionUtils.IsMoving(_actions) && !InputHandler.Singleton.IsInInventory ? CameraHandler.Singleton.CameraData.playerLookInputLerpSpeed * Time.deltaTime : 1f;

            // Calcula el "arriba" local basado en la orientación del planeta.
            Vector3 localUp = (_locomotion.Rb.position - PlanetData.Center).normalized;

            // Obtiene la dirección a la cual la cabeza está mirando, pero transformada al plano local del personaje.
            Vector3 forwardOnPlanetSurface = _locomotion.lookForwardDirection;

            // Calcula la rotación que alinea el eje "arriba" del personaje con el "arriba" local del planeta.
            Quaternion groundAlignmentRotation = Quaternion.FromToRotation(transform.up, localUp);

            // Combina la rotación que alinea al personaje con el suelo y la rotación de la cabeza.
            Quaternion targetRotation = groundAlignmentRotation * Quaternion.LookRotation(forwardOnPlanetSurface, localUp);

            // Aplica la rotación al modelo del personaje.
            model.rotation = Quaternion.Slerp(model.rotation, targetRotation, rotationSpeed);
        }
        // Entrada por UIHandler
        private void HandlePicking() {
            if (IsLocal)
                HandlePicking(GetNearGrabbable());
        }
        //Entrada por servidor y Cliente
        public bool HandlePicking(Grabbable lookingAtGrabbable) {
            bool mustDestroy = false;
            if (Vector3.Distance(lookingAtGrabbable.transform.position, transform.position) > grabDistance)
                return false;
            GrabbableMessageStruct grabbableMessage = new GrabbableMessageStruct(lookingAtGrabbable);
            ItemStack pickedItems = lookingAtGrabbable.GetItemStack().GetCopy();
            ItemStack leftovers = _grabler.TryPickItems(lookingAtGrabbable);
            if (!leftovers.IsEmpty()) {
                Logger.Singleton.Log($"Sobran items! {leftovers}", Logger.Type.DEBUG);
            }
            else {
                Logger.Singleton.Log($"Picked all items! {pickedItems}", Logger.Type.DEBUG);
                mustDestroy = true;
                if (IsLocal)
                    _networkSender.SendToServer(MessageSendMode.reliable, (ushort) serverPickUpGrabbable, grabbableMessage);
            }
            return mustDestroy;
        }
        private void HandleClick() {
            EquipmentDisplayer equippedItem = EquipmentHandler.GetEquipmentSlotByBodyPart(BodyPart.RightArm);
            if (!equippedItem.CurrentEquipedItem.IsEmpty()) equippedItem.CurrentEquipedItem.Item.TryDoMainAction();
        }
        public void UpdatePlayerMovementState(MovementMessageStruct movementMessage,  bool isInstant = true, float speed = 1f) { 
            _locomotion.Rb.position = (isInstant) ? movementMessage.position : Vector3.Lerp(transform.position, movementMessage.position, 
                speed);
            _locomotion.lookForwardDirection = movementMessage.forwardDirection;
            _locomotion.Rb.velocity = Vector3.zero;
            Model.rotation = movementMessage.modelRotation;
            Locomotion.RelativeDirection = movementMessage.relativeDirection;
            HeadPivot.rotation = movementMessage.headPivotRotation;
            SetActions(movementMessage.actions);
            HandleAnimations(movementMessage.actions);
            _locomotion.FixedTick();
        }
        public void NotifyEquipment(ItemStack itemStack, BodyPart equipmentSlot, bool activeState, ushort ofClientId = 0) {
            EquipmentMessageStruct equipmentData = new EquipmentMessageStruct(itemStack, (int) equipmentSlot, activeState, ofClientId);
            
            if (_networkManager.IsServer) {
                _networkSender.SendToClients(MessageSendMode.reliable, (ushort) clientReceiveEquipment, equipmentData, ofClientId);
            } else if (_networkManager.IsClient) {
                _networkSender.SendToServer(MessageSendMode.reliable, (ushort) serverItemEquip, equipmentData);
            }
        }
        public override string ToString() {
            return $"World Position:{transform.position} | Id:{Id}";
        }
    }
}
