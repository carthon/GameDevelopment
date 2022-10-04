using _Project.Scripts.Components;

public class RunState : AbstractBaseState {
    public RunState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) { }
    public override void EnterState() {
        locomotion.CurrentMovementSpeed = locomotion.Stats.runSpeed;
    }
    public override void ExitState() {
    }
    public override void CheckSwitchStates() {
        if (locomotion.IsSprinting && locomotion.IsMoving)
            SwitchState(factory.Sprint());
        else if (!locomotion.IsMoving) SwitchState(factory.Idle());
    }
    public override void InitializeSubState() {
    }
    public override void UpdateState() {
        CheckSwitchStates();
    }
    public override string StateName { get => "RunState"; }
}