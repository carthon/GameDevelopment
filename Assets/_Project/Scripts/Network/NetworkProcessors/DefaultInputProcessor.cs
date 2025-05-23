using System.Collections.Generic;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;

namespace _Project.Scripts.Network {
    public class DefaultInputProcessor : IInputProcessor {
        private readonly IInputQueueManager _queueManager;
        private readonly int _allowedBacklog;
        private Dictionary<ushort, InputMessageStruct> _lastInputRegistered = new Dictionary<ushort, InputMessageStruct>();
        
        public DefaultInputProcessor(IInputQueueManager queueManager, int allowedBackLog) {
            _queueManager = queueManager;
            _allowedBacklog = allowedBackLog;
        }
        public void AddInput(ushort playerId, InputMessageStruct inputMessageStruct) {
            _lastInputRegistered.TryAdd(playerId, inputMessageStruct);
            _lastInputRegistered[playerId] = inputMessageStruct;
            _queueManager.Enqueue(playerId, inputMessageStruct);
        }
        public IEnumerable<ushort> GetPlayers() {
            return _queueManager.GetActivePlayers();
        }
        public int GetTotalInputsForClient(ushort clientId) => _queueManager.GetCount(clientId);
        public InputMessageStruct GetInputForTick(ushort clientId, int currentTick) {
            // 1) descartar inputs demasiado viejos
            while (_queueManager.TryPeek(clientId, out var oldest) &&
                   oldest.tick < currentTick - _allowedBacklog) {
                _queueManager.TryDequeue(clientId, out oldest);
                Logger.Singleton.Log($"Dequeuing old inputs at tick {oldest.tick}", Logger.Type.DEBUG);
            }

            // 2) si hay uno para este tick, procesarlo
            InputMessageStruct currentTickMessage = new InputMessageStruct(currentTick);
            if (_queueManager.TryPeek(clientId, out var next) && next.tick == currentTick) {
                if (_queueManager.TryDequeue(clientId, out currentTickMessage)) {
                    Logger.Singleton.Log($"Dequeuing input for tick {currentTick}: {currentTickMessage.tick}", Logger.Type.DEBUG);
                }
            }

            // 3) si no, repetir el último (fallback)
            
            return currentTickMessage;
        }
    }
}