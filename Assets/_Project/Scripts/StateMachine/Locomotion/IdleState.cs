using _Project.Scripts.Components;

public class IdleState : AbstractBaseState {
    public IdleState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) { }
    public override void EnterState() {
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
}