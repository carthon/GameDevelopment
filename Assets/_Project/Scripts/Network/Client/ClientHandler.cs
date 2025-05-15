using System.Collections.Generic;
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
using UnityEngine.Assertions;

namespace _Project.Scripts.Network.Client {
    public partial class Client {
        
        [MessageHandler((ushort)Server.Server.PacketHandler.clientInventoryChange)]
        private static void ReceiveSlotChangeServer(Message message) {
            if (Singleton is {IsServerOwner: true})
                return;
            ushort playerId = message.GetUShort();
            int inventoryId = message.GetInt();
            InventorySlot inventorySlot = message.GetInventorySlot();
            Singleton.Player.InventoryManager.SetInventorySlot(inventorySlot, inventoryId);
        }
        
        [MessageHandler((ushort)Server.Server.PacketHandler.spawnMessage)]
        private static void ReceiveSpawnPlayer(Message message) {
            if (Singleton is {IsServerOwner: true})
                return;
            SpawnMessageStruct spawnData = new SpawnMessageStruct(message);
            NetworkManager.Singleton.Tick = spawnData.tick + TicksAheadOfServer;
            GameManager.Spawn(spawnData.id, spawnData.entityId, spawnData.position, spawnData.tick);
            Message updateClient = Message.Create(MessageSendMode.reliable, PacketHandler.serverUpdateClient);
            NetworkManager.Singleton.Client.Send(updateClient);
        }
        
        [MessageHandler((ushort)Server.Server.PacketHandler.clientPlayerDespawn)]
        private static void ReceiveDeSpawnPlayer(Message message) {
            if (Singleton is {IsServerOwner: true})
                return;
            ushort playerId = message.GetUShort();
            if (NetworkManager.playersList.TryGetValue(playerId, out Player player)) {
                Object.Destroy(player.gameObject);
                NetworkManager.playersList.Remove(playerId);
            }
        }

        [MessageHandler((ushort) Server.Server.PacketHandler.grabbablesPosition)]
        private static void ReceiveGrabbableStatus(Message message) {
            if (Singleton is {IsServerOwner: true})
                return;

            GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(message);
            if (GameManager.grabbableItems.TryGetValue(grabbableData.grabbableId, out Grabbable grabbable)) {
                var transform = grabbable.transform;
                transform.position = grabbableData.position;
                transform.rotation = grabbableData.rotation;
            }
        }
        [MessageHandler((ushort) Server.Server.PacketHandler.movementMessage)]
        private static void ReceiveMovement(Message message) {
            if (Singleton is {IsServerOwner: true})
                return;
            MovementMessageStruct movementMessageStruct = new MovementMessageStruct(message);
            if (NetworkManager.playersList.TryGetValue(movementMessageStruct.id, out Player player)) {
                if(player.IsLocal) {
                    Singleton._latestServerMovement = movementMessageStruct;
                    if (NetworkManager.Singleton.debugServerPosition && NetworkManager.Singleton.Client._serverDummy != null) {
                        NetworkManager.Singleton.Client._serverDummy.UpdateServerDummy(movementMessageStruct);
                    }
                } else {
                    player.UpdatePlayerMovementState(movementMessageStruct, true, Time.deltaTime * movementMessageStruct.velocity.sqrMagnitude);
                }
            }
        }
        
        [MessageHandler((ushort) Server.Server.PacketHandler.clientReceiveEquipment)]
        private static void ReceiveEquipment(Message message) {
            if (Singleton is {IsServerOwner: true})
                return;
            EquipmentMessageStruct equipmentData = new EquipmentMessageStruct(message);
            if (NetworkManager.playersList.TryGetValue(equipmentData.clientId, out Player player)) {
                if (!player.IsLocal) {
                    ItemStack itemStack = equipmentData.itemStack;
                    BodyPart equipmentSlot = (BodyPart) equipmentData.equipmentSlot;
                    bool activeStatus = equipmentData.activeState;
                    player.UpdateEquipment(itemStack, equipmentSlot, activeStatus);
                }
            }
        }
        [MessageHandler((ushort) Server.Server.PacketHandler.clientReceivePlayerData)]
        private static void ReceivePlayerData(Message message) {
            if (Singleton is {IsServerOwner: true})
                return;
            PlayerDataMessageStruct playerData = new PlayerDataMessageStruct(message);
            NetworkManager.Singleton.Tick = playerData.tick + TicksAheadOfServer;
        }
        public static void DropItemStack(ItemStack itemStack, Vector3 position, Quaternion rotation) {
            if (NetworkManager.Singleton.IsClient) {
                Message message = Message.Create(MessageSendMode.reliable, PacketHandler.serverItemDrop);
                message.AddInt(itemStack.GetInventory().Id);
                message.AddVector2Int(itemStack.OriginalSlot);
                message.AddVector3(position);
                message.AddQuaternion(rotation);
                NetworkManager.Singleton.Client.Send(message);
            }
        }
    }
}