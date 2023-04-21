using _Project.Scripts.Components;

namespace _Project.Scripts.StateMachine.LocomotionStates {
    public class IdleState : AbstractBaseState {
        public IdleState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) { EnterState(); }
        public sealed override void EnterState() {
            locomotion.CurrentMovementSpeed = 0;
        }
        public override void ExitState() {
        }
        public override void CheckSwitchStates() {
            if (!locomotion.IsSprinting && locomotion.IsMoving)
                SwitchState(factory.Run());
            else if (locomotion.IsSprinting && locomotion.IsMoving) SwitchState(factory.Sprint());
        }
        public override void InitializeSubState() {
        }
        public override void UpdateState() {
            CheckSwitchStates();
        }
        public override string StateName { get => "IdleState"; }
    }
}