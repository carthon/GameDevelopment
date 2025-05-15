using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Network {
    public class DefaultInputProcessor : IInputProcessor {
        private readonly IInputQueueManager _queueManager;
        private readonly int _allowedBacklog;
        
        public DefaultInputProcessor(IInputQueueManager queueManager, int allowedBackLog) {
            _queueManager = queueManager;
            _allowedBacklog = allowedBackLog;
        }
        public IEnumerable<InputMessageStruct> GetInputsForTick(ushort clientId, int currentTick) {
            var results = new List<InputMessageStruct>();
            while (_queueManager.TryPeek(clientId, out var peek)) {
                if (peek.tick < currentTick - _allowedBacklog) {
                    // Descarta muy antiguos
                    _queueManager.TryDequeue(clientId, out _);
                    continue;
                }
                if (peek.tick > currentTick) {
                    // Aún no toca procesarlo
                    break;
                }
                // Es el tick actual: lo consumimos
                _queueManager.TryDequeue(clientId, out var input);
                results.Add(input);
            }
            return results;
        }
    }
}