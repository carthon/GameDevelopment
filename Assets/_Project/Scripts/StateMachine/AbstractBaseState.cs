using _Project.Scripts.Components;

public abstract class AbstractBaseState {
    protected AbstractBaseState _currentSubState;
    protected AbstractBaseState _currentSuperState;
    protected bool _isRootState = false;
    protected LocomotionStateFactory factory;
    protected Locomotion locomotion;
    public abstract string StateName { get; }

    protected AbstractBaseState(LocomotionStateFactory factory, Locomotion locomotion) {
        this.factory = factory;
        this.locomotion = locomotion;
    }

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
    public abstract void InitializeSubState();
    public abstract void UpdateState();

    protected void SwitchState(AbstractBaseState newState) {
        ExitState();
        newState.EnterState();
        if (_isRootState)
            locomotion.CurrentState = newState;
        else if (_currentSuperState != null) _currentSuperState.SetSubState(newState);
    }

    protected void SetSubState(AbstractBaseState newSubState) {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }
    protected void SetSuperState(AbstractBaseState newSuperState) {
        _currentSuperState = newSuperState;
    }
    public void UpdateStates() {
        UpdateState();
        if (_currentSubState != null)
            _currentSubState.UpdateStates();
    }
    private void ExitStates() {
        ExitState();
        if (_currentSubState != null)
            _currentSubState.ExitStates();
    }
}