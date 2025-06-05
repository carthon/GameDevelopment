using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;

namespace _Project.Scripts.Network {
    public class DefaultMovementApplier : IMovementApplier{
        public void ApplyMovement(Player player, LocomotionInputMessage locomotionInputMessage) {
            ulong actions = locomotionInputMessage.actions;
            player.HeadPivot.rotation = locomotionInputMessage.headPivotRotation;
            player.SetActions(actions);
            player.HandleAnimations(locomotionInputMessage.actions);
            if (player.LocomotionBridge != null)
                player.LocomotionBridge.EnqueueInput(locomotionInputMessage);
        }
    }
}