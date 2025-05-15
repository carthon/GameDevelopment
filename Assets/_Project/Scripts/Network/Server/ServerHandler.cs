using _Project.Scripts.Components;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Network.MessageUtils;
using _Project.Scripts.Utils;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.Server {
    public partial class Server {
        [MessageHandler((ushort) Client.Client.PacketHandler.serverInput)]
        private static void ReceiveInput(ushort fromClientId, Message message) {
            InputMessageStruct messageData = new InputMessageStruct(message);
            if (messageData.tick > NetworkManager.Singleton.Tick) {
                NetworkManager.Singleton.Server.AddPlayerInput(fromClientId, messageData);
            }
            else {
                SendPlayerDataToClient(fromClientId);
            }
        }
        [MessageHandler((ushort) Client.Client.PacketHandler.serverItemSwap)]
        private static void SlotSwapServer(ushort fromClientId, Message message) {
            int[] data = message.GetInts();
            int inventoryId = data[0];
            int otherInventoryId = data[1];
            Vector2Int slot = message.GetVector2Int();
            Vector2Int otherSlot = message.GetVector2Int();
            bool wasFlipped = message.GetBool();
            if (NetworkManager.playersList.TryGetValue(fromClientId, out Player player)) {
                Inventory otherInventory = player.InventoryManager.Inventories[otherInventoryId];
                player.InventoryManager.Inventories[inventoryId].SwapItemsInInventory(otherInventory, slot, otherSlot, wasFlipped);
            }
        }
        [MessageHandler((ushort) Client.Client.PacketHandler.serverItemDrop)]
        private static void DropItemOnSlotServer(ushort clientId, Message message) {
            if (NetworkManager.playersList.TryGetValue(clientId, out Player player)) {
                int inventoryId = message.GetInt();
                Vector2Int cellData = message.GetVector2Int();
                Vector3 position = message.GetVector3();
                Quaternion rotation = message.GetQuaternion();
                player.InventoryManager.DropItemStack(inventoryId, cellData, position, rotation);
            }
        }
        [MessageHandler((ushort) Client.Client.PacketHandler.serverUsername)]
        private static void SpawnPlayerServer(ushort fromClientId, Message message) {
            GameManager.Spawn(fromClientId, message.GetString(), GameManager.Singleton.spawnPoint.position +
                Vector3.right * Random.value * 4, NetworkManager.Singleton.Tick);
        }
        [MessageHandler((ushort) Client.Client.PacketHandler.serverItemEquip)]
        private static void SpawnItemOnPlayer(ushort clientId, Message message) {
            EquipmentMessageStruct equipmentData = new EquipmentMessageStruct(message);
            ItemStack itemStack = equipmentData.itemStack;
            BodyPart equipmentSlot = (BodyPart) equipmentData.equipmentSlot;
            bool activeState = equipmentData.activeState;
            if (NetworkManager.playersList.TryGetValue(clientId, out Player player) && !player.IsLocal) {
                //Actualizo el equipamiento en el servidor
                player.UpdateEquipment(itemStack, equipmentSlot, activeState);
                //Notifico al resto de jugadores
                player.NotifyEquipment(itemStack, equipmentSlot, activeState, equipmentData.clientId);
            }
        }
        [MessageHandler((ushort) Client.Client.PacketHandler.serverUpdateClient)]
        private static void SyncClientWorldData(ushort clientId, Message message) {
            SendGrabbables(clientId);
            SendPlayerDataToClient(clientId);
        }
    }
}