using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;

namespace _Project.Scripts.Network.Server {
    public partial class Server {
        public enum PacketHandler : ushort {
            spawnMessage = 1,
            movementMessage,
            clientPlayerDespawn,
            clientItemDespawn,
            clientItemSpawn,
            clientInventoryChange,
            clientReceiveEquipment,
            clientReceivePlayerData,
            grabbablesPosition,
        }
    }
}