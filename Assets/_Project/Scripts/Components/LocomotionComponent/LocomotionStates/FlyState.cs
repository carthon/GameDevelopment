using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Components.LocomotionComponent.LocomotionStates {
    public class FlyState : AbstractBaseState{
        public FlyState(LocomotionStateFactory factory, Locomotion locomotion) : base(factory, locomotion) { EnterState(); }
        public sealed override void EnterState() {
            _isRootState = true;
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
        private void HandleFlight() {
            Vector3 gravityUp = (locomotion.Rb.position - locomotion.GravityCenter).normalized;
            Vector3 localUp = MathUtility.LocalToWorldVector(locomotion.Rb.rotation, Vector3.up);
            locomotion.Rb.rotation = Quaternion.FromToRotation(localUp, gravityUp) * locomotion.Rb.rotation;
            locomotion.Rb.velocity = locomotion.AppliedMovement;
        }
        public override string StateName { get => "FlyState"; }
    }
}