using System.Collections.Generic;
using _Project.Scripts;
using _Project.Scripts.Network;
using RiptideNetworking;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
    public static Dictionary<ushort, PlayerManager> list = new Dictionary<ushort, PlayerManager>();
    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }
    public string Username { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy() {
        list.Remove(Id);
    }
    private static void Spawn(ushort id, string username, Vector3 position) {
        NetworkManager net = NetworkManager.Singleton;
        PlayerManager player = Instantiate(GodEntity.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<PlayerManager>();
        if(net.IsServer) {
            foreach (PlayerManager otherPlayer in list.Values) {
                otherPlayer.SendSpawned(id);
            }
        }
        if (net.IsClient) {
            player.IsLocal = id == net.Client.Id;
        }
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)}";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;

        player.SendSpawned();
        list.Add(id, player);
    }

#region Messages
    [MessageHandler((ushort) NetworkManager.ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message) {
        if (NetworkManager.Singleton.IsServer) {
            Spawn(fromClientId, message.GetString(), GodEntity.Singleton.spawnPoint.position);
        }
    }
    [MessageHandler((ushort)NetworkManager.ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message) {
        if (NetworkManager.Singleton.IsClient) {
            Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
        }
    }
    private void SendSpawned() {
        NetworkManager net = NetworkManager.Singleton;
        if (net.IsServer) {
            Message message = AddSpawnData(Message.Create(
                MessageSendMode.reliable, (ushort)NetworkManager.ServerToClientId.playerSpawned));
            net.Server.SendToAll(message);
        }
    }
    private void SendSpawned(ushort toClientId) {
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
        return message;
    }
#endregion
}
