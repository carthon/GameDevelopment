using _Project.Scripts.Components;
using UnityEngine;

public class JumpState : AbstractBaseState {
    private bool hasJumped;
    private Vector3 lastDirection;
    public JumpState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) {
        _isRootState = true;
    }
    public override void EnterState() {
        lastDirection = locomotion.AppliedMovement;
    }
    public override void ExitState() {
    }
    public override void CheckSwitchStates() {
        if (locomotion.IsGrounded) SwitchState(factory.Grounded());
    }
    public override void InitializeSubState() {
    }
    public override void UpdateState() {
        CheckSwitchStates();
        if (hasJumped) {
            HandleJump();
            hasJumped = false;
        }
        HandleGravity();
    }
    public void SetJumped(bool hasJumped) {
        this.hasJumped = hasJumped;
    }
    public void HandleJump() {
        var stats = locomotion.Stats;
        locomotion.Rb.AddForce(Vector3.up * stats.jumpStrength);
    }

    public void HandleGravity() {
        var stats = locomotion.Stats;
        var appliedVelocity = locomotion.TargetPosition.normalized * stats.inAirSpeed;
        locomotion.Rb.AddForce(-Vector3.up * stats.fallingSpeed * Time.fixedDeltaTime);
        //if (locomotion.Rb.velocity.y > 0) {
        appliedVelocity += lastDirection;
        if (appliedVelocity.magnitude < lastDirection.magnitude)
            lastDirection = appliedVelocity;
        locomotion.Rb.velocity = new Vector3(appliedVelocity.x, locomotion.Rb.velocity.y, appliedVelocity.z);
        //}
    }
    public override string StateName => "JumpState";
}