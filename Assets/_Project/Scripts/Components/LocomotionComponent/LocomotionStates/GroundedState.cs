using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates {
    public class GroundedState : AbstractBaseState {
        private GameObject _lastHit;
        private Vector3 _lastHitPosition;
        
        public GroundedState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) {
            _isRootState = true;
            InitializeSubState();
        }
        public override void EnterState() {
        }
        public override void ExitState() {
        }
        public override void CheckSwitchStates() {
            if (!locomotion.IsGrounded)
                SwitchState(factory.Fall());
            else if (locomotion.IsJumping) SwitchState(factory.Jump());
        }
        public sealed override void InitializeSubState() {
            if (!locomotion.IsMoving && !locomotion.IsSprinting)
                SetSubState(factory.Idle());
            else if (locomotion.IsMoving && !locomotion.IsSprinting)
                SetSubState(factory.Run());
            else if (locomotion.IsCrouching)
                SetSubState(factory.Crouch());
            else
                SetSubState(factory.Sprint());
        }
        public override void UpdateState() {
            CheckSwitchStates();
            HandleGrounded();
        }

        public void HandleGrounded() {
            var stats = locomotion.Stats;
            Vector3 gravityUp = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            // Align body's up axis with the centre of planet
            Vector3 localUp = MathUtility.LocalToWorldVector(locomotion.Rb.rotation, Vector3.up);
            locomotion.Rb.rotation = Quaternion.FromToRotation(localUp, gravityUp) * locomotion.Rb.rotation;

            if (locomotion.IsGrounded && !locomotion.IsJumping) {
                var groundPoint = locomotion.groundRayCast.point;
                locomotion.Trans.position = groundPoint + gravityUp * stats.height;
            }
            locomotion.Rb.velocity = locomotion.AppliedMovement;
        }
        private Vector3 CalculateMovableGround(RaycastHit hitObj) {
            var groundPoint = locomotion.Trans.position;
            groundPoint.y = hitObj.point.y;
            if (_lastHit != hitObj.transform.gameObject) {
                _lastHitPosition = hitObj.transform.position;
                _lastHit = hitObj.transform.gameObject;
            }
            else if (_lastHitPosition != hitObj.transform.position) {
                var actualHitPosition = hitObj.transform.position;
                var difObjectPosition = actualHitPosition - _lastHitPosition;
                Debug.Log(difObjectPosition.magnitude);
                if (difObjectPosition.y < 0)
                    difObjectPosition.y = 0;
                _lastHitPosition = actualHitPosition;
                if (difObjectPosition.magnitude < 4)
                    groundPoint += difObjectPosition;
            }
            return groundPoint;
        }
        public override string StateName { get => $"GroundedState -> {_currentSubState.StateName}"; }
    }
}