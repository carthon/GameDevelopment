using _Project.Scripts.DataClasses;
using UnityEngine;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates {
    public class JumpUpState : AbstractBaseState {
        private bool _impulseApplied;
        public JumpUpState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) {
        }
        public override string StateName => "JumpUpState";
        public override void EnterState() {
            _impulseApplied = false;
            ApplyJumpImpulse();
        }
        public override void ExitState() {}
        public override void CheckSwitchStates() {
            if (Vector3.Dot(locomotion.Rb.velocity, (locomotion.Rb.position - locomotion.GravityCenter).normalized) <= 0f)
                SwitchState(factory.Fall());
        }
        public override void InitializeSubState() {}
        public override void UpdateState() {
            ApplyJumpImpulse();
            CheckSwitchStates();
        }
        private void ApplyJumpImpulse() {
            if (_impulseApplied) return;
            Vector3 gravityUp = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            float strength = locomotion.Stats.jumpStrength;   // impulso inicial
            locomotion.Rb.AddForce(gravityUp * strength, ForceMode.VelocityChange);
            _impulseApplied = true;
        }
    }
}