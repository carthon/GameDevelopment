using _Project.Scripts.Utils;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates.Grounded {
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