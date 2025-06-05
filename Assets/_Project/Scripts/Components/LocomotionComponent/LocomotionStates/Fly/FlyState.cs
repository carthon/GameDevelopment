using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates.Fly {
    public class FlyState : AbstractBaseState{
        public FlyState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) { 
            _isRootState = true;
        }
        public sealed override void EnterState() {
            locomotion.CurrentMovementSpeed = locomotion.Stats.flySpeed;
            locomotion.IgnoreGround = true;
        }
        public override void ExitState() {
            locomotion.IgnoreGround = false;
        }
        public override void CheckSwitchStates() {
            if (locomotion.IsGrounded)
                SwitchState(factory.Grounded());
        }
        public override void InitializeSubState() {
        }
        public override void UpdateState() {
            CheckSwitchStates();
            HandleFlight();
        }
        public override void ComputeMovement(InputMessageStruct inputMessage, Transform camera) {
            Vector3 normalFromPlanet = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            Quaternion headRotation = camera.rotation;
            Quaternion groundRotation = Quaternion.FromToRotation(camera.up, normalFromPlanet) * headRotation;
            locomotion.lookForwardDirection = MathUtility.LocalToWorldVector(headRotation, Vector3.forward);
            locomotion.lookRightDirection = MathUtility.LocalToWorldVector(groundRotation, Vector3.right);
            Vector3 lookUpDirection = MathUtility.LocalToWorldVector(groundRotation, Vector3.up);
            var relativeMoveDirection = inputMessage.moveInput.z * locomotion.lookForwardDirection +
                inputMessage.moveInput.x * locomotion.lookRightDirection + (LocomotionUtils.IsJumping(inputMessage.actions) ? 1 : 0) * lookUpDirection;
            
            locomotion.RelativeDirection = inputMessage.moveInput;
            locomotion.WorldDirection = relativeMoveDirection.sqrMagnitude > 0.0001f ? relativeMoveDirection.normalized : Vector3.zero;

            Vector3 desiredGlobalVelocity = locomotion.WorldDirection * (locomotion.CurrentMovementSpeed * locomotion.Delta);
            
            locomotion.AppliedMovement = desiredGlobalVelocity;
        }
        private void HandleFlight() {
            Vector3 gravityUp = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            Vector3 localUp = MathUtility.LocalToWorldVector(locomotion.Rb.rotation, Vector3.up);
            locomotion.Rb.rotation = Quaternion.FromToRotation(localUp, gravityUp) * locomotion.Rb.rotation;
        }
        public override string StateName { get => "FlyState"; }
    }
}