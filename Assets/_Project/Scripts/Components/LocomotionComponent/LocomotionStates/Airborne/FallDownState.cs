namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates.Airborne {
    public class FallDownState : AbstractBaseState {

        public FallDownState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) { EnterState();}
        public override string StateName => "FallDown";
        public sealed override void EnterState() {
            //locomotion.CurrentMovementSpeed = locomotion.Stats.inAirSpeed;
        }
        public override void ExitState() {}
        public override void CheckSwitchStates() {
            if (locomotion.IsGrounded) SwitchState(factory.Grounded());
        }
        public override void InitializeSubState() {}
        public override void UpdateState() {
            CheckSwitchStates();
        }
    }
}