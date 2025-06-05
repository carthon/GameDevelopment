using _Project.Scripts.Utils;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates.Grounded {
    public class CrouchState : AbstractBaseState {
        public CrouchState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) { }
        public sealed override void EnterState() {
            locomotion.CurrentMovementSpeed = locomotion.Stats.crouchSpeed;
        }
        public override void ExitState() {
        }
        public override void CheckSwitchStates() {
            if (!LocomotionUtils.IsCrouching(locomotion.actions))
                SwitchState(factory.Idle());
            else if (LocomotionUtils.IsJumping(locomotion.actions)) SwitchState(factory.Airborne());
        }
        public override void InitializeSubState() {
        }
        public override void UpdateState() {
            CheckSwitchStates();
        }
        public override string StateName { get => "CrouchState"; }
    }
}