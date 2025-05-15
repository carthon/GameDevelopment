using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Network {
    public interface IInputProcessor { 
        IEnumerable<InputMessageStruct> GetInputsForTick(ushort clientId, int currentTick);
    }
}