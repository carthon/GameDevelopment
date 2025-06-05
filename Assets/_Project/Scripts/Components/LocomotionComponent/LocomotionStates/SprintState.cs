using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates {
    public class SprintState : AbstractBaseState {
        public SprintState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) { }
        public sealed override void EnterState() {
            locomotion.CurrentMovementSpeed = locomotion.Stats.sprintSpeed;
        }
        public override void ExitState() {
        }
        public override void CheckSwitchStates() {
            if (!LocomotionUtils.IsSprinting(locomotion.actions) && LocomotionUtils.IsMoving(locomotion.actions))
                SwitchState(factory.Run());
            else if (!LocomotionUtils.IsMoving(locomotion.actions)) SwitchState(factory.Idle());
        }
        public override void InitializeSubState() {
        }
        public override void UpdateState() {
            CheckSwitchStates();
        }
        public override string StateName { get => "SprintState"; }
    }
}