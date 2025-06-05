using _Project.Scripts.Utils;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates.Grounded {
    public class IdleState : AbstractBaseState {
        public IdleState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) { }
        public sealed override void EnterState() {
            locomotion.CurrentMovementSpeed = 0;
        }
        public override void ExitState() {
        }
        public override void CheckSwitchStates() {
            if (!LocomotionUtils.IsSprinting(locomotion.actions) && LocomotionUtils.IsMoving(locomotion.actions))
                SwitchState(factory.Run());
            if (LocomotionUtils.IsCrouching(locomotion.actions))
                SwitchState(factory.Crouch());
            else if (LocomotionUtils.IsSprinting(locomotion.actions) && LocomotionUtils.IsMoving(locomotion.actions)) SwitchState(factory.Sprint());
        }
        public override void InitializeSubState() {
        }
        public override void UpdateState() {
            CheckSwitchStates();
        }
        public override string StateName { get => "IdleState"; }
    }
}