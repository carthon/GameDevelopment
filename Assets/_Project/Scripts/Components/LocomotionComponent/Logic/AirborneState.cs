using _Project.Scripts.DataClasses;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Components.LocomotionComponent.Logic {
    public class AirborneState : ILocomotionState {
        private LocomotionStats _stats;
        public void Enter(ref LocomotionStateMessage state,
            in LocomotionInputMessage input,
            in LocomotionStats stats) {
            state.mode = LocomotionMode.Airborne;
            _stats = stats;
            // state.Velocity ya lleva componente vertical heredada
        }

        public void Tick(ref LocomotionStateMessage state,
            in LocomotionInputMessage input,
            in LocomotionStats stats,
            float deltaTime) {
            bool wantDoubleJump = LocomotionUtils.IsDoubleJumping(input.actions);
            bool wantMove = LocomotionUtils.IsMoving(input.actions);

            // 1) Componente horizontal en el aire
            Vector3 planarDir = new Vector3(input.moveInput.x, 0f, input.moveInput.y).normalized;
            Vector3 desiredHoriz = planarDir * (wantMove ? stats.inAirSpeed : 0f);

            // 2) Componente vertical actual
            Vector3 verticalComp = Vector3.Project(state.velocity, Vector3.up);

            state.velocity = desiredHoriz + verticalComp;

            // 3) Aplicar gravedad
            Vector3 down = - (state.position - state.planetData.Center); // en planeta: -(Position - Center).normalized
            state.velocity += down * (state.planetData.Gravity * deltaTime);

            // 4) Actualizar posición
            state.position += state.velocity * deltaTime;
        }

        public ILocomotionState CheckTransition(in LocomotionStateMessage state,
            in LocomotionInputMessage input) {
            // Si aterrizamos, volvemos a Grounded
            if (state.isGrounded)
                return new GroundedState();

            // Si pulsamos doble salto y podemos volar
            if (LocomotionUtils.IsDoubleJumping(input.actions) && _stats.canFly)
                return new FlyState();

            return null;
        }
    }
}