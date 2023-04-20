using _Project.Scripts.Components;
using UnityEngine;

namespace _Project.Scripts.StateMachine.LocomotionStates {
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
        private void HandleJump() {
            var stats = locomotion.Stats;
            Vector3 velocity = locomotion.Rb.velocity;
            velocity.y = Mathf.Sqrt(stats.jumpStrength * -2f * stats.fallingSpeed);
            locomotion.Rb.velocity = velocity;
        }

        private void HandleGravity() {
            var stats = locomotion.Stats;
            var appliedVelocity = locomotion.WorldDirection.normalized;
            Vector3 velocity = locomotion.Rb.velocity;
            Vector3 gravity = velocity + Vector3.up * stats.fallingSpeed * Time.fixedDeltaTime;
            locomotion.Rb.velocity = gravity;
            if (locomotion.Rb.velocity.y > 0) {
                appliedVelocity += lastDirection;
                if (appliedVelocity.magnitude < lastDirection.magnitude)
                    lastDirection = appliedVelocity;
                locomotion.Rb.velocity = new Vector3(appliedVelocity.x, locomotion.Rb.velocity.y, appliedVelocity.z);
            }
        }
        public override string StateName => "JumpState";
    }
}