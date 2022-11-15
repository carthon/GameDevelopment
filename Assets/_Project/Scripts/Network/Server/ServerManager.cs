using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.Server {
    public class ServerManager : RiptideNetworking.Server {
        public static Dictionary<ushort, PlayerNetworkManager> playersList = new Dictionary<ushort, PlayerNetworkManager>();
        public override void Tick() {
            float fixedDelta = Time.fixedDeltaTime;
            float delta = Time.deltaTime;
            foreach (PlayerNetworkManager player in playersList.Values) {
                //Revisar porque no se eliminan al no estar un jugador
                if (!player.IsLocal) {
                    player.Locomotion.FixedTick(fixedDelta);
                    SendMovement(player);
                }
            }
            base.Tick();
        }
        private void SendMovement(PlayerNetworkManager toPlayer) {
            Locomotion locomotion = toPlayer.Locomotion;
            bool[] actions = new[] {
                locomotion.IsMoving,
                locomotion.IsJumping,
                locomotion.IsSprinting,
                locomotion.IsGrounded
            };
            MovementMessageStruct movementStruct = new MovementMessageStruct(toPlayer.Id, 
                toPlayer.transform.position, locomotion.Rb.velocity,locomotion.RelativeDirection, toPlayer.transform.rotation,
                toPlayer.HeadRotation, actions);
            NetworkMessage networkMessage = new NetworkMessage(MessageSendMode.reliable, (ushort) NetworkManager.ServerToClientId.clientPlayerMovement, movementStruct);
            networkMessage.Send(false);
        }
        
        [MessageHandler((ushort)NetworkManager.ClientToServerId.serverInput)]
        private static void ReceiveInput(ushort fromClientId, Message message) {
            if (playersList.TryGetValue(fromClientId, out PlayerNetworkManager player)) {
                InputMessageStruct messageData = new InputMessageStruct(message);
            
                Vector3 moveInput = messageData.moveInput;
                bool[] actions = messageData.actions;
                int clientTick = messageData.clientTick;
                Quaternion playerHeadRotation = messageData.headPivotRotation;
                player.HeadPivot.rotation = playerHeadRotation;
                player.HandleLocomotion(moveInput);
                player.HandleAnimations(actions);
                if (actions[3]) player.HandlePicking();
            }
        }
        
        [MessageHandler((ushort)NetworkManager.ClientToServerId.serverItemSwap)]
        private static void SlotSwapServer(ushort fromClientId, Message message) {
            int[] data = message.GetInts();
            int inventoryId = data[0];
            int otherInventoryId = data[1];
            int slot = data[2];
            int otherSlot = data[3];
            if (playersList.TryGetValue(fromClientId, out PlayerNetworkManager player)) {
                Inventory otherInventory = player.InventoryManager.Inventories[otherInventoryId];
                player.InventoryManager.Inventories[inventoryId].SwapItemsInInventory(otherInventory, slot, otherSlot);
            }
        }
        
        [MessageHandler((ushort) NetworkManager.ClientToServerId.serverItemDrop)]
        private static void DropItemOnSlotServer(ushort clientId, Message message) {
            if (playersList.TryGetValue(clientId, out PlayerNetworkManager player)) {
                int[] data = message.GetInts();
                player.InventoryManager.DropItemStack(data[0], data[1]);
            }
        }

        [MessageHandler((ushort) NetworkManager.ClientToServerId.serverUsername)]
        private static void SpawnPlayerServer(ushort fromClientId, Message message) {
            PlayerNetworkManager.Spawn(fromClientId, message.GetString(), GodEntity.Singleton.spawnPoint.position + 
                    Vector3.right * Random.value * 4);
        }
        
        [MessageHandler((ushort) NetworkManager.ClientToServerId.serverItemEquip)]
        private static void SpawnItemOnPlayer(ushort clientId, Message message) {
            if (NetworkManager.Singleton.IsServer) {
                EquipmentMessageStruct equipmentData = new EquipmentMessageStruct(message);
                ItemStack itemStack = equipmentData.itemStack;
                BodyPart equipmentSlot = (BodyPart) equipmentData.equipmentSlot;
                bool activeState = equipmentData.activeState;
                if (playersList.TryGetValue(clientId, out PlayerNetworkManager player) && !player.IsLocal) {
                    //Actualizo el equipamiento en el servidor
                    player.UpdateEquipment(itemStack, equipmentSlot, activeState);
                    //Notifico al resto de jugadores
                    player.NotifyEquipment(itemStack, equipmentSlot, activeState, equipmentData.clientId);
                }
            }
        }
        
        [MessageHandler((ushort) NetworkManager.ClientToServerId.serverUpdateClient)]
        private static void SyncClientWorldData(ushort clientId, Message message) {
            NetworkManager.GrabbableToClient(clientId);
            NetworkManager.PlayersDataToClient(clientId);
        }
    }
}