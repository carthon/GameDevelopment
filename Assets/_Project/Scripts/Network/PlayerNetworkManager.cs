using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _Project.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Network.Server;
using _Project.Scripts.UI;
using RiptideNetworking;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerNetworkManager : MonoBehaviour {
    private InputHandler _inputHandler;
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
        float fixedDelta = Time.fixedDeltaTime;
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
    public void SyncWorldData() {
        //Envía la información desde el sevidor al cliente con los Objetos en la escena
        if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
            NetworkManager.GrabbableToClient(Id);
        else if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
            Debug.Log("Sending Update Message");
            Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ClientToServerId.serverUpdateClient);
            NetworkManager.Singleton.Client.Send(message);
        }
    }
    private void OnSpawn() {
        InitializeComponents();
        _inputHandler = InputHandler.Singleton;
        _inputHandler.enabled = IsLocal;
        _usernameDisplay.text = Username;
        if (IsLocal) {
            Cursor.lockState = CursorLockMode.Locked;
            CameraHandler.Singleton.InitializeCamera(_head, _headPivot);
            UIHandler.Instance.AddInventory(_inventoryManager.Inventories[0], _inventoryManager);
            UIHandler.Instance.TriggerInventory(0);
            SyncWorldData();
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
    public void HandleLocomotion(Vector3 moveInput) {
        Transform cameraPivot = _headPivot.transform;
        _locomotion.TargetPosition = CalculateDirection(moveInput, cameraPivot);
        _locomotion.RelativeDirection = moveInput;
    }
    //Aplica las acciones a los componentes necesarios. Es client-side,
    //por lo que se replica en el servidor
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
        if(net.IsServer) {
            foreach (PlayerNetworkManager otherPlayer in ServerManager.playersList.Values) {
                otherPlayer.NotifySpawn(id);
            }
        }
        playerNetwork.name = $"Player {id} {(string.IsNullOrEmpty(username) ? "Guest" : username)}";
        playerNetwork.Id = id;
        playerNetwork.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
        playerNetwork.OnSpawn();

        playerNetwork.NotifySpawn();
        ServerManager.playersList.Add(id, playerNetwork);
    }
    private void NotifySpawn(ushort toClientId = 0) {
        NetworkManager net = NetworkManager.Singleton;
        if (net.IsServer) {
            SpawnMessageStruct spawnData = new SpawnMessageStruct(Id, Username, transform.position);
            NetworkMessage networkMessage = new NetworkMessage(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.clientPlayerSpawned,spawnData);
            networkMessage.Send(false, toClientId);
        }
    }
    public void NotifyEquipment(ItemStack itemStack, BodyPart equipmentSlot, bool activeState, ushort ofClientId = 0) {
        if(NetworkManager.Singleton.IsServer && IsLocal)
            return;
        EquipmentMessageStruct equipmentData = new EquipmentMessageStruct(itemStack, (int) equipmentSlot, activeState, ofClientId);
        ushort messageId = NetworkManager.Singleton.IsServer ? (ushort) NetworkManager.ServerToClientId.clientReceiveEquipment : (ushort) NetworkManager.ClientToServerId.serverItemEquip;
        NetworkMessage message = new NetworkMessage(MessageSendMode.reliable, messageId, equipmentData);
        message.Send(NetworkManager.Singleton.IsClient);
    }
    
#endregion
}
