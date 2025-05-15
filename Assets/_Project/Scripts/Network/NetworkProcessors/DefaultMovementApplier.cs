using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageDataStructures;
using UnityEngine;

namespace _Project.Scripts.Network {
    public class DefaultMovementApplier : IMovementApplier{
        public void ApplyMovement(Player player, InputMessageStruct movementMessageStruct) {
            bool[] actions = movementMessageStruct.actions;
            Quaternion playerHeadRotation = movementMessageStruct.headPivotRotation;
            player.HeadPivot.rotation = playerHeadRotation;
            player.SetActions(actions);
            player.HandleAnimations(actions);
            player.HandleLocomotion(NetworkManager.Singleton.minTimeBetweenTicks, movementMessageStruct.moveInput);
        }
    }
}