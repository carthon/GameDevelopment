using _Project.Scripts.DataClasses;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Components.LocomotionComponent.Logic {
    public class GroundedState : ILocomotionState {
        private LocomotionStats _stats;
        public void Enter(ref LocomotionStateMessage state,
            in LocomotionInputMessage input,
            in LocomotionStats stats) {
            // Al entrar en sistema grounded:
            state.mode = LocomotionMode.Grounded;
            state.velocity = Vector3.zero;
            _stats = stats;
        }
        public void Find(){
            Vector3 normalFromPlanet = (Rb.position - GravityCenter).normalized;
            Quaternion groundRotation = Quaternion.FromToRotation(relativeTransform.up, normalFromPlanet) * relativeTransform.rotation;
            lookForwardDirection = groundRotation * Vector3.forward;
            lookRightDirection = groundRotation * Vector3.right;
            var relativeMoveDirection = moveInputDirection.z * lookForwardDirection +
                moveInputDirection.x * lookRightDirection;
            relativeMoveDirection = Vector3.ProjectOnPlane(relativeMoveDirection, normalFromPlanet).normalized;
            
            RelativeDirection = moveInputDirection.normalized;
            WorldDirection = relativeMoveDirection.normalized;

            Vector3 desiredGlobalVelocity = relativeMoveDirection * (CurrentMovementSpeed * Delta);
            
            AppliedMovement = desiredGlobalVelocity;
            
        }

        public void Tick(ref LocomotionStateMessage state,
            in LocomotionInputMessage input,
            in LocomotionStats stats,
            float deltaTime) {
            // 1) EXTRAER FLAGS desde input.Actions
            bool wantJump = LocomotionUtils.IsJumping(input.actions);
            bool wantCrouch = LocomotionUtils.IsCrouching(input.actions);
            bool wantSprint = LocomotionUtils.IsSprinting(input.actions);
            bool wantMoving = LocomotionUtils.IsMoving(input.actions);
            // 3) ELEGIR VELOCIDAD SEGÚN STATUSES
            float speed;
            if (wantCrouch)
                speed = stats.crouchSpeed;
            else if (wantSprint && wantMoving)
                speed = stats.sprintSpeed;
            else if (wantMoving)
                speed = stats.runSpeed;
            else
                speed = 0f;
            
            Vector3 normalFromPlanet = (state.position - state.planetData.Center).normalized;
            Quaternion groundRotation = Quaternion.FromToRotation(state.headPivot.up, normalFromPlanet) * state.headRotation;
            Vector3 lookForwardDirection = groundRotation * Vector3.forward;
            Vector3 lookRightDirection = groundRotation * Vector3.right;
            var relativeMoveDirection = input.moveInput.z * lookForwardDirection +
                input.moveInput.x * lookRightDirection;
            relativeMoveDirection = Vector3.ProjectOnPlane(relativeMoveDirection, normalFromPlanet).normalized;
            
            state.localDirection = input.moveInput.normalized;

            Vector3 desiredGlobalVelocity = relativeMoveDirection * speed;
            
            state.velocity = desiredGlobalVelocity;

            // 5) SALTO
            if (wantJump) {
                // Aplicamos impulso vertical
                Vector3 gravityUp =  (state.position - state.planetData.Center).normalized; // o (state.Position - stats.GravityCenter).normalized si es planeta esférico
                state.velocity += gravityUp * stats.jumpStrength;
                state.mode = LocomotionMode.Airborne;
            }

            // 6) ACTUALIZAR POSICIÓN
            state.position += state.velocity * deltaTime;
        }

        public ILocomotionState CheckTransition(in LocomotionStateMessage state,
            in LocomotionInputMessage input) {
            // 1) Si dejamos el suelo (IsGrounded == false), vamos a Airborne
            if (!state.isGrounded)
                return new AirborneState();

            // 2) Si estuviera haciendo doble salto (y pudiera volar), ir a Fly
            if (LocomotionUtils.IsDoubleJumping(input.actions) && _stats.canFly)
                return new FlyState();

            // 3) En otro caso, seguimos en Grounded
            return null;
        }
    }
}