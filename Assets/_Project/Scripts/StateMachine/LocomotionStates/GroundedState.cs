using _Project.Scripts.Components;
using UnityEngine;

namespace _Project.Scripts.StateMachine.LocomotionStates {
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
            else
                SetSubState(factory.Sprint());
        }
        public override void UpdateState() {
            CheckSwitchStates();
            HandleGrounded();
        }

        public void HandleGrounded() {
            var stats = locomotion.Stats;
            RaycastHit hit;
            var transformPosition = locomotion.transform.position;
            var origin = transformPosition;
            var moveDirection = locomotion.WorldDirection;
            var dir = moveDirection;
            // dir.Normalize();
            // origin += dir;

            Debug.DrawRay(origin, -Vector3.up * stats.height, Color.red, 0.1f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, stats.height, locomotion.Stats.groundLayer)) {
                //Vector3 groundPoint = CalculateMovableGround(hit);
                var groundPoint = locomotion.transform.position;
                groundPoint.y = hit.point.y;
                locomotion.transform.position = groundPoint + Vector3.up * stats.height;
                if (locomotion.transform.parent != hit.transform && hit.collider.CompareTag("Grid")) {
                    locomotion.transform.SetParent(hit.transform);
                }
            }
            else {
                _lastHitPosition = Vector3.zero;
                locomotion.transform.SetParent(null);
            }
            locomotion.Rb.velocity = locomotion.AppliedMovement;
        }
        private Vector3 CalculateMovableGround(RaycastHit hitObj) {
            var groundPoint = locomotion.transform.position;
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