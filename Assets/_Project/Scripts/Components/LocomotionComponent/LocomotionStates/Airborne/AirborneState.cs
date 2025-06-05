using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates.Airborne {
    public class AirborneState : AbstractBaseState {
        public AirborneState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) {
            _isRootState = true;
        }
        public override void EnterState() {
            InitializeSubState();
        }
        public override void ExitState() {}
        public override void CheckSwitchStates() {
            if (LocomotionUtils.IsDoubleJumping(locomotion.actions)) SwitchState(factory.Fly());
            if (locomotion.IsGrounded) SwitchState(factory.Grounded());
        }
        public sealed override void InitializeSubState() {
            if (!locomotion.IsGrounded) SetSubState(factory.Fall());
            else if (LocomotionUtils.IsJumping(locomotion.actions)) SetSubState(factory.JumpUp());
        }
        public override void UpdateState() {
            CheckSwitchStates();
            HandleGravity();
        }
        public override void ComputeMovement(InputMessageStruct inputMessage, Transform camera) {
            Vector3 normalFromPlanet = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            Quaternion headRotation = camera.rotation;
            Quaternion groundRotation = Quaternion.FromToRotation(camera.up, normalFromPlanet) * headRotation;
            locomotion.lookForwardDirection = MathUtility.LocalToWorldVector(headRotation, Vector3.forward);
            locomotion.lookRightDirection = MathUtility.LocalToWorldVector(groundRotation, Vector3.right);
            var relativeMoveDirection = inputMessage.moveInput.z * locomotion.lookForwardDirection +
                inputMessage.moveInput.x * locomotion.lookRightDirection;
            relativeMoveDirection = Vector3.ProjectOnPlane(relativeMoveDirection, normalFromPlanet);
            
            locomotion.RelativeDirection = inputMessage.moveInput;
            locomotion.WorldDirection = relativeMoveDirection.sqrMagnitude > 0.0001f ? relativeMoveDirection.normalized : Vector3.zero;

            Vector3 desiredGlobalVelocity = locomotion.WorldDirection * (locomotion.CurrentMovementSpeed * locomotion.Delta);
            
            locomotion.AppliedMovement = desiredGlobalVelocity;
        }

        private void HandleGravity() {
            Vector3 gravityUp = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            Vector3 gravityForce = -gravityUp * locomotion.Gravity; // La aceleración de la gravedad debe ser negativa para que 'tire' del jugador hacia el centro del planeta
            locomotion.Rb.velocity += gravityForce * locomotion.Delta;
        }
        public override string StateName => $"AirborneState -> {_currentSubState.StateName}";
    }
}