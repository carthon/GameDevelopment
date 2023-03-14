using _Project.Scripts.Components;
using _Project.Scripts.StateMachine.LocomotionStates;

public class SprintState : AbstractBaseState {
    public SprintState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) {
        EnterState();
    }
    public sealed override void EnterState() {
        locomotion.CurrentMovementSpeed = locomotion.Stats.sprintSpeed;
    }
    public override void ExitState() {
    }
    public override void CheckSwitchStates() {
        if (!locomotion.IsSprinting && locomotion.IsMoving)
            SwitchState(factory.Run());
        else if (!locomotion.IsMoving) SwitchState(factory.Idle());
    }
    public override void InitializeSubState() {
    }
    public override void UpdateState() {
        CheckSwitchStates();
    }
    public override string StateName { get => "SprintState"; }
}