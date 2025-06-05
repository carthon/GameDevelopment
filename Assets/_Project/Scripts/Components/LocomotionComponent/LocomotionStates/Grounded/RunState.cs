using _Project.Scripts.Utils;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates.Grounded {
    public class RunState : AbstractBaseState {
        public RunState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) { }
        public sealed override void EnterState() {
            locomotion.CurrentMovementSpeed = locomotion.Stats.runSpeed;
        }
        public override void ExitState() {
        }
        public override void CheckSwitchStates() {
            if (LocomotionUtils.IsSprinting(locomotion.actions) && LocomotionUtils.IsMoving(locomotion.actions)) SwitchState(factory.Sprint());
            else if (!LocomotionUtils.IsMoving(locomotion.actions)) SwitchState(factory.Idle());
            else if (LocomotionUtils.IsCrouching(locomotion.actions)) SwitchState(factory.Crouch());
        }
        public override void InitializeSubState() {
        }
        public override void UpdateState() {
            CheckSwitchStates();
        }
        public override string StateName { get => "RunState"; }
    }
}