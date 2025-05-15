using System.Collections.Generic;
using _Project.Scripts.Components;
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
            if (_unprocessedInputQueue.TryGetValue(clientId, out InputRingBuffer inputBuffer)) {
                inputBuffer.Peek(out peeked);
                return true;
            }
            return false;
        }
        public bool TryDequeue(ushort clientId, out InputMessageStruct dequeued) {
            dequeued = new InputMessageStruct();
            if (_unprocessedInputQueue.TryGetValue(clientId, out InputRingBuffer inputBuffer)) {
                inputBuffer.Dequeue(out dequeued);
                return true;
            }
            return false;
        }
        public void RemoveClient(ushort clientId) {
            if (!_unprocessedInputQueue.Remove(clientId)) {
                Logger.Singleton.Log($"ClientHandler {clientId} could not be removed", Logger.Type.ERROR);
            }
        }
    }
}