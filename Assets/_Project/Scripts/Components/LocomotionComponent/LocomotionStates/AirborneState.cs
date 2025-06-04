using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates {
    public class AirborneState : AbstractBaseState {
        public AirborneState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) {
            _isRootState = true;
        }
        public override void EnterState() {
            InitializeSubState();
        }
        public override void ExitState() {}
        public override void CheckSwitchStates() {
            if (locomotion.IsDoubleJumping) SwitchState(factory.Fly());
            if (locomotion.IsGrounded) SwitchState(factory.Grounded());
        }
        public sealed override void InitializeSubState() {
            if (!locomotion.IsGrounded) SetSubState(factory.Fall());
            else if (locomotion.IsJumping) SetSubState(factory.JumpUp());
        }
        public override void UpdateState() {
            CheckSwitchStates();
            ApplyInAirSpeed();
            HandleGravity();
        }
        private void ApplyInAirSpeed() {
            // 1) Calcula la velocidad horizontal deseada
            Vector3 horizontalDir = locomotion.WorldDirection; 
            float delta = locomotion.Delta; // asumiendo que guardas el delta fijo en locomotion

            Vector3 desiredHorizontalVel = horizontalDir * (locomotion.CurrentMovementSpeed * delta);

            // 2) Conserva la componente vertical actual
            Vector3 currentVel = locomotion.Rb.velocity;
            Vector3 gravityUp = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            float verticalVel = Vector3.Dot(currentVel, gravityUp);

            // 3) Combina
            Vector3 newVel = desiredHorizontalVel 
                + gravityUp * verticalVel;
            locomotion.Rb.velocity = newVel;
        }

        private void HandleGravity() {
            Vector3 gravityUp = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            Vector3 gravityForce = -gravityUp * locomotion.Gravity; // La aceleración de la gravedad debe ser negativa para que 'tire' del jugador hacia el centro del planeta
            locomotion.Rb.velocity += gravityForce * locomotion.Delta;
        }
        public override string StateName => $"AirborneState -> {_currentSubState.StateName}";
    }
}