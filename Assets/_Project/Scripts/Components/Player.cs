using System;
using _Project.Scripts.Components.LocomotionComponent;
using _Project.Scripts.Constants;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.DiegeticUI;
using _Project.Scripts.Entities;
using _Project.Scripts.Factories;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using _Project.Scripts.Network.Client;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Network.MessageUtils;
using _Project.Scripts.Network.Server;
using RiptideNetworking;
using TMPro;
using UnityEngine;
using static _Project.Scripts.Network.PacketType;

namespace _Project.Scripts.Components {
    public class Player : MonoBehaviour, IEntity {

        private Locomotion _locomotion;
        private AnimatorHandler _animator;
        private InventoryManager _inventoryManager;
        private EquipmentHandler _equipmentHandler;
        public ContainerRenderer containerRenderer;
        private Grabler _grabler;
        private bool[] _actions;
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

        public bool CanRotate { get; set; }
        public bool CanMove { get; set; }
        
        public Grabbable GetNearGrabbable() =>
            _grabler.GetPickableInRange(new Ray(_headPivot.position, _headPivot.forward), grabRadius, grabDistance);
        
        public MovementMessageStruct GetMovementState(int currentTick) => new MovementMessageStruct(Id, 
            Locomotion.Rb.position, Locomotion.Rb.velocity, Locomotion.RelativeDirection, Model.rotation,
            HeadRotation, currentTick, _actions);
        public void SetActions(bool[] actions) => _actions = actions;
        public void SetNetworking(INetworkSender networkSender) { _networkSender = networkSender; }
        private void OnDestroy() {
            enabled = false;
        }
        private void Update() {
            _animator.UpdateAnimatorValues(_locomotion.RelativeDirection.z, _locomotion.RelativeDirection.x);
            Vector3 position = transform.position;
            //UIHandler.Instance.UpdateWatchedVariables("density", $"DensityAtPosition:{_planet.GetDensityAtPoint(position)}");
            if (_planet is not null) {
                UIHandler.Instance.UpdateWatchedVariables("continentalness", $"Continentalness:{_planet.GetHeightMapValuesAtPoint(position)}");
                float planetSize = _planet.NumChunks * 100;
                UIHandler.Instance.UpdateWatchedVariables("planetheight", $"Planet height {position.magnitude / planetSize}");
            }
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
            InputHandler.Singleton.OnPickAction += HandlePicking;
            _actions = new bool[typeof(ActionsEnum).GetFields().Length];
        }
        private void OnInventorySlotChange(int inventoryId, InventorySlot inventorySlot) {
            if (!NetworkManager.Singleton.IsServer)
                return;
            _networkSender.Send(MessageSendMode.reliable, (ushort) serverInventoryChange, new InventorySlotMessageStruct(Id, inventoryId, inventorySlot));
        }
        private void OnItemDropped(ItemStack itemStack, Vector3 position, Quaternion rotation) {
            Grabbable grabbable = SpawnFactory.CreateGrabbableInstance(itemStack, position, rotation);
            _networkSender.Send(MessageSendMode.reliable, (ushort) clientItemSpawn, new GrabbableMessageStruct(grabbable));
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
            _animator.SetBool(AnimatorHandler.IsSearching, actions[(int)ActionsEnum.SEARCHING]);
            _animator.SetBool(AnimatorHandler.IsAttacking, actions[(int)ActionsEnum.ATTACKING], actions[(int)ActionsEnum.ATTACKING]);
            _animator.SetBool(AnimatorHandler.IsFalling, !_locomotion.IsGrounded);
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
        private void HandleActions(bool[] actions) {
            if (actions[(int) ActionsEnum.ATTACKING] && !actions[(int) ActionsEnum.SEARCHING]) HandleClick();
        }
        private void HandlePicking() {
            Grabbable lookingAtGrabbable = GetNearGrabbable();
            if (!(lookingAtGrabbable is null)) {
                ItemStack leftovers = _grabler.TryPickItems(lookingAtGrabbable);
                if (!leftovers.IsEmpty()) {
                    Debug.Log("Sobran items!");
                }
                else if (NetworkManager.Singleton.IsServer) {
                    _networkSender.Send(MessageSendMode.reliable, (ushort) clientItemDespawn,new ItemDespawnMessageStruct(Id));
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
            if (IsLocal)
                return;
            
            model.transform.rotation = movementMessage.rotation;
            Locomotion.RelativeDirection = movementMessage.relativeDirection;
            SetActions(movementMessage.actions);
            HandleAnimations(movementMessage.actions);
            HeadPivot.rotation = movementMessage.headPivotRotation;
            Locomotion.FixedTick();
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
            ushort messageId = NetworkManager.Singleton.IsServer ? (ushort) clientReceiveEquipment 
                : (ushort) serverItemEquip;
            _networkSender.Send(MessageSendMode.reliable, messageId, equipmentData);
        }
        public override string ToString() {
            return $"World Position:{transform.position} | Id:{Id}";
        }
    }
}
