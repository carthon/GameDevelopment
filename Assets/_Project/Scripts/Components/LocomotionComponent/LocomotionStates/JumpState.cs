using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates {
    public class JumpState : AbstractBaseState {
        private bool hasJumped;
        private Vector3 lastDirection;
        private float _lastJumpTime;
        private float _jumpBufferTime = 1f;
        private bool _finishedJump = true;
        public JumpState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) {
            _isRootState = true;
        }
        public override void EnterState() {
            lastDirection = locomotion.AppliedMovement;
            _finishedJump = !hasJumped;
        }
        public override void ExitState() {
        }
        public override void CheckSwitchStates() {
            if (locomotion.IsGrounded && !hasJumped && _finishedJump) SwitchState(factory.Grounded());
            if (locomotion.IsDoubleJumping) SwitchState(factory.Fly());
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
            _lastJumpTime = Time.time;
            _finishedJump = false;
            Vector3 gravityUp = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            var stats = locomotion.Stats;
            locomotion.Rb.AddForce(gravityUp * stats.jumpStrength, ForceMode.Impulse);
        }

        private void HandleGravity() {
            Vector3 gravityUp = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            Vector3 gravityForce = -gravityUp * locomotion.Gravity; // La aceleración de la gravedad debe ser negativa para que 'tire' del jugador hacia el centro del planeta
            locomotion.Rb.AddForce(gravityForce, ForceMode.Acceleration); // Aplicamos la fuerza de gravedad como una aceleración
            if (!_finishedJump && Time.time - _lastJumpTime < _jumpBufferTime) {
                _finishedJump = true;
            }
            /*
            if (locomotion.Rb.velocity.y > 0) {
                appliedVelocity += lastDirection;
                if (appliedVelocity.magnitude < lastDirection.magnitude)
                    lastDirection = appliedVelocity;
                locomotion.Rb.velocity = new Vector3(appliedVelocity.x, locomotion.Rb.velocity.y, appliedVelocity.z);
            }*/
        }
        public override string StateName => "JumpState";
    }
}