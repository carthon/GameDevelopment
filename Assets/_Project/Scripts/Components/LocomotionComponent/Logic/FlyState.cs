using _Project.Scripts.DataClasses;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Components.LocomotionComponent.Logic {
    public class FlyState : ILocomotionState
    {
        private LocomotionStats _stats;
        public void Enter(ref LocomotionStateMessage state,
            in LocomotionInputMessage input,
            in LocomotionStats stats)
        {
            state.mode = LocomotionMode.Fly;
            _stats = stats;
            // En el bridge se activará IgnoreGround = true
        }

        public void Tick(ref LocomotionStateMessage state,
            in LocomotionInputMessage input,
            in LocomotionStats stats,
            float deltaTime)
        {
            bool wantMove   = LocomotionUtils.IsMoving(input.actions);
            bool wantSprint = LocomotionUtils.IsSprinting(input.actions);

            // 1) Determinar ejes de vuelo a partir de HeadRotation
            Vector3 forwardDir = (state.headRotation * Vector3.forward).normalized;
            Vector3 rightDir   = (state.headRotation * Vector3.right).normalized;

            // 2) Formar dirección deseada
            Vector3 desiredDir = Vector3.zero;
            desiredDir += input.moveInput.y * forwardDir;
            desiredDir += input.moveInput.x * rightDir;
            desiredDir = desiredDir.normalized;

            // 3) Velocidad de vuelo
            float flySpeed = stats.flySpeed;
            if (wantSprint)
                flySpeed *= 1.2f;
            Vector3 desiredVel = wantMove ? desiredDir * flySpeed : Vector3.zero;

            // 4) Aplicar
            state.velocity = desiredVel;
            state.position += state.velocity * deltaTime;
        }

        public ILocomotionState CheckTransition(in LocomotionStateMessage state,
            in LocomotionInputMessage input)
        {
            if (state.isGrounded)
                return new GroundedState();

            return null;
        }
    }
}