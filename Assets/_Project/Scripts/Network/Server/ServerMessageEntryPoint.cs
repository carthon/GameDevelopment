using _Project.Scripts.Network.Client;
using RiptideNetworking;
using static _Project.Scripts.Network.PacketType;

namespace _Project.Scripts.Network.Server {
    public static class ServerMessageEntryPoint {
        [MessageHandler((ushort) serverInput)]
        private static void ReceiveInput(ushort clientId, Message msg) => ServerHandler.Singleton.ReceiveInput(clientId, msg);

        [MessageHandler((ushort)serverItemSwap)]
        private static void SlotSwapServer(ushort clientId, Message msg) => ServerHandler.Singleton.ReceiveSlotSwap(clientId, msg);

        [MessageHandler((ushort)serverItemDrop)]
        private static void DropItemOnSlotServer(ushort clientId, Message msg) => ServerHandler.Singleton.ReceiveDropItemAtSlot(clientId, msg);

        [MessageHandler((ushort)serverUsername)]
        private static void SpawnPlayerServer(ushort clientId, Message msg) => ServerHandler.Singleton.ReceiveSpawnPlayer(clientId, msg);

        [MessageHandler((ushort)serverItemEquip)]
        private static void SpawnItemOnPlayer(ushort clientId, Message msg) => ServerHandler.Singleton.ReceiveDisplayItemOnPlayer(clientId, msg);

        [MessageHandler((ushort)serverUpdateClient)]
        private static void SyncClientWorldData(ushort clientId, Message msg) => ServerHandler.Singleton.SyncClientWorldData(clientId, msg);
        
        [MessageHandler((ushort) serverInventoryChange)]
        private static void ReceiveSlotChangeServer(ushort clientId, Message message) => ServerHandler.Singleton.ReceiveSlotChange(clientId, message);
        [MessageHandler((ushort) serverItemPick)]
        private static void ReceiveItemPick(ushort clientId, Message message) => ServerHandler.Singleton.ReceiveItemPick(clientId, message);
    }
}