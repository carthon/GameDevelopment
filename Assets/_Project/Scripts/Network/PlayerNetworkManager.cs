using System;
using System.Collections.Generic;
using _Project.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
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
    [SerializeField] private Transform _camTransform;
    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }
    public string Username { get; private set; }

    private void Start() {
    }
    public void Initialize() {
        _inputHandler = GetComponent<InputHandler>();
        _locomotion = GetComponent<Locomotion>();
        _locomotion.SetUp();
    }
    private void OnSpawn() {
        Initialize();
        _inputHandler.enabled = IsLocal;
    }
    private void FixedUpdate() {
        if (NetworkManager.Singleton.IsClient) {
            SendInput();
            _inputHandler?.ClearInputs();
        }
        if (NetworkManager.Singleton.IsServer) {
            _locomotion.Tick();
            Physics.Simulate(Time.fixedDeltaTime);
            SendMovement();
        }
    }

    private void OnDestroy() {
        enabled = false;
        NotifySpawn(isDeSpawning:true);
        list.Remove(Id);
    }
    private static void Spawn(ushort id, string username, Vector3 position, bool isDeSpawning = false) {
        NetworkManager net = NetworkManager.Singleton;
        if (isDeSpawning) {
            Debug.Log($"DeSpawning ID: {id}");
            Destroy(list[id].gameObject);
            return;
        }
        PlayerNetworkManager playerNetwork = Instantiate(GodEntity.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<PlayerNetworkManager>();
        if(net.IsServer) {
            foreach (PlayerNetworkManager otherPlayer in list.Values) {
                otherPlayer.NotifySpawn(id);
            }
        }
        if (net.IsClient) {
            playerNetwork.IsLocal = id == net.Client.Id;
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
            Spawn(message.GetUShort(), message.GetString(), message.GetVector3(), message.GetBool());
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
            bool[] actions = message.GetBools();
            player._locomotion.TargetPosition = new Vector3(moveInput.x,0, moveInput.y);
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
                MessageSendMode.reliable, (ushort)NetworkManager.ServerToClientId.playerSpawned).AddBool(isDeSpawning));
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
