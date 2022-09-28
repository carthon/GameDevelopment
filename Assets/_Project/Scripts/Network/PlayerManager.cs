using System;
using System.Collections.Generic;
using System.Globalization;
using _Project.Scripts;
using _Project.Scripts.Components;
using RiptideNetworking;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
    public static Dictionary<ushort, PlayerManager> list = new Dictionary<ushort, PlayerManager>();
    public ushort Id { get; private set; }
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

    [MessageHandler((ushort) NetworkManager.ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message) {
        Spawn(fromClientId, message.GetString(), new Vector3(0,1f,0));
    }
    private static void Spawn(ushort id, string username, Vector3 position) {
        PlayerManager player = Instantiate(GodEntity.Singleton.playerPrefab, position, Quaternion.identity).GetComponent<PlayerManager>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)}";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
        
        list.Add(id, player);
    }
}
