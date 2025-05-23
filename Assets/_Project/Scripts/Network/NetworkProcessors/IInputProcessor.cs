using System.Collections.Generic;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Network {
    public interface IInputProcessor { 
        InputMessageStruct GetInputForTick(ushort clientId, int currentTick);
        void AddInput(ushort playerId, InputMessageStruct inputMessageStruct);
        IEnumerable<ushort> GetPlayers();
        int GetTotalInputsForClient(ushort clientId);
    }
}