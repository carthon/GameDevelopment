using System;
using System.Collections.Generic;
using System.Text;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Factories;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;
using static _Project.Scripts.Network.PacketType;
using Logger = _Project.Scripts.Utils.Logger;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Network.Server {
    public partial class ServerHandler : RiptideNetworking.Server {
        private const int ServerPositionSnapshotRate = 30;
        private int allowedBacklog = 0;

        #region ReconciliationVariables
        private Dictionary<ushort, InputRingBuffer> _unprocessedInputQueue = new Dictionary<ushort, InputRingBuffer>();
        #endregion

        private static ServerHandler _singleton;
        private readonly INetworkSender _networkSender;
        public static ServerHandler Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != null) {
                    Debug.Log($"{nameof (Client)} instance already exists, destroying duplicate!");
                }
            }
        }

        public ServerHandler(INetworkSender networkSender) {
            _singleton = this;
            _networkSender = networkSender;
            ClientDisconnected += PlayerLeft;
            //GameManager.Singleton.defaultPlanet.Generate();
        }

        ~ServerHandler() {
            ClientDisconnected += PlayerLeft;
            GC.SuppressFinalize(this);
        }
        
        public void Tick(int currentTick) {
            StringBuilder sb = new StringBuilder();
            sb.Append($"UnprocessedKeys ListCount:{_unprocessedInputQueue.Keys.Count}");
            IInputProcessor inputProcessor = new DefaultInputProcessor(new DefaultQueueManager(), allowedBacklog);
            IMovementApplier movementApplier = new DefaultMovementApplier();
            foreach (ushort clientId in _unprocessedInputQueue.Keys) {
                sb.Append($"ForClient:{clientId}");
                Queue<InputMessageStruct> inputQueue = new Queue<InputMessageStruct>(inputProcessor.GetInputsForTick(clientId, currentTick));
                while (inputQueue.TryDequeue(out InputMessageStruct inputMessage)) {
                    if (!NetworkManager.playersList.TryGetValue(clientId, out Player player))
                        continue;
                    movementApplier.ApplyMovement(player, inputMessage);
                    player.Locomotion.FixedTick();
                    MovementMessageStruct movementMessage = player.GetMovementState(currentTick);
                    SendMovement(movementMessage);
                }
            }
            if(currentTick % ServerPositionSnapshotRate == 0) {
                foreach (Grabbable grabbable in GameManager.grabbableItems.Values) {
                    GrabbableMessageStruct grabbableStruct = new GrabbableMessageStruct(grabbable);
                    _networkSender.Send(MessageSendMode.reliable, (ushort) clientGrabbablesPosition, grabbableStruct);
                }
            }
            UIHandler.Instance.UpdateWatchedVariables("InputData", sb.ToString());
            base.Tick();
        }
        private void AddPlayerInput(ushort playerId, InputMessageStruct inputMessageStruct) {
            if (!_unprocessedInputQueue.ContainsKey(playerId)) {
                _unprocessedInputQueue.Add(playerId, new InputRingBuffer(global::Constants.MAX_SERVER_INPUTS));
            }
            _unprocessedInputQueue[playerId].Enqueue(inputMessageStruct);
        }
        
        public void ReceiveInput(ushort fromClientId, Message message) {
            InputMessageStruct messageData = new InputMessageStruct(message);
            if (messageData.tick > NetworkManager.Singleton.Tick) {
                NetworkManager.Singleton.ServerHandler.AddPlayerInput(fromClientId, messageData);
            }
            else {
                SendPlayerDataToClient(fromClientId);
            }
        }
        public void ReceiveSlotSwap(ushort fromClientId, Message message) {
            int[] data = message.GetInts();
            int inventoryId = data[0];
            int otherInventoryId = data[1];
            int slot = data[2];
            int otherSlot = data[3];
            if (NetworkManager.playersList.TryGetValue(fromClientId, out Player player)) {
                Inventory otherInventory = player.InventoryManager.Inventories[otherInventoryId];
                player.InventoryManager.Inventories[inventoryId].SwapItemsInInventory(otherInventory, slot, otherSlot);
            }
        }
        public void ReceiveSlotChange(ushort fromClientId, Message message) {
            ushort playerId = message.GetUShort();
            int inventoryId = message.GetInt();
            if (playerId != fromClientId) {
                Logger.Singleton.Log($"Cliente: {fromClientId} ha notificado sobre un slot del jugador {playerId}", Logger.Type.ERROR);
                return;
            }
            InventorySlot inventorySlot = message.GetInventorySlot();
            if (NetworkManager.playersList.TryGetValue(playerId, out Player player)) {
                player.InventoryManager.SetInventorySlot(inventorySlot, inventoryId);
                _networkSender.Send(MessageSendMode.reliable, (ushort) clientItemSlotChange, new InventorySlotMessageStruct(playerId, inventoryId, inventorySlot));
            }
        }
        public void ReceiveDropItemAtSlot(ushort clientId, Message message) {
            if (NetworkManager.playersList.TryGetValue(clientId, out Player player)) {
                int[] data = message.GetInts();
                Vector3 position = message.GetVector3();
                Quaternion rotation = message.GetQuaternion();
                player.InventoryManager.DropItemStack(data[0], data[1], position, rotation);
            }
        }
        public void ReceiveSpawnPlayer(ushort fromClientId, Message message) {
            PlayerConnectionMessageStruct playerConnectionMessage = new PlayerConnectionMessageStruct(message);
            SpawnPlayerOnServer(fromClientId, playerConnectionMessage.username, GameManager.Singleton.spawnPoint.position +
                Vector3.right * Random.value * 4, NetworkManager.Singleton.Tick);
        }
        public void ReceiveDisplayItemOnPlayer(ushort clientId, Message message) {
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
        public void SyncClientWorldData(ushort clientId, Message message) {
            SendGrabbables(clientId);
            SendPlayerDataToClient(clientId);
        }
        /**<summary>
         * <param name="ofPlayer">Información del jugador</param>
         * <p>Replica la información del jugador al resto de clientes conectados</p>
         * </summary>
         */
        private void SendMovement(MovementMessageStruct movementMessageStruct) {
            _networkSender.Send(MessageSendMode.reliable, (ushort) clientMovementMessage, 
                movementMessageStruct);
        }
        public void SendGrabbables(ushort toClientId = 0) {
            foreach (Grabbable grabbable in GameManager.grabbableItems.Values) {
                Transform transform = grabbable.transform;
                GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(grabbable.Id, grabbable.GetItemStack(), transform.position, transform.rotation);
                _networkSender.Send(MessageSendMode.reliable, (ushort) clientItemSpawn, grabbableData);
            }
        }
        
        /**<summary>
         *  <param name="toClientId">[Optional] cliente a notificar del spawn</param>
         * <p>Se ejecuta como servidor: notifica a uno o todos los clientes de un spawn de jugador nuevo</p>
         * </summary>
         */
        public void SpawnPlayerOnServer(ushort id, string username, Vector3 position, int currentTick)
        {
            var player = SpawnFactory.CreatePlayerInstance(
                GameManager.Singleton.PlayerPrefab, id, username, position);
            player.SetNetworking(_networkSender);
            player.IsLocal = false;
            NetworkManager.playersList.Add(id, player);
            foreach (var other in NetworkManager.playersList.Values)
                NotifySpawn(other, id);
            NotifySpawn(player, currentTick);
        }
        private void NotifySpawn(Player player, int currentTick, ushort toClientId = 0) {
            Transform transform = player.transform;
            PlayerSpawnMessageStruct playerSpawnData = new PlayerSpawnMessageStruct(player.Id, player.Username, transform.position, transform.rotation, currentTick);
            _networkSender.Send(MessageSendMode.reliable, (ushort) clientSpawnMessage, playerSpawnData);
            SendGrabbables(toClientId);
        }
        public void SpawnGrabbableOnServer(ItemStack item, Vector3 position, Quaternion rotation)
        {
            var pickable = SpawnFactory.CreateGrabbableInstance(item, position, rotation);
            GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(pickable.Id, item, position, rotation);
            NetworkManager.Singleton.ServerHandler._networkSender.Send(MessageSendMode.reliable, (ushort) clientItemSpawn, 
                grabbableData);
        }
        public void PlayerLeft(object sender, ClientDisconnectedEventArgs e) {
            if (NetworkManager.playersList.TryGetValue(e.Id, out Player player)) {
                Object.Destroy(player.gameObject);
                NetworkManager.playersList.Remove(e.Id);
                _networkSender.Send(MessageSendMode.reliable, (ushort) clientPlayerDespawn, 
                    new PlayerDespawnMessageStruct(e.Id));
                Logger.Singleton.Log($"Player {e.Id} disconnected", Logger.Type.INFO);
            }
        }
        private void SendPlayerDataToClient(ushort id = 0) {
            if (NetworkManager.Singleton.IsServer) {
                if (NetworkManager.playersList.TryGetValue(id, out Player player)) {
                    PlayerDataMessageStruct playerData = PlayerDataMessage.getPlayerData(player);
                    _networkSender.Send(MessageSendMode.reliable, (ushort) clientReceivePlayerData, playerData);
                }
            }
        }

    }
}