using System;
using System.Collections.Generic;
using _Project.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network;
using RiptideNetworking;
using TMPro.Examples;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerNetworkManager : MonoBehaviour {
    public static Dictionary<ushort, PlayerNetworkManager> list = new Dictionary<ushort, PlayerNetworkManager>();
    private InputHandler _inputHandler;
    private Locomotion _locomotion;
    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }
    public string Username { get; private set; }

    private void Start() {
        _inputHandler = GetComponent<InputHandler>();
        
    }
    private void OnSpawn() {
        _locomotion = GetComponent<Locomotion>();
        if (IsLocal) _inputHandler = GetComponent<InputHandler>();
    }
    private void FixedUpdate() {
        SendInput();
        _inputHandler?.ClearInputs();
    }

    private void OnDestroy() {
        enabled = false;
        NotifySpawn();
        list.Remove(Id);
    }
    private static void Spawn(ushort id, string username, Vector3 position, bool isDeSpawning = false) {
        NetworkManager net = NetworkManager.Singleton;
        if (isDeSpawning && net.IsClient) {
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

#region Messages
#region Client Messages
    [MessageHandler((ushort)NetworkManager.ServerToClientId.playerSpawned)]
    private static void SpawnPlayerClient(Message message) {
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
            Spawn(message.GetUShort(), message.GetString(), message.GetVector3(), message.GetBool());
        }
    }
    private void SendInput() {
        if (NetworkManager.Singleton.IsClient && IsLocal){
            Message message = Message.Create(MessageSendMode.unreliable, NetworkManager.ClientToServerId.input);
            message.AddFloats(new[] {
                _inputHandler.Vertical,
                _inputHandler.Horizontal,
            });
            message.AddBools(new[] {
                _inputHandler.IsJumping,
                _inputHandler.IsRolling,
                _inputHandler.IsSprinting,
            });
            NetworkManager.Singleton.Client.Send(message);
        }
    }
    #endregion

    #region Server Messages
    [MessageHandler((ushort) NetworkManager.ClientToServerId.username)]
    private static void SpawnPlayerServer(ushort fromClientId, Message message) {
        if (NetworkManager.Singleton.IsServer) {
            Spawn(fromClientId, message.GetString(), GodEntity.Singleton.spawnPoint.position + 
                Vector3.right * Random.value * 4, message.GetBool());
        }
    }
    private void NotifySpawn() {
        NetworkManager net = NetworkManager.Singleton;
        if (net.IsServer) {
            Message message = AddSpawnData(Message.Create(
                MessageSendMode.reliable, (ushort)NetworkManager.ServerToClientId.playerSpawned));
            net.Server.SendToAll(message);
        }
    }
    private void NotifySpawn(ushort toClientId) {
        NetworkManager net = NetworkManager.Singleton;
        if (net.IsServer) {
            Message message = AddSpawnData(Message.Create(
                MessageSendMode.reliable, (ushort)NetworkManager.ServerToClientId.playerSpawned));
            net.Server.Send(message, toClientId);
        }
    }
    private Message AddSpawnData(Message message) {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        message.AddBool(isActiveAndEnabled);
        return message;
    }
    #endregion
#endregion
}
