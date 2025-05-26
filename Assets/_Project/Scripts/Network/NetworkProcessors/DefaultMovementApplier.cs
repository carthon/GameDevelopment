using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Network {
    public class DefaultMovementApplier : IMovementApplier{
        public void ApplyMovement(Player player, InputMessageStruct inputMessageStruct, float delta) {
            ulong actions = inputMessageStruct.actions;
            player.HeadPivot.rotation = inputMessageStruct.headPivotRotation;
            player.SetActions(actions);
            player.HandleAnimations(actions);
            player.HandleLocomotion(delta, inputMessageStruct.moveInput);
        }
    }
}