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
using Action = System.Action;
using Logger = _Project.Scripts.Utils.Logger;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Network.Server {
    public class ServerHandler : RiptideNetworking.Server {
        private const int ServerPositionSnapshotRate = 30;
        private int allowedBacklog = 0;

        private static ServerHandler _singleton;
        private readonly INetworkSender _networkSender;
        private IInputProcessor _inputProcessor;
        private IMovementApplier _movementApplier;
        private StringBuilder _sb;
        public NetworkManager NetworkManager { get; private set; }
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

        public ServerHandler(INetworkSender networkSender, NetworkManager networkManager) {
            _singleton = this;
            _networkSender = networkSender;
            NetworkManager = networkManager;
            _movementApplier = new DefaultMovementApplier();
            _inputProcessor = new DefaultInputProcessor(new DefaultQueueManager(), allowedBacklog);
            _sb = new StringBuilder();
            ClientDisconnected += PlayerLeft;
        }

        ~ServerHandler() {
            ClientDisconnected += PlayerLeft;
            GC.SuppressFinalize(this);
        }
        
        public void Tick(int currentTick) {
            List<ushort> clientIds = new List<ushort>(_inputProcessor.GetPlayers());
            _sb.Append($"UnprocessedKeys ListCount:{clientIds.Count}");
            foreach (ushort clientId in clientIds) {
                _sb.Append($"ForClient:{clientId} InputQueueCount: {_inputProcessor.GetTotalInputsForClient(clientId)}");
                if (!NetworkManager.playersList.TryGetValue(clientId, out Player player))
                    continue;
                InputMessageStruct inputMessage = _inputProcessor.GetInputForTick(clientId, currentTick);
                Logger.Singleton.Log($"InputForTick {currentTick}: {inputMessage}", Logger.Type.DEBUG);
                _movementApplier.ApplyMovement(player, inputMessage, NetworkManager.NetworkTimer.MinTimeBetweenTicks);
                player.Locomotion.FixedTick();
                MovementMessageStruct movementMessage = player.GetMovementState(currentTick);
                Logger.Singleton.Log($"Movement {currentTick}: {movementMessage}", Logger.Type.DEBUG);
                _networkSender.SendToClients(MessageSendMode.unreliable, (ushort) clientMovementMessage, movementMessage);
            }
            if(currentTick % ServerPositionSnapshotRate == 0) {
                foreach (Grabbable grabbable in GameManager.grabbableItems.Values) {
                    GrabbableMessageStruct grabbableStruct = new GrabbableMessageStruct(grabbable);
                    _networkSender.SendToClients(MessageSendMode.reliable, (ushort) clientGrabbablesPosition, grabbableStruct);
                }
            }
            UIHandler.Instance.UpdateWatchedVariables("InputData", _sb.ToString());
            _sb.Clear();
            base.Tick();
        }
        private void AddPlayerInput(ushort playerId, InputMessageStruct inputMessageStruct) {
            _inputProcessor.AddInput(playerId, inputMessageStruct);
        }
        public void ReceiveInput(ushort fromClientId, Message message) {
            if (NetworkManager.IsHost && fromClientId == NetworkManager.ClientHandler.Id) return;
            InputMessageStruct messageData = new InputMessageStruct(message);
            UIHandler.Instance.UpdateWatchedVariables("DiffTicks", $"Diff between client ticks {messageData.tick - NetworkManager.NetworkTimer.CurrentTick}");
            Logger.Singleton.Log($"[{NetworkManager.NetworkTimer.CurrentTick}]Received Input {fromClientId}: {messageData}", Logger.Type.DEBUG);
            if (messageData.tick >= NetworkManager.NetworkTimer.CurrentTick) {
                NetworkManager.ServerHandler.AddPlayerInput(fromClientId, messageData);
            }
            else {
                Logger.Singleton.Log($"Sending player actualization clientTick:{messageData.tick} serverTick: {NetworkManager.NetworkTimer.CurrentTick}", Logger.Type.DEBUG);
                SendPlayerDataToClient(fromClientId);
            }
        }
        public void ReceiveSlotSwap(ushort fromClientId, Message message) {
            if (NetworkManager.IsHost && fromClientId == NetworkManager.ClientHandler.Id) return;
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
        public void ReceiveSlotChange(ushort fromClientId, Message message) {
            if (NetworkManager.IsHost && fromClientId == NetworkManager.ClientHandler.Id) return;
            ushort playerId = message.GetUShort();
            int inventoryId = message.GetInt();
            if (playerId != fromClientId) {
                Logger.Singleton.Log($"Cliente: {fromClientId} ha notificado sobre un slot del jugador {playerId}", Logger.Type.ERROR);
                return;
            }
            InventorySlot inventorySlot = message.GetInventorySlot();
            if (NetworkManager.playersList.TryGetValue(playerId, out Player player)) {
                player.InventoryManager.SetInventorySlot(inventorySlot, inventoryId);
                _networkSender.SendToClients(MessageSendMode.reliable, (ushort) clientItemSlotChange, new InventorySlotMessageStruct(playerId, inventoryId, inventorySlot));
            }
        }
        public void ReceiveDropItemAtSlot(ushort fromClientId, Message message) {
            if (NetworkManager.IsHost && fromClientId == NetworkManager.ClientHandler.Id) return;
            GrabbableMessageStruct grabbable = new GrabbableMessageStruct(message);
            if (NetworkManager.playersList.TryGetValue(fromClientId, out Player player)) {
                player.InventoryManager.DropItemStack(grabbable.inventoryId, grabbable.itemStack.OriginalSlot, grabbable.position, grabbable.rotation);
            }
        }
        public void ReceiveSpawnPlayer(ushort fromClientId, Message message) {
            PlayerConnectionMessageStruct playerConnectionMessage = new PlayerConnectionMessageStruct(message);
            _inputProcessor.AddClient(fromClientId);
            Logger.Singleton.Log($"Cliente: {fromClientId} se está spawneando", Logger.Type.DEBUG);
            SpawnPlayerOnServer(fromClientId, playerConnectionMessage.username, GameManager.Singleton.spawnPoint.position +
                Vector3.right * Random.value * 4, NetworkManager.NetworkTimer.CurrentTick);
        }
        public void ReceiveDisplayItemOnPlayer(ushort clientId, Message message) {
            if (NetworkManager.IsHost && clientId == NetworkManager.ClientHandler.Id) return;
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
        }
        public void SendGrabbables(ushort toClientId = 0) {
            foreach (Grabbable grabbable in GameManager.grabbableItems.Values) {
                Transform transform = grabbable.transform;
                GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(grabbable.Id, grabbable.GetItemStack(), transform.position, transform.rotation);
                _networkSender.SendToClients(MessageSendMode.reliable, (ushort) clientItemSpawn, grabbableData, toClientId);
            }
        }
        private void SpawnPlayerOnServer(ushort id, string username, Vector3 position, int currentTick) {
            var player = SpawnFactory.CreatePlayerInstance(
                GameManager.Singleton.PlayerPrefab, id, username, position);
            player.SetNetworking(_networkSender, NetworkManager);
            foreach (var otherPlayers in NetworkManager.playersList.Values) {
                if (NetworkManager.IsHost) break;
                NotifySpawn(otherPlayers, currentTick, id);
            }
            NetworkManager.playersList.Add(id, player);
            NotifySpawn(player, currentTick);
        }
        private void NotifySpawn(Player player, int currentTick, ushort toClientId = 0) {
            Transform transform = player.transform;
            PlayerSpawnMessageStruct playerSpawnData = new PlayerSpawnMessageStruct(player.Id, player.Username, transform.position, transform.rotation, currentTick);
            _networkSender.SendToClients(MessageSendMode.reliable, (ushort) clientSpawnMessage, playerSpawnData, toClientId);
            SendGrabbables(toClientId);
        }
        public void SpawnGrabbableOnServer(ItemStack item, Vector3 position, Quaternion rotation)
        {
            var pickable = SpawnFactory.CreateGrabbableInstance(item, position, rotation);
            GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(pickable.Id, item, position, rotation);
            NetworkManager.ServerHandler._networkSender.SendToClients(MessageSendMode.reliable, (ushort) clientItemSpawn, 
                grabbableData);
        }
        public void PlayerLeft(object sender, ClientDisconnectedEventArgs e) {
            if (NetworkManager.playersList.TryGetValue(e.Id, out Player player)) {
                Object.Destroy(player.gameObject);
                NetworkManager.playersList.Remove(e.Id);
                PlayerDespawnMessageStruct playerDespawn = new PlayerDespawnMessageStruct(e.Id);
                _inputProcessor.RemoveClient(e.Id);
                _networkSender.SendToClients(MessageSendMode.reliable, (ushort) clientPlayerDespawn, playerDespawn);
                Logger.Singleton.Log($"Player {e.Id} disconnected", Logger.Type.DEBUG);
            }
        }
        private void SendPlayerDataToClient(ushort id = 0) {
            if (!NetworkManager.playersList.TryGetValue(id, out Player player))
                return;
            PlayerDataMessageStruct playerData = PlayerDataMessage.getPlayerData(player, NetworkManager.NetworkTimer.CurrentTick);
            _networkSender.SendToClients(MessageSendMode.reliable, (ushort) clientReceivePlayerData, playerData, id);
        }
        public void PickUpGrabbable(ushort clientId, Message message) {
            if (!NetworkManager.playersList.TryGetValue(clientId, out Player player))
                return;
            GrabbableMessageStruct grabbableMessage = new GrabbableMessageStruct(message);
            if (!GameManager.grabbableItems.TryGetValue(grabbableMessage.grabbableId, out Grabbable grabbable))
                return;
            if (!player.HandlePicking(grabbable))
                return;
            Object.Destroy(grabbable.gameObject);
            _networkSender.SendToClients(MessageSendMode.reliable, (ushort)clientItemDespawn, grabbableMessage);
        }
        public void PrintPlayersInventory() {
            Debug.Log("Printing Inventories");
            foreach (Player player in NetworkManager.playersList.Values) {
                foreach (Inventory inventory in player.InventoryManager.Inventories) {
                    Logger.Singleton.Log($"PlayerInventory{player.Id}-{inventory.Id}:\n{inventory}", Logger.Type.INFO);
                }
            }
        }
    }
}