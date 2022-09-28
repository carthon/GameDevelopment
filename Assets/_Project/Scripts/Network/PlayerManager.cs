using System.Collections;
using System.Collections.Generic;
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
    [MessageHandler((ushort) NetworkManager.ClientToServerId.name)]
    private static void 
}
