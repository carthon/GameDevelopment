using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates {
    public class GroundedState : AbstractBaseState {
        private GameObject _lastHit;
        private Vector3 _lastHitPosition;
        
        public GroundedState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) {
            _isRootState = true;
        }
        public override void EnterState() {
            InitializeSubState();
        }
        public override void ExitState() {
        }
        public override void CheckSwitchStates() {
            if (!locomotion.IsGrounded || LocomotionUtils.IsJumping(locomotion.actions))
                SwitchState(factory.Airborne());
        }
        public sealed override void InitializeSubState() {
            if (!LocomotionUtils.IsMoving(locomotion.actions) && !LocomotionUtils.IsSprinting(locomotion.actions))
                SetSubState(factory.Idle());
            else if (LocomotionUtils.IsMoving(locomotion.actions) && !LocomotionUtils.IsSprinting(locomotion.actions))
                SetSubState(factory.Run());
            else if (LocomotionUtils.IsCrouching(locomotion.actions))
                SetSubState(factory.Crouch());
            else
                SetSubState(factory.Sprint());
        }
        public override void UpdateState() {
            CheckSwitchStates();
            HandleGrounded();
        }

        public override void ComputeMovement(InputMessageStruct inputMessage, Transform camera) {
            Vector3 normalFromPlanet = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            Quaternion headRotation = camera.rotation;
            Quaternion groundRotation = Quaternion.FromToRotation(camera.up, normalFromPlanet) * headRotation;
            locomotion.lookForwardDirection = MathUtility.LocalToWorldVector(groundRotation, Vector3.forward);
            locomotion.lookRightDirection = MathUtility.LocalToWorldVector(groundRotation, Vector3.right);
            var relativeMoveDirection = inputMessage.moveInput.z * locomotion.lookForwardDirection +
                inputMessage.moveInput.x * locomotion.lookRightDirection;
            relativeMoveDirection = Vector3.ProjectOnPlane(relativeMoveDirection, normalFromPlanet);
            
            locomotion.RelativeDirection = inputMessage.moveInput;
            locomotion.WorldDirection = relativeMoveDirection.sqrMagnitude > 0.0001f ? relativeMoveDirection.normalized : Vector3.zero;

            Vector3 desiredGlobalVelocity = locomotion.WorldDirection * (locomotion.CurrentMovementSpeed * locomotion.Delta);
            
            locomotion.AppliedMovement = desiredGlobalVelocity;
        }

        public void HandleGrounded() {
            var stats = locomotion.Stats;
            Vector3 gravityUp = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            // Align body's up axis with the centre of planet
            Vector3 localUp = MathUtility.LocalToWorldVector(locomotion.Rb.rotation, Vector3.up);
            locomotion.Rb.rotation = Quaternion.FromToRotation(localUp, gravityUp) * locomotion.Rb.rotation;

            if (locomotion.IsGrounded && !LocomotionUtils.IsJumping(locomotion.actions)) {
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