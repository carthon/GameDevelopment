using System;
using System.Collections.Generic;
using _Project.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using Cinemachine;
using RiptideNetworking;
using TMPro.Examples;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Random = UnityEngine.Random;

public class PlayerNetworkManager : MonoBehaviour {
    public static Dictionary<ushort, PlayerNetworkManager> list = new Dictionary<ushort, PlayerNetworkManager>();
    private InputHandler _inputHandler;
    private Locomotion _locomotion;
    private CameraHandler _cameraHandler;
    private Inventory _inventory;
    
    [SerializeField] private Transform _camTransform;
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
        if (NetworkManager.Singleton.IsClient) {
            Cursor.lockState = CursorLockMode.Locked;
            _cameraHandler.InitializeCamera();
            _inventory = new Inventory("PlayerInventory", 9);
        }
    }
    private void FixedUpdate() {
        float delta = Time.deltaTime;
        if (NetworkManager.Singleton.IsClient) {
            _cameraHandler.Tick(delta);
            _cameraHandler.FixedTick(delta);
            HandleRotation(new Vector2(_inputHandler.MouseX, _inputHandler.MouseY));
            SendInput();
            _inputHandler?.ClearInputs();
        }
        if (NetworkManager.Singleton.IsServer) {
            _locomotion.Tick();
            Physics.Simulate(Time.fixedDeltaTime);
            SendMovement();
        }
    }
    private void HandleRotation(Vector2 mouseInput) {
        var rb = _locomotion.Rb;
        rb.rotation = Quaternion.Euler(0.0f, rb.rotation.eulerAngles.y +
            mouseInput.x * _cameraHandler.CameraData.rotationMultiplier * Time.deltaTime, 0.0f);
    }

    private void OnDestroy() {
        enabled = false;
        NotifySpawn(isDeSpawning:true);
        list.Remove(Id);
    }
    private static void Spawn(ushort id, string username, Vector3 position) {
        NetworkManager net = NetworkManager.Singleton;
        PlayerNetworkManager playerNetwork = Instantiate(GodEntity.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<PlayerNetworkManager>();
        if(net.IsServer) {
            foreach (PlayerNetworkManager otherPlayer in list.Values) {
                otherPlayer.NotifySpawn(id);
            }
        }
        if (net.IsClient) {
            playerNetwork.IsLocal = id == net.Client.Id;
            GodEntity.Singleton.PlayerInstance = playerNetwork;
        }
        playerNetwork.name = $"Player {id} {(string.IsNullOrEmpty(username) ? "Guest" : username)}";
        playerNetwork.Id = id;
        playerNetwork.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
        playerNetwork.OnSpawn();

        playerNetwork.NotifySpawn();
        list.Add(id, playerNetwork);
    }
    private void SetPosition(Vector3 position, Vector3 forwardVector) {
        transform.position = position;
        if (!IsLocal) {
            _camTransform.forward = forwardVector;
        }
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
            player.SetPosition(message.GetVector3(), message.GetVector3());
        }
    }
    private void SendInput() {
        if (IsLocal){
            Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ClientToServerId.input);
            message.AddVector2(_inputHandler.MovementInput);
            message.AddQuaternion(_locomotion.Rb.rotation);
            message.AddQuaternion(_cameraHandler.CameraPivot.rotation);
            bool[] actions = new[] {
                _inputHandler.IsMoving,
                _inputHandler.IsJumping,
                _inputHandler.IsSprinting
            };
            message.AddBools(actions);
            NetworkManager.Singleton.Client.Send(message);
        }
    }
    #endregion

    #region Server Messages
    private void SendMovement() {
        Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.playerMovement);
        message.AddUShort(this.Id);
        message.AddVector3(transform.position);
        message.AddVector3(_camTransform.forward);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
    [MessageHandler((ushort)NetworkManager.ClientToServerId.input)]
    private static void ReceiveInput(ushort fromClientId, Message message) {
        if (list.TryGetValue(fromClientId, out PlayerNetworkManager player)) {
            Vector2 moveInput = message.GetVector2();
            Quaternion rotation = message.GetQuaternion();
            Quaternion headRotation = message.GetQuaternion();
            bool[] actions = message.GetBools();
            
            Transform playerTransform = player.transform;
            var calculateDirection = moveInput.y * playerTransform.forward +
                moveInput.x * playerTransform.right;
            calculateDirection = calculateDirection.normalized;
            player._cameraHandler.CameraPivot.rotation = headRotation;
            player._locomotion.TargetPosition = calculateDirection;
            player._locomotion.RelativeDirection = new Vector3(moveInput.x, 0, moveInput.y);
            player._locomotion.Rb.rotation = rotation;
            player._locomotion.IsMoving = actions[0];
            player._locomotion.IsJumping = actions[1];
            player._locomotion.IsSprinting = actions[2];
        }
    }
    [MessageHandler((ushort) NetworkManager.ClientToServerId.username)]
    private static void SpawnPlayerServer(ushort fromClientId, Message message) {
        if (NetworkManager.Singleton.IsServer) {
            Spawn(fromClientId, message.GetString(), GodEntity.Singleton.spawnPoint.position + 
                Vector3.right * Random.value * 4);
        }
    }
    private void NotifySpawn(ushort toClientId = UInt16.MaxValue, bool isDeSpawning = false) {
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
