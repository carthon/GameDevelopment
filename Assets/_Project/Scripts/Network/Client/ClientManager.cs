using _Project.Scripts.Handlers;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.UI;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.Client {
    public class ClientManager : RiptideNetworking.Client {
        private PlayerNetworkManager _player;
        public PlayerNetworkManager Player
        {
            get
            {
                if (_player != null)
                    return _player;
                return null;
            }
        }
    public override void Tick() {
            if (_player != null) {
                float fixedDelta = Time.fixedDeltaTime;
                float delta = Time.deltaTime;
                Vector3 mouseInput = (CameraHandler.Singleton.GetDirectionFromMouse(InputHandler.Singleton.MouseX, InputHandler.Singleton.MouseY));
                if (!InputHandler.Singleton.IsUIEnabled)
                    CameraHandler.Singleton.FixedTick(fixedDelta);
                HandleUI();
                CameraHandler.Singleton.Tick(delta);
                SendInputs();
                InputHandler.Singleton.ClearInputs();
            }
            base.Tick();
        }
        private void HandleUI(){
            if (InputHandler.Singleton.IsUIEnabled) {
                Cursor.lockState = CursorLockMode.None;
                if (!UIHandler.Instance.ShowingInventory) UIHandler.Instance.TriggerInventory(0);
            }
            else if (Cursor.lockState == CursorLockMode.None && !InputHandler.Singleton.IsUIEnabled) {
                Cursor.lockState = CursorLockMode.Locked;
                if (UIHandler.Instance.ShowingInventory) UIHandler.Instance.TriggerInventory(0);
            }
        
            if (InputHandler.Singleton.HotbarSlot != UIHandler.Instance._hotbarUi.ActiveSlot) {
                HotbarUI hotbar = UIHandler.Instance._hotbarUi;
                hotbar.ActiveSlot = InputHandler.Singleton.HotbarSlot;
                ItemLinks linkedItemLinkInSlot = hotbar.GetItemLinkInSlot(hotbar.ActiveSlot);
                if (linkedItemLinkInSlot != null && linkedItemLinkInSlot.LinkedStacks.Count > 0 && linkedItemLinkInSlot.LinkedStacks[0].GetCount() > 0) {
                    ItemStack itemStack = linkedItemLinkInSlot.LinkedStacks[0];
                    _player.EquipmentHandler.LoadItemModel(itemStack, BodyPart.RightArm);
                    _player.NotifyEquipment(itemStack, BodyPart.RightArm, true, Id);
                }
                else if (InputHandler.Singleton.HotbarSlot != -1 && _player.EquipmentHandler.GetEquipmentSlotByBodyPart(BodyPart.RightArm).IsActive){
                    _player.EquipmentHandler.UnloadItemModel(BodyPart.RightArm);
                    _player.NotifyEquipment(ItemStack.EMPTY, BodyPart.RightArm, false, Id);
                }
            }
        }
        private void SendInputs() {
            InputHandler inputHandler = InputHandler.Singleton;
            Vector3 moveInput = new Vector3(inputHandler.Horizontal, 0, inputHandler.Vertical);
            bool[] actions = new[] {
                inputHandler.IsMoving,
                inputHandler.IsJumping,
                inputHandler.IsSprinting,
                inputHandler.IsPicking
            };
            InputMessageStruct inputData = new InputMessageStruct(moveInput, actions, CameraHandler.Singleton.CameraPivot.rotation,NetworkManager.ClientTick);
            NetworkMessage networkMessage = new NetworkMessage(MessageSendMode.reliable, (ushort)NetworkManager.ClientToServerId.serverInput, inputData);
            networkMessage.Send(true);
            _player.HandleLocomotion(moveInput);
            _player.HandleAnimations(actions);
        }

        [MessageHandler((ushort)NetworkManager.ServerToClientId.clientInventoryChange)]
        private static void ReceiveSlotChangeServer(Message message) {
                ushort playerId = message.GetUShort();
                int inventoryId = message.GetInt();
                ItemStack itemStack = message.GetItemStack();
                NetworkManager.Singleton.Client._player.InventoryManager.SetItemStackInInventory(itemStack, inventoryId);
        }
        [MessageHandler((ushort)NetworkManager.ServerToClientId.clientPlayerSpawned)]
        private static void SpawnPlayerClient(Message message) {
            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
                SpawnMessageStruct spawnData = new SpawnMessageStruct(message);
                PlayerNetworkManager.Spawn(spawnData.id, spawnData.username, spawnData.position);
            }
        }
        
        [MessageHandler((ushort)NetworkManager.ServerToClientId.clientPlayerDespawn)]
        private static void DeSpawnPlayer(Message message) {
            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
                ushort playerId = message.GetUShort();
                if (PlayerNetworkManager.playersList.TryGetValue(playerId, out PlayerNetworkManager player)) {
                    GameObject.Destroy(player.gameObject);
                    PlayerNetworkManager.playersList.Remove(playerId);
                }
            }
        }
        [MessageHandler((ushort) NetworkManager.ServerToClientId.clientPlayerMovement)]
        private static void ReceiveMovement(Message message) {
            MovementMessageStruct movementMessageStruct = new MovementMessageStruct(message);
            if (PlayerNetworkManager.playersList.TryGetValue(movementMessageStruct.id, out PlayerNetworkManager player)) {
                bool[] actions = movementMessageStruct.actions;
                player.SetPositionAndRotation(movementMessageStruct.position, movementMessageStruct.velocity, movementMessageStruct.rotation);
                player.Locomotion.IsGrounded = actions[3];
                if (!player.IsLocal) {
                    player.Locomotion.RelativeDirection = movementMessageStruct.relativeDirection;
                    player.HandleAnimations(actions);
                    player.HeadPivot.rotation = movementMessageStruct.headPivotRotation;
                }
            }
        }
        [MessageHandler((ushort) NetworkManager.ServerToClientId.clientReceiveEquipment)]
        private static void ReceiveEquipment(Message message) {
            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
                EquipmentMessageStruct equipmentData = new EquipmentMessageStruct(message);
                if (PlayerNetworkManager.playersList.TryGetValue(equipmentData.clientId, out PlayerNetworkManager player)) {
                    if (!player.IsLocal) {
                        ItemStack itemStack = equipmentData.itemStack;
                        BodyPart equipmentSlot = (BodyPart) equipmentData.equipmentSlot;
                        bool activeStatus = equipmentData.activeState;
                        player.UpdateEquipment(itemStack, equipmentSlot, activeStatus);
                    }
                }
            }
        }
        public void SetPlayer(PlayerNetworkManager player) => _player = player;
    }
}