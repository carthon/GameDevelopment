using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Network {
    public interface IInputQueueManager {
        bool Enqueue(ushort clientId, InputMessageStruct input);
        bool TryPeek(ushort clientId, out InputMessageStruct peeked);
        bool TryDequeue(ushort clientId, out InputMessageStruct dequeued);
        void RemoveClient(ushort clientId);
    }
}