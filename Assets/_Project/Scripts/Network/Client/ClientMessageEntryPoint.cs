using RiptideNetworking;
using static _Project.Scripts.Network.PacketType;

namespace _Project.Scripts.Network.Client {
    public static class ClientMessageEntryPoint {

        [MessageHandler((ushort) clientSpawnMessage)]
        private static void ReceiveSpawnPlayer(Message message) => ClientHandler.Singleton.ReceiveSpawnPlayer(message);

        [MessageHandler((ushort) clientPlayerDespawn)]
        private static void ReceiveDeSpawnPlayer(Message message) => ClientHandler.Singleton.ReceiveDespawnPlayer(message);

        [MessageHandler((ushort) clientGrabbablesPosition)]
        private static void ReceiveGrabbableStatus(Message message) => ClientHandler.Singleton.ReceiveGrabbableStatus(message);

        [MessageHandler((ushort) clientMovementMessage)]
        private static void ReceiveMovement(Message message) => ClientHandler.Singleton.ReceiveMovement(message);

        [MessageHandler((ushort) clientReceiveEquipment)]
        private static void ReceiveEquipment(Message message) => ClientHandler.Singleton.ReceiveEquipment(message);

        [MessageHandler((ushort) clientReceivePlayerData)]
        private static void ReceivePlayerData(Message message) => ClientHandler.Singleton.ReceivePlayerData(message);

        [MessageHandler((ushort) clientItemSpawn)]
        private static void SpawnItemClient(Message message) => ClientHandler.Singleton.ReceiveSpawnItem(message);

        [MessageHandler((ushort)clientItemDespawn)]
        private static void DestroyItem(Message message) => ClientHandler.Singleton.ReceiveDestroyItem(message);
        
        [MessageHandler((ushort)clientItemSlotChange)]
        private static void InventorySlotChange(Message message) => ClientHandler.Singleton.ReceiveInventorySlotChange(message);
    }
}