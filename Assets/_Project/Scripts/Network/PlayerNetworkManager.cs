using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _Project.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using _Project.Scripts.UI;
using RiptideNetworking;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerNetworkManager : MonoBehaviour {
    public static Dictionary<ushort, PlayerNetworkManager> list = new Dictionary<ushort, PlayerNetworkManager>();
    private InputHandler _inputHandler;
    private Locomotion _locomotion;
    private AnimatorHandler _animator;
    private CameraHandler _cameraHandler;
    private InventoryManager _inventoryManager;
    private EquipmentHandler _equipmentHandler;
    private Grabler _grabler;
    [SerializeField] private Transform model;
    private TextMeshProUGUI _usernameDisplay;

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }
    public string Username { get; private set; }
    public InventoryManager InventoryManager => _inventoryManager;
    public EquipmentHandler EquipmentHandler => _equipmentHandler;

    [SerializeField] private float grabDistance = 5f;
    
    public void InitializeComponents() {
        _inputHandler = GetComponent<InputHandler>();
        _locomotion = GetComponent<Locomotion>();
        _animator = GetComponent<AnimatorHandler>();
        _cameraHandler = GetComponent<CameraHandler>();
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
    private void SyncWorldData() {
        //TODO: Cambiarlo a que cuando el servidor detecte que se ha conectado un jugador, que le envíe los items a su alrededor
        if (NetworkManager.Singleton.IsServer && !IsLocal)
            foreach (Grabbable grabbable in GodEntity.grabbableItems.Values) {
                grabbable.SpawnItemMessage(Id);
            }
    }
    private void OnSpawn() {
        InitializeComponents();
        _inputHandler.enabled = IsLocal;
        _cameraHandler.enabled = IsLocal;
        _usernameDisplay.text = Username;
        SyncWorldData();
        if (IsLocal) {
            Cursor.lockState = CursorLockMode.Locked;
            _cameraHandler.InitializeCamera();
            UIHandler.Instance.AddInventory(_inventoryManager.Inventories[0], _inventoryManager);
            UIHandler.Instance.TriggerInventory(0);
        }
    }
    private void Update() {
        float delta = Time.deltaTime;
        if (IsLocal) {
            HandleUI();
            _cameraHandler.Tick(delta);
            SendInput();
            _inputHandler?.ClearInputs();
        }
    }
    private void FixedUpdate() {
        float fixedDelta = Time.fixedDeltaTime;
        HandleRotation();
        _animator.UpdateAnimatorValues(_locomotion.RelativeDirection.z, _locomotion.RelativeDirection.x, _locomotion.IsSprinting);
        if (IsLocal) {
            Vector3 mouseInput = (_cameraHandler.GetDirectionFromMouse(_inputHandler.MouseX, _inputHandler.MouseY));
            if (!_inputHandler.IsUIEnabled)
                _cameraHandler.FixedTick();
        }
        if (NetworkManager.Singleton.IsServer) {
            _locomotion.FixedTick(fixedDelta);
            SendMovement();
        }
    }
    private void HandleRotation() {
        Quaternion newRotation = Quaternion.Euler(0.0f, _cameraHandler.CameraPivot.rotation.eulerAngles.y, 0.0f);
        model.rotation = Quaternion.Lerp(model.rotation, newRotation, _cameraHandler.CameraData.playerLookInputLerpSpeed * Time.fixedDeltaTime);
        //transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, _cameraHandler.CameraData.rotationMultiplier * Time.fixedDeltaTime);
        //transform.rotation = newRotation;
    }

    private void OnDestroy() {
        enabled = false;
        list.Remove(Id);
    }
    private static void Spawn(ushort id, string username, Vector3 position) {
        NetworkManager net = NetworkManager.Singleton;
        PlayerNetworkManager playerNetwork = Instantiate(GodEntity.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<PlayerNetworkManager>();
        
        if (net.IsClient) {
            playerNetwork.IsLocal = id == net.Client.Id;
            if(playerNetwork.IsLocal)
                GodEntity.Singleton.PlayerInstance = playerNetwork;
        }
        if(net.IsServer) {
            foreach (PlayerNetworkManager otherPlayer in list.Values) {
                otherPlayer.NotifySpawn(id);
            }
        }
        playerNetwork.name = $"Player {id} {(string.IsNullOrEmpty(username) ? "Guest" : username)}";
        playerNetwork.Id = id;
        playerNetwork.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
        playerNetwork.OnSpawn();

        playerNetwork.NotifySpawn();
        list.Add(id, playerNetwork);
    }
    private void UpdateEquipment(ItemStack itemStack, BodyPart equipmentSlot, bool activeState){
            if (activeState) {
                _equipmentHandler.LoadItemModel(itemStack, equipmentSlot);
            }
            else
                _equipmentHandler.UnloadItemModel(equipmentSlot);
    }
    private void SetPositionAndRotation(Vector3 position, Quaternion rotation) {
        transform.position = position;
        if(!IsLocal)
            _locomotion.Rb.rotation = rotation;
    }
    private void HandleLocomotion(Vector3 moveInput) {
        Transform cameraPivot = _cameraHandler.CameraPivot.transform;
        _locomotion.TargetPosition = CalculateDirection(moveInput, cameraPivot);
        _locomotion.RelativeDirection = moveInput;
    }
    //Aplica las acciones a los componentes necesarios. Es client-side,
    //por lo que se replica en el servidor
    private void HandleAnimations(bool[] actions) {
        _locomotion.IsMoving = actions[0];
        _locomotion.IsJumping = actions[1];
        _locomotion.IsSprinting = actions[2];
        _animator.SetBool("isPicking", actions[3]);
        _animator.SetBool("isFalling", !_locomotion.IsGrounded);
    }
    private void HandlePicking() {
        if(NetworkManager.Singleton.IsServer) {
            Ray ray = new Ray(_cameraHandler.CameraPivot.position, _cameraHandler.CameraPivot.forward);
            Grabbable grabbable = _grabler.GetPickableInRange(ray, grabDistance);
            if(!(grabbable is null)) {
                LootTable leftovers = _grabler.TryPickItems(grabbable);
                if (!leftovers.IsEmpty()) {
                    Debug.Log("Sobran items!");
                }
            }
        }
    }
    private void HandleUI(){
        if (_inputHandler.IsUIEnabled) {
            Cursor.lockState = CursorLockMode.None;
            if (!UIHandler.Instance.ShowingInventory) UIHandler.Instance.TriggerInventory(0);
        }
        else if (Cursor.lockState == CursorLockMode.None && !_inputHandler.IsUIEnabled) {
            Cursor.lockState = CursorLockMode.Locked;
            if (UIHandler.Instance.ShowingInventory) UIHandler.Instance.TriggerInventory(0);
        }
        
        if (_inputHandler.HotbarSlot != UIHandler.Instance._hotbarUi.ActiveSlot) {
            HotbarUI hotbar = UIHandler.Instance._hotbarUi;
            hotbar.ActiveSlot = _inputHandler.HotbarSlot;
            ItemLinks linkedItemLinkInSlot = hotbar.GetItemLinkInSlot(hotbar.ActiveSlot);
            if (linkedItemLinkInSlot != null && linkedItemLinkInSlot.LinkedStacks.Count > 0 && linkedItemLinkInSlot.LinkedStacks[0].GetCount() > 0) {
                ItemStack itemStack = linkedItemLinkInSlot.LinkedStacks[0];
                _equipmentHandler.LoadItemModel(itemStack, BodyPart.RightArm);
                NotifyEquipment(itemStack, BodyPart.RightArm, true);
            }
            else {
                _equipmentHandler.UnloadItemModel(BodyPart.RightArm);
                NotifyEquipment(ItemStack.EMPTY, BodyPart.RightArm, false);
            }
        }
    }
    private Vector3 CalculateDirection(Vector3 moveInput, Transform someTransform) {
        var calculateDirection = moveInput.z * someTransform.forward +
            moveInput.x * someTransform.right;
        calculateDirection.y = 0;
        return calculateDirection.normalized;
    }
    private Vector3 GetLookDirection() {
        return _cameraHandler.CameraPivot.rotation.eulerAngles;
    }
    #region Messages
    #region Client Messages
    [MessageHandler((ushort)NetworkManager.ServerToClientId.playerSpawned)]
    private static void SpawnPlayerClient(Message message) {
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
            Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
        }
    }
    [MessageHandler((ushort)NetworkManager.ServerToClientId.playerDespawn)]
    private static void DeSpawnPlayer(Message message) {
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
            if (list.TryGetValue(message.GetUShort(), out PlayerNetworkManager player))
                Destroy(player.gameObject);
        }
    }
    [MessageHandler((ushort) NetworkManager.ServerToClientId.playerMovement)]
    private static void ReceiveMovement(Message message) {
        if (list.TryGetValue(message.GetUShort(), out PlayerNetworkManager player)) {
            Vector3 position = message.GetVector3();
            Vector3 relativeDirection = message.GetVector3();
            Quaternion playerRotation = message.GetQuaternion();
            Quaternion cameraRotation = message.GetQuaternion();
            bool[] actions = message.GetBools();
            player.SetPositionAndRotation(position, playerRotation);
            player._locomotion.IsGrounded = actions[3];
            if (!player.IsLocal) {
                player._locomotion.RelativeDirection = relativeDirection;
                player.HandleAnimations(actions);
                player._cameraHandler.CameraPivot.rotation = cameraRotation;
            }
        }
    }
    [MessageHandler((ushort) NetworkManager.ServerToClientId.itemEquip)]
    private static void ReceiveEquipment(Message message) {
        if (NetworkManager.Singleton.IsClient) {
            if (list.TryGetValue(message.GetUShort(), out PlayerNetworkManager player)) {
                ItemStack itemStack = message.GetItemStack();
                BodyPart equipmentSlot = (BodyPart) message.GetInt();
                bool activeStatus = message.GetBool();
                if (!player.IsLocal) {
                    Debug.Log($"Equipping: {itemStack.Item?.name} : {activeStatus} for Player: {player.Username}");
                    player.UpdateEquipment(itemStack, equipmentSlot, activeStatus);
                }
            }
        }
    }
    private void SendInput() {
        Vector3 moveInput = new Vector3(_inputHandler.Horizontal, 0, _inputHandler.Vertical);
        bool[] actions = new[] {
            _inputHandler.IsMoving,
            _inputHandler.IsJumping,
            _inputHandler.IsSprinting,
            _inputHandler.IsPicking
        };
        if (actions[3]) HandlePicking();
        Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ClientToServerId.input);
        message.AddVector3(moveInput);
        message.AddBools(actions);
        HandleLocomotion(moveInput);
        HandleAnimations(actions);
        message.AddQuaternion(_cameraHandler.CameraPivot.rotation);
        NetworkManager.Singleton.Client.Send(message);
    }
    #endregion

    #region Server Messages
    private void SendMovement() {
        bool[] actions = new[] {
            _locomotion.IsMoving,
            _locomotion.IsJumping,
            _locomotion.IsSprinting,
            _locomotion.IsGrounded
        };
        Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.playerMovement);
        message.AddUShort(Id);
        message.AddVector3(transform.position);
        message.AddVector3(_locomotion.RelativeDirection);
        message.AddQuaternion(transform.rotation);
        message.AddQuaternion(_cameraHandler.CameraPivot.rotation);
        message.AddBools(actions);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
    [MessageHandler((ushort)NetworkManager.ClientToServerId.itemSwap)]
    private static void SlotSwapServer(ushort fromClientId, Message message) {
        if (!NetworkManager.Singleton.IsServer) return;
        int[] data = message.GetInts();
        int inventoryId = data[0];
        int otherInventoryId = data[1];
        int slot = data[2];
        int otherSlot = data[3];
        if (list.TryGetValue(fromClientId, out PlayerNetworkManager player)) {
            Inventory otherInventory = player._inventoryManager.Inventories[otherInventoryId];
            player._inventoryManager.Inventories[inventoryId].SwapItemsInInventory(otherInventory, slot, otherSlot);
        }
    }
    [MessageHandler((ushort)NetworkManager.ClientToServerId.input)]
    private static void ReceiveInput(ushort fromClientId, Message message) {
        if (list.TryGetValue(fromClientId, out PlayerNetworkManager player)) {
            //Si es host, entonces no recive el input (ya ha sido procesado por el servidor)
            if (player.IsLocal && !NetworkManager.Singleton.IsServer) return;
            
            Vector3 moveInput = message.GetVector3();
            bool[] actions = message.GetBools();
            Quaternion cameraPivot = message.GetQuaternion();
            player._cameraHandler.CameraPivot.rotation = cameraPivot;
            player.HandleLocomotion(moveInput);
            player.HandleAnimations(actions);
            if (actions[3])
                player.HandlePicking();
        }
    }
    [MessageHandler((ushort) NetworkManager.ClientToServerId.username)]
    private static void SpawnPlayerServer(ushort fromClientId, Message message) {
        if (NetworkManager.Singleton.IsServer) {
            Spawn(fromClientId, message.GetString(), GodEntity.Singleton.spawnPoint.position + 
                Vector3.right * Random.value * 4);
        }
    }
    [MessageHandler((ushort) NetworkManager.ClientToServerId.itemEquip)]
    private static void SpawnItemOnPlayer(ushort clientId, Message message) {
        if (NetworkManager.Singleton.IsServer) {
            ItemStack itemStack = message.GetItemStack();
            BodyPart equipmentSlot = (BodyPart) message.GetInt();
            bool activeState = message.GetBool();
            if (list.TryGetValue(clientId, out PlayerNetworkManager player)) {
                //Actualizo el equipamiento en el servidor
                Debug.Log($"Equipping: {itemStack.Item?.name} : {activeState} for Player: {player.Username}");
                player.UpdateEquipment(itemStack, equipmentSlot, activeState);
                //Notifico al resto de jugadores
                player.NotifyEquipment(itemStack, equipmentSlot, activeState, clientId);
            }
        }
    }
    private void NotifyEquipment(ItemStack itemStack, BodyPart equipmentSlot, bool activeState, ushort ofClientId = 0) {
        NetworkManager net = NetworkManager.Singleton;
        Message message;
        if (net.IsServer) {
            message = Message.Create(
                MessageSendMode.reliable, (ushort)NetworkManager.ServerToClientId.itemEquip);
            message.AddUShort(ofClientId);
        } else {
            message = Message.Create(
                MessageSendMode.reliable, (ushort)NetworkManager.ClientToServerId.itemEquip);
        }
        message.AddItemStack(itemStack);
        message.AddInt((int) equipmentSlot);
        message.AddBool(activeState);
        if (net.IsServer)
            net.Server.SendToAll(message);
        if (net.IsClient)
            net.Client.Send(message);
    }
    private void NotifySpawn(ushort toClientId = 0) {
        NetworkManager net = NetworkManager.Singleton;
        if (net.IsServer) {
            Message message = AddSpawnData(Message.Create(
                MessageSendMode.reliable, (ushort)NetworkManager.ServerToClientId.playerSpawned));
            if (toClientId == 0)
                net.Server.SendToAll(message);
            else
                net.Server.Send(message, toClientId);
        }
    }
    private Message AddSpawnData(Message message) {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }
    #endregion
#endregion
}
