using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _Project.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using RiptideNetworking;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerNetworkManager : MonoBehaviour {
    public static Dictionary<ushort, PlayerNetworkManager> list = new Dictionary<ushort, PlayerNetworkManager>();
    private InputHandler _inputHandler;
    private Locomotion _locomotion;
    private CameraHandler _cameraHandler;
    private Inventory _inventory;
    
    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }
    public string Username { get; private set; }

    public void InitializeComponents() {
        _inputHandler = GetComponent<InputHandler>();
        _locomotion = GetComponent<Locomotion>();
        _locomotion.SetUp();
        _cameraHandler = GetComponent<CameraHandler>();
    }
    private void OnSpawn() {
        InitializeComponents();
        _inputHandler.enabled = IsLocal;
        _cameraHandler.enabled = IsLocal;
        if (NetworkManager.Singleton.IsClient && IsLocal) {
            Cursor.lockState = CursorLockMode.Locked;
            _cameraHandler.InitializeCamera();
            _inventory = new Inventory("PlayerInventory", 9);
        }
    }
    private void FixedUpdate() {
        float delta = Time.deltaTime;
        if (IsLocal) {
            Vector3 mouseInput = (_cameraHandler.GetDirectionFromMouse(_inputHandler.MouseX, _inputHandler.MouseY));
            HandleUI();
            _cameraHandler.FixedTick();
            _cameraHandler.Tick(delta);
            HandleRotation(mouseInput);
            SendInput();
            _inputHandler?.ClearInputs();
        }
        if (NetworkManager.Singleton.IsServer) {
            _locomotion.Tick();
            Physics.Simulate(Time.fixedDeltaTime);
            SendMovement();
        }
    }
    private void HandleRotation(Vector3 mouseInput) {
        var rb = _locomotion.Rb;
        rb.rotation = Quaternion.Euler(0.0f, _cameraHandler.CameraPivot.rotation.y, 0.0f);
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
    private void SetPositionAndRotation(Vector3 position, Quaternion rotation) {
        transform.position = position;
        if(!IsLocal)
            _locomotion.Rb.rotation = rotation;
    }
    private void HandleLocomotion(Vector2 moveInput, bool[] actions) {
        Transform playerTransform = transform;
        var calculateDirection = moveInput.y * playerTransform.forward +
            moveInput.x * playerTransform.right;
        calculateDirection = calculateDirection.normalized;
        _locomotion.TargetPosition = calculateDirection;
        _locomotion.RelativeDirection = new Vector3(moveInput.x, 0, moveInput.y);
        _locomotion.IsMoving = actions[0];
        _locomotion.IsJumping = actions[1];
        _locomotion.IsSprinting = actions[2];
    }
    private void HandleUI(){
        if (_inputHandler.IsUIEnabled)
            Cursor.lockState = CursorLockMode.None;
        else if (Cursor.lockState == CursorLockMode.None && !_inputHandler.IsUIEnabled) Cursor.lockState = CursorLockMode.Locked;
        
    }

#region Messages
    #region Client Messages
    [MessageHandler((ushort)NetworkManager.ServerToClientId.playerSpawned)]
    private static void SpawnPlayerClient(Message message) {
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
            Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
        }
    }
    [MessageHandler((ushort) NetworkManager.ServerToClientId.playerMovement)]
    private static void ReceiveMovement(Message message) {
        if (list.TryGetValue(message.GetUShort(), out PlayerNetworkManager player)) {
            player.SetPositionAndRotation(message.GetVector3(), message.GetQuaternion());
        }
    }
    private void SendInput() {
        Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ClientToServerId.input);
        bool[] actions = new[] {
            _inputHandler.IsMoving,
            _inputHandler.IsJumping,
            _inputHandler.IsSprinting
        };
        message.AddVector2(_inputHandler.MovementInput);
        message.AddBools(actions);
        HandleLocomotion(_inputHandler.MovementInput, actions);
        message.AddQuaternion(_locomotion.Rb.rotation);
        NetworkManager.Singleton.Client.Send(message);
    }
    #endregion

    #region Server Messages
    private void SendMovement() {
        Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.playerMovement);
        message.AddUShort(this.Id);
        message.AddVector3(transform.position);
        message.AddQuaternion(transform.rotation);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
    [MessageHandler((ushort)NetworkManager.ClientToServerId.input)]
    private static void ReceiveInput(ushort fromClientId, Message message) {
        if (list.TryGetValue(fromClientId, out PlayerNetworkManager player)) {
            if (player.IsLocal && !NetworkManager.Singleton.IsServer) return;
            
            Vector2 moveInput = message.GetVector2();
            bool[] actions = message.GetBools();
            player.HandleLocomotion(moveInput, actions);
        }
    }
    [MessageHandler((ushort) NetworkManager.ClientToServerId.username)]
    private static void SpawnPlayerServer(ushort fromClientId, Message message) {
        if (NetworkManager.Singleton.IsServer) {
            Spawn(fromClientId, message.GetString(), GodEntity.Singleton.spawnPoint.position + 
                Vector3.right * Random.value * 4);
        }
    }
    private void NotifySpawn(ushort toClientId = UInt16.MaxValue) {
        NetworkManager net = NetworkManager.Singleton;
        if (net.IsServer) {
            Message message = AddSpawnData(Message.Create(
                MessageSendMode.reliable, (ushort)NetworkManager.ServerToClientId.playerSpawned));
            if (toClientId == UInt16.MaxValue)
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
