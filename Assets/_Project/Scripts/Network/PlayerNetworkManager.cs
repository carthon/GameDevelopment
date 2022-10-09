using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _Project.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
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
    private Inventory _inventory;
    private TextMeshProUGUI _usernameDisplay;

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }
    public string Username { get; private set; }

    public void InitializeComponents() {
        _inputHandler = GetComponent<InputHandler>();
        _locomotion = GetComponent<Locomotion>();
        _animator = GetComponent<AnimatorHandler>();
        _cameraHandler = GetComponent<CameraHandler>();
        _usernameDisplay = GetComponentInChildren<TextMeshProUGUI>();
        _animator.Initialize();
        _locomotion.SetUp();
    }
    private void OnSpawn() {
        InitializeComponents();
        _inputHandler.enabled = IsLocal;
        _cameraHandler.enabled = IsLocal;
        _usernameDisplay.text = Username;
        if (IsLocal) {
            Cursor.lockState = CursorLockMode.Locked;
            _cameraHandler.InitializeCamera();
            _inventory = new Inventory("PlayerInventory", 9);
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
            Physics.Simulate(fixedDelta);
            SendMovement();
        }
    }
    private void HandleRotation() {
        Quaternion newRotation = Quaternion.Euler(0.0f, _cameraHandler.CameraPivot.rotation.eulerAngles.y, 0.0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, _cameraHandler.CameraData.rotationMultiplier);
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
    private void HandleLocomotion(Vector3 moveInput, bool[] actions) {
        Transform cameraPivot = _cameraHandler.CameraPivot.transform;
        _locomotion.TargetPosition = CalculateDirection(moveInput, cameraPivot);
        _locomotion.RelativeDirection = moveInput;
    }
    private void HandleAnimations(bool[] actions) {
        _locomotion.IsMoving = actions[0];
        _locomotion.IsJumping = actions[1];
        _locomotion.IsSprinting = actions[2];
    }
    private void HandleUI(){
        if (_inputHandler.IsUIEnabled)
            Cursor.lockState = CursorLockMode.None;
        else if (Cursor.lockState == CursorLockMode.None && !_inputHandler.IsUIEnabled) Cursor.lockState = CursorLockMode.Locked;
        
    }
    private Vector3 CalculateDirection(Vector3 moveInput, Transform transform) {
        var calculateDirection = moveInput.z * transform.forward +
            moveInput.x * transform.right;
        calculateDirection.y = 0;
        return calculateDirection.normalized;
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
            if (!player.IsLocal) {
                player._locomotion.RelativeDirection = relativeDirection;
                player.HandleAnimations(actions);
                player._cameraHandler.CameraPivot.rotation = cameraRotation;
            }
        }
    }
    private void SendInput() {
        Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ClientToServerId.input);
        Vector3 moveInput = new Vector3(_inputHandler.Horizontal, 0, _inputHandler.Vertical);
        bool[] actions = new[] {
            _inputHandler.IsMoving,
            _inputHandler.IsJumping,
            _inputHandler.IsSprinting
        };
        Vector3 calculatedInput = CalculateDirection(_inputHandler.MovementInput, _cameraHandler.CameraPivot);
        message.AddVector3(moveInput);
        message.AddBools(actions);
        HandleLocomotion(moveInput, actions);
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
            _locomotion.IsSprinting
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
    [MessageHandler((ushort)NetworkManager.ClientToServerId.input)]
    private static void ReceiveInput(ushort fromClientId, Message message) {
        if (list.TryGetValue(fromClientId, out PlayerNetworkManager player)) {
            if (player.IsLocal && !NetworkManager.Singleton.IsServer) return;
            
            Vector3 moveInput = message.GetVector3();
            bool[] actions = message.GetBools();
            Quaternion cameraPivot = message.GetQuaternion();
            player._cameraHandler.CameraPivot.rotation = cameraPivot;
            player.HandleLocomotion(moveInput, actions);
            player.HandleAnimations(actions);
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
