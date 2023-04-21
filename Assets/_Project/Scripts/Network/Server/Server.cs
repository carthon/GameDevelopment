using System.Collections.Generic;
using System.Text;
using _Project.Scripts.Components;
using _Project.Scripts.Entities;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.Server {
    public partial class Server : RiptideNetworking.Server {
        private const int ServerPositionSnapshotRate = 30;

        #region ReconciliationVariables
        private Dictionary<ushort, Queue<InputMessageStruct>> _unprocessedInputQueue = new Dictionary<ushort, Queue<InputMessageStruct>>();

        #endregion

        private static Server _singleton;
        public static Server Singleton
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

        public Server() {
            _singleton = this;
        }

        public void Tick(int currentTick) {
            StringBuilder sb = new StringBuilder();
            sb.Append($"UnprocessedKeys ListCount:{_unprocessedInputQueue.Keys.Count}");
            foreach (ushort clientId in _unprocessedInputQueue.Keys) {
                sb.Append($"ForClient:{clientId}");
                if(NetworkManager.playersList.TryGetValue(clientId, out Player player)) {
                    sb.Append($"TotalInput:{_unprocessedInputQueue[clientId].Count}");
                    while(_unprocessedInputQueue[clientId].Count > 0) {
                        if (_unprocessedInputQueue[clientId].Peek().tick > currentTick) {
                            _unprocessedInputQueue[clientId].Dequeue();
                            continue;
                        }
                        sb.Append($"tick at peek: {_unprocessedInputQueue[clientId].Peek().tick} and current: {currentTick}");
                        InputMessageStruct inputMessage = _unprocessedInputQueue[clientId].Dequeue();
                        bool[] actions = inputMessage.actions;
                        Quaternion playerHeadRotation = inputMessage.headPivotRotation;
                        player.HeadPivot.rotation = playerHeadRotation;
                        player.SetActions(actions);
                        player.HandleAnimations(actions);
                        player.HandleLocomotion(NetworkManager.Singleton.minTimeBetweenTicks, inputMessage.moveInput);
                    }
                    player.Locomotion.FixedTick();
                    MovementMessageStruct movementMessage = player.GetMovementState(currentTick);
                    SendMovement(movementMessage);
                }
            }
            if(currentTick % ServerPositionSnapshotRate == 0) {
                foreach (Grabbable grabbable in GodEntity.grabbableItems.Values) {
                    GrabbableMessageStruct grabbableStruct = new GrabbableMessageStruct(grabbable);
                    NetworkMessageBuilder message = new NetworkMessageBuilder(MessageSendMode.reliable, (int) PacketHandler.grabbablesPosition, grabbableStruct);
                    message.Send(asServer: true);
                }
            }
            UIHandler.Instance.UpdateWatchedVariables("InputData", sb.ToString());
            base.Tick();
        }
        private void AddPlayerInput(ushort playerId, InputMessageStruct inputMessageStruct) {
            if (!_unprocessedInputQueue.ContainsKey(playerId)) {
                _unprocessedInputQueue.Add(playerId, new Queue<InputMessageStruct>());
            }
            _unprocessedInputQueue[playerId].Enqueue(inputMessageStruct);
        }
        /**<summary>
         * <param name="ofPlayer">Información del jugador</param>
         * <p>Replica la información del jugador al resto de clientes conectados</p>
         * </summary>
         */
        private void SendMovement(MovementMessageStruct movementMessageStruct) {
            NetworkMessageBuilder networkMessageBuilder = new NetworkMessageBuilder(MessageSendMode.reliable, (ushort) PacketHandler.movementMessage, movementMessageStruct);
            networkMessageBuilder.Send(asServer:true);
        }
        public static void SendGrabbables(ushort toClientId = 0) {
            foreach (Grabbable grabbable in GodEntity.grabbableItems.Values) {
                Transform transform = grabbable.transform;
                GrabbableMessageStruct grabbableData = new GrabbableMessageStruct(grabbable.Id, grabbable.itemData.id, transform.position, transform.rotation);
                NetworkMessageBuilder messageBuilder = new NetworkMessageBuilder(MessageSendMode.reliable, (ushort) PacketHandler.clientItemSpawn, grabbableData);
                messageBuilder.Send(toClientId, asServer:true);
            }
        }
        /**<summary>
     *  <param name="toClientId">[Optional] cliente a notificar del spawn</param>
     * <p>Se ejecuta como servidor: notifica a uno o todos los clientes de un spawn de jugador nuevo</p>
     * </summary>
     */
        public static void NotifySpawn(Player player, int currentTick, ushort toClientId = 0) {
            Transform transform = player.transform;
            SpawnMessageStruct spawnData = new SpawnMessageStruct(player.Id, player.Username, transform.position, transform.rotation, currentTick);
            NetworkMessageBuilder networkMessageBuilder = new NetworkMessageBuilder(MessageSendMode.reliable, (ushort) PacketHandler.spawnMessage, spawnData);
            networkMessageBuilder.Send(toClientId, asServer:true);
            SendGrabbables(toClientId);
        }
        public static void SendPlayerDataToClient(ushort id = 0) {
            if (NetworkManager.Singleton.IsServer) {
                if (NetworkManager.playersList.TryGetValue(id, out Player player)) {
                    PlayerDataMessageStruct playerData = PlayerDataMessage.getPlayerData(player);
                    NetworkMessageBuilder messageBuilder = new NetworkMessageBuilder(MessageSendMode.reliable, (ushort) PacketHandler.clientReceivePlayerData, playerData);
                    messageBuilder.Send(id, asServer: true);
                }
            }
        }

    }
}