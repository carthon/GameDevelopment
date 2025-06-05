using System.Collections.Generic;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Network {
    public interface IInputProcessor { 
        LocomotionInputMessage GetInputForTick(ushort clientId, int currentTick);
        void AddInput(ushort playerId, LocomotionInputMessage locomotionInputMessage);
        IEnumerable<ushort> GetPlayers();
        int GetTotalInputsForClient(ushort clientId);
        void RemoveClient(ushort clientId);
        void AddClient(ushort clientId);
    }
}