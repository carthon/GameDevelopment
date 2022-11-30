using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.MessageDataStructures;
using RiptideNetworking;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Network {
    public class PlayerNetworkManager : MonoBehaviour {
        public static Dictionary<ushort, PlayerNetworkManager> playersList = new Dictionary<ushort, PlayerNetworkManager>();
    
        private Locomotion _locomotion;
        private AnimatorHandler _animator;
        private CameraHandler _cameraHandler;
        private InventoryManager _inventoryManager;
        private EquipmentHandler _equipmentHandler;
        private Grabler _grabler;
        [SerializeField] private Transform model;
        [SerializeField] private Transform _headPivot;
        [SerializeField] private Transform _head;
        private TextMeshProUGUI _usernameDisplay;

        public ushort Id { get; private set; }
        public bool IsLocal { get; private set; }
        public string Username { get; private set; }
        public Quaternion HeadRotation { get => _headPivot.rotation; }
        public Transform HeadPivot { get => _headPivot; set => _headPivot = value; }
        public Transform Head { get => _head; set => _head = value; }
        public InventoryManager InventoryManager => _inventoryManager;
        public Locomotion Locomotion => _locomotion;
        public EquipmentHandler EquipmentHandler => _equipmentHandler;
        public AnimatorHandler AnimatorHandler => _animator;

        [SerializeField] private float grabDistance = 5f;
    
        private void OnDestroy() {
            enabled = false;
        }
        private void Update() {
            float delta = Time.deltaTime;
        }
        private void FixedUpdate() {
            float fixedDelta = Time.deltaTime;
            HandleRotation();
            _animator.UpdateAnimatorValues(_locomotion.RelativeDirection.z, _locomotion.RelativeDirection.x, _locomotion.IsSprinting);
            _locomotion.FixedTick(fixedDelta);
        }
    
        public void InitializeComponents() {
            _locomotion = GetComponent<Locomotion>();
            _animator = GetComponent<AnimatorHandler>();
            _cameraHandler = CameraHandler.Singleton;
            _inventoryManager = GetComponent<InventoryManager>();
            _inventoryManager = GetComponent<InventoryManager>();
            _equipmentHandler = GetComponent<EquipmentHandler>();
            _equipmentHandler.Player = this;
            _inventoryManager.Player = this;
            _grabler = GetComponent<Grabler>();
            _grabler.LinkedInventoryManager = _inventoryManager;
            _inventoryManager.Add(new Inventory("PlayerInventory", 9));
            _grabler.CanPickUp = true;
            _usernameDisplay = GetComponentInChildren<TextMeshProUGUI>();
            _animator.Initialize();
            _locomotion.SetUp();
        }
        /**<summary>
     * <p>Se ejecuta como servidor: envía los items que hay en el mundo al cliente</p>
     * <p>Se ejecuta como cliente: envia una petición de actualización de los grabbables que le rodean</p>
     * </summary>
     */
        public void UpdateGrabbables() {
            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
                NetworkManager.GrabbableToClient(Id);
            else if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
                Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ClientToServerId.serverUpdateClient);
                NetworkManager.Singleton.Client.Send(message);
            }
        }
        private void OnSpawn() {
            InitializeComponents();
            _usernameDisplay.text = Username;
            if (IsLocal) {
                Cursor.lockState = CursorLockMode.Locked;
                CameraHandler.Singleton.InitializeCamera(_head, _headPivot);
                UIHandler.Instance.AddInventory(_inventoryManager.Inventories[0], _inventoryManager);
                UIHandler.Instance.TriggerInventory(0);
                UpdateGrabbables();
            }
        }
        public void HandleRotation() {
            Quaternion newRotation = Quaternion.Euler(0.0f, _headPivot.rotation.eulerAngles.y, 0.0f);
            model.rotation = Quaternion.Lerp(model.rotation, newRotation, _cameraHandler.CameraData.playerLookInputLerpSpeed * Time.fixedDeltaTime);
            //transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, _cameraHandler.CameraData.rotationMultiplier * Time.fixedDeltaTime);
            //transform.rotation = newRotation;
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
        public void HandleLocomotion(Vector3 moveInput) {
            Transform cameraPivot = _headPivot.transform;
            _locomotion.TargetPosition = CalculateDirection(moveInput, cameraPivot);
            _locomotion.RelativeDirection = moveInput;
        }
        /**<summary>
     *  <param name="actions">Lista de bools</param>
     * <p>Se ejecuta como servidor y cliente: actualiza el estado de las animaciones del jugador</p>
     * </summary>
     */
        public void HandleAnimations(bool[] actions) {
            _locomotion.IsMoving = actions[0];
            _locomotion.IsJumping = actions[1];
            _locomotion.IsSprinting = actions[2];
            _animator.SetBool("isPicking", actions[3]);
            _animator.SetBool("isFalling", !_locomotion.IsGrounded);
        }
        public void HandlePicking() {
            Ray ray = new Ray(_headPivot.position, _headPivot.forward);
            Grabbable grabbable = _grabler.GetPickableInRange(ray, grabDistance);
            if(!(grabbable is null)) {
                LootTable leftovers = _grabler.TryPickItems(grabbable);
                if (!leftovers.IsEmpty()) {
                    Debug.Log("Sobran items!");
                }
            }
        }
        private Vector3 CalculateDirection(Vector3 moveInput, Transform someTransform) {
            var calculateDirection = moveInput.z * someTransform.forward +
                moveInput.x * someTransform.right;
            calculateDirection.y = 0;
            return calculateDirection.normalized;
        }
        public void SetPositionAndRotation(Vector3 position, Vector3 rbVelocity,Quaternion rotation) {
            transform.position = position;
            _locomotion.Rb.velocity = rbVelocity;
            if(!IsLocal)
                _locomotion.Rb.rotation = rotation;
        }
        #region Messages
        public static void Spawn(ushort id, string username, Vector3 position) {
            NetworkManager net = NetworkManager.Singleton;
            PlayerNetworkManager playerNetwork = Instantiate(GodEntity.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<PlayerNetworkManager>();
        
            if (net.IsClient) {
                playerNetwork.IsLocal = id == net.Client.Id;
                if(playerNetwork.IsLocal)
                    NetworkManager.Singleton.Client.SetPlayer(playerNetwork);
            }
            //Notifica al nuevo jugador del resto de jugadores
            if(net.IsServer) {
                foreach (PlayerNetworkManager otherPlayer in playersList.Values) {
                    otherPlayer.NotifySpawn(id);
                }
            }
            playerNetwork.name = $"Player {id} {(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            playerNetwork.Id = id;
            playerNetwork.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
            playerNetwork.OnSpawn();
            //Notifica a todos los jugadores del nuevo jugador
            playerNetwork.NotifySpawn();
            playersList.Add(id, playerNetwork);
        }
        /**<summary>
     *  <param name="toClientId">[Optional] cliente a notificar del spawn</param>
     * <p>Se ejecuta como servidor: notifica a uno o todos los clientes de un spawn de jugador nuevo</p>
     * </summary>
     */
        private void NotifySpawn(ushort toClientId = 0) {
            NetworkManager net = NetworkManager.Singleton;
            if (net.IsServer) {
                SpawnMessageStruct spawnData = new SpawnMessageStruct(Id, Username, transform.position);
                NetworkMessage networkMessage = new NetworkMessage(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.clientPlayerSpawned,spawnData);
                networkMessage.Send(false, toClientId);
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
            ushort messageId = NetworkManager.Singleton.IsServer ? (ushort) NetworkManager.ServerToClientId.clientReceiveEquipment : (ushort) NetworkManager.ClientToServerId.serverItemEquip;
            NetworkMessage message = new NetworkMessage(MessageSendMode.reliable, messageId, equipmentData);
            if (NetworkManager.Singleton.IsServer && IsLocal)
                message.Send(false);
            else
                message.Send(NetworkManager.Singleton.IsClient);
        }
    
        #endregion
    }
}
