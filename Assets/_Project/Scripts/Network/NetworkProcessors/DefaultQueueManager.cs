using System.Collections.Generic;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;

namespace _Project.Scripts.Network {
    public class DefaultQueueManager : IInputQueueManager {
        private readonly Dictionary<ushort, InputRingBuffer> _unprocessedInputQueue = new Dictionary<ushort, InputRingBuffer>();

        public bool Enqueue(ushort clientId, InputMessageStruct input) {
            if (_unprocessedInputQueue.TryGetValue(clientId, out InputRingBuffer inputBuffer)) {
                inputBuffer.Enqueue(input);
                return true;
            }
            return false;
        }
        public bool TryPeek(ushort clientId, out InputMessageStruct peeked) {
            peeked = new InputMessageStruct();
            return _unprocessedInputQueue.TryGetValue(clientId, out InputRingBuffer inputBuffer) && inputBuffer.Peek(out peeked);
        }
        public bool TryPeekTail(ushort clientId, out InputMessageStruct peeked) {
            peeked = new InputMessageStruct();
            return _unprocessedInputQueue.TryGetValue(clientId, out InputRingBuffer inputBuffer) && inputBuffer.Tail(out peeked);
        }
        public bool TryDequeue(ushort clientId, out InputMessageStruct dequeued) {
            dequeued = new InputMessageStruct();
            if (!_unprocessedInputQueue.TryGetValue(clientId, out InputRingBuffer inputBuffer))
                return false;
            return inputBuffer.Dequeue(out dequeued);
        }
        public void RemoveClient(ushort clientId) {
            if (!_unprocessedInputQueue.Remove(clientId)) {
                Logger.Singleton.Log($"ClientHandler {clientId} could not be removed", Logger.Type.ERROR);
            }
        }
        public IEnumerable<ushort> GetActivePlayers() {
            // Necesita mantener un yield return ya que no captura variables locales en lambdas y no incurre en ningun cierre
            // que genere objetos en el heap
            foreach (KeyValuePair<ushort,InputRingBuffer> keyValue in _unprocessedInputQueue) {
                if (keyValue.Value.Count > 0)
                    yield return keyValue.Key;
            }
        }
        public int GetCount(ushort clientId) => _unprocessedInputQueue[clientId].Count;
        public void AddClient(ushort clientId) { _unprocessedInputQueue.Add(clientId, new InputRingBuffer(global::Constants.MAX_SERVER_INPUTS)); }
    }
}