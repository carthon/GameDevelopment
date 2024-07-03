namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates {
    public class CrouchState : AbstractBaseState {
        public CrouchState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) { EnterState(); }
        public sealed override void EnterState() {
            locomotion.CurrentMovementSpeed = locomotion.Stats.crouchSpeed;
        }
        public override void ExitState() {
        }
        public override void CheckSwitchStates() {
            if (!locomotion.IsCrouching)
                SwitchState(factory.Idle());
            else if (locomotion.IsJumping) SwitchState(factory.Jump());
        }
        public override void InitializeSubState() {
        }
        public override void UpdateState() {
            CheckSwitchStates();
        }
        public override string StateName { get => "CrouchState"; }
    }
}