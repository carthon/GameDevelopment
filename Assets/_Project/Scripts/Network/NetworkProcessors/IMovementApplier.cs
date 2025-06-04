using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Network {
    public interface IMovementApplier {
        public void ApplyMovement(Player player, InputMessageStruct inputMessageStruct);
    }
}