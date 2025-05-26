using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Network {
    public interface IInputQueueManager {
        bool Enqueue(ushort clientId, InputMessageStruct input);
        bool TryPeek(ushort clientId, out InputMessageStruct peeked);
        bool TryDequeue(ushort clientId, out InputMessageStruct dequeued);
        public bool TryPeekTail(ushort clientId, out InputMessageStruct peeked);
        void RemoveClient(ushort clientId);
        IEnumerable<ushort> GetActivePlayers();
        int GetCount(ushort clientId);
        void AddClient(ushort clientId);
    }
}