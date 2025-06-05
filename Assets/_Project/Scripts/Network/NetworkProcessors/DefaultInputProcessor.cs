using System.Collections.Generic;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;

namespace _Project.Scripts.Network {
    public class DefaultInputProcessor : IInputProcessor {
        private readonly IInputQueueManager _queueManager;
        private readonly int _allowedBacklog;
        private Dictionary<ushort, LocomotionInputMessage> _lastInputRegistered = new Dictionary<ushort, LocomotionInputMessage>();
        
        public DefaultInputProcessor(IInputQueueManager queueManager, int allowedBackLog) {
            _queueManager = queueManager;
            _allowedBacklog = allowedBackLog;
        }
        public void AddInput(ushort playerId, LocomotionInputMessage locomotionInputMessage) {
            _lastInputRegistered.TryAdd(playerId, locomotionInputMessage);
            _lastInputRegistered[playerId] = locomotionInputMessage;
            _queueManager.Enqueue(playerId, locomotionInputMessage);
        }
        public IEnumerable<ushort> GetPlayers() {
            return _queueManager.GetActivePlayers();
        }
        public int GetTotalInputsForClient(ushort clientId) => _queueManager.GetCount(clientId);
        public void RemoveClient(ushort clientId) {
            _queueManager.RemoveClient(clientId);
        }
        public void AddClient(ushort clientId) {
            _queueManager.AddClient(clientId);
        }
        public LocomotionInputMessage GetInputForTick(ushort clientId, int currentTick) {
            // 1) descartar inputs demasiado viejos
            while (_queueManager.TryPeek(clientId, out var oldest) &&
                   oldest.tick < currentTick - _allowedBacklog) {
                _queueManager.TryDequeue(clientId, out oldest);
                Logger.Singleton.Log($"Dequeuing old input at tick {oldest.tick} for client {clientId}", Logger.Type.DEBUG);
            }
            // 2) si hay uno para este tick, procesarlo
            LocomotionInputMessage currentTickMessage = new LocomotionInputMessage(currentTick);
            if (_queueManager.TryPeek(clientId, out LocomotionInputMessage next) && next.tick == currentTick) {
                Logger.Singleton.Log($"[{currentTick}]Peeking input: client {clientId} {next}", Logger.Type.DEBUG);
                if (!_queueManager.TryDequeue(clientId, out currentTickMessage)) {
                    Logger.Singleton.Log($"Error dequeuing for tick {currentTick} and client {clientId}", Logger.Type.DEBUG);
                }
            }
            // 2) si falta, usar el último conocido
            //if (_lastInputRegistered.TryGetValue(clientId, out var last))
            //    return last;
            return currentTickMessage;
        }
    }
}