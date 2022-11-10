using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Network.Server {
    public class ServerManager : MonoBehaviour {
        public static Dictionary<ushort, PlayerNetworkManager> playersList = new Dictionary<ushort, PlayerNetworkManager>();
    }
}