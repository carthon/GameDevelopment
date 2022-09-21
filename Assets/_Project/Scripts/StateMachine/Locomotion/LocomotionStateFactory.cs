using _Project.Scripts.Components;
using Scripts.StateMachine.Locomotion;

public class LocomotionStateFactory {
    private readonly Locomotion _context;
    public LocomotionStateFactory(Locomotion currentContext) {
        _context = currentContext;
    }

    public AbstractBaseState Idle() {
        return new IdleState(this, _context);
    }
    public AbstractBaseState Run() {
        return new RunState(this, _context);
    }
    public AbstractBaseState Sprint() {
        return new SprintState(this, _context);
    }
    public AbstractBaseState Grounded() {
        return new GroundedState(this, _context);
    }
    public AbstractBaseState Jump() {
        var jumpState = new JumpState(this, _context);
        jumpState.SetJumped(true);
        return jumpState;
    }
    public AbstractBaseState Fall() {
        return new JumpState(this, _context);
    }
}