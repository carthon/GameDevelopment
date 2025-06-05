namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates {
    public class LocomotionStateFactory {
        private readonly Locomotion _context;
        private IdleState _idleState;
        private RunState _runState;
        private SprintState _sprintState;
        private GroundedState _groundedState;
        private FlyState _flyState;
        private AirborneState _airborneState;
        private FallDownState _fallState;
        private JumpUpState _jumpState;
        private CrouchState _crouchState;
        public LocomotionStateFactory(Locomotion currentContext) {
            _context = currentContext;
            Grounded();
            Airborne();
            Idle();
            Run();
            Sprint();
            Crouch();
            JumpUp();
            Fall();
            Fly();
        }
        public AbstractBaseState Idle() => _idleState ??= new IdleState(this, _context);
        public AbstractBaseState Run() => _runState ??= new RunState(this, _context);
        public AbstractBaseState Sprint() => _sprintState ??= new SprintState(this, _context);
        public AbstractBaseState Grounded() => _groundedState ??= new GroundedState(this, _context);
        public AbstractBaseState Fly() => _flyState ??= new FlyState(this, _context);
        public AbstractBaseState Airborne() => _airborneState ??= new AirborneState(this, _context);
        public AbstractBaseState Fall() => _fallState ??= new FallDownState(this, _context);
        public AbstractBaseState JumpUp() => _jumpState ??= new JumpUpState(this, _context);
        public AbstractBaseState Crouch() => _crouchState ??= new CrouchState(this, _context);
    }
}