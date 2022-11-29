using System;
using Scripts.DataClasses;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class Locomotion : MonoBehaviour {
        [SerializeField]
        private LocomotionStats _stats;

        private LocomotionStateFactory _states;

        public AbstractBaseState CurrentState { get; set; }

        public Vector3 TargetPosition { get; set; }

        public Vector3 AppliedMovement { get; set; }

        public Vector3 RelativeDirection { get; set; }

        public float CurrentMovementSpeed { get; set; }

        public LocomotionStats Stats => _stats;
        public Rigidbody Rb { get; private set; }

        public bool IsGrounded { get; set; }

        public bool IsMoving { get; set; }

        public bool IsJumping { get; set; }

        public bool IsSprinting { get; set; }

        public void SetUp() {
            Rb = GetComponent<Rigidbody>();
            _states = new LocomotionStateFactory(this);
            CurrentState = _states.Grounded();
            CurrentState.EnterState();
        }

        public void FixedTick(float delta) {
            HandleMovement(delta);
            CurrentState.UpdateStates();
            IsGrounded = Physics.Raycast(transform.position,-Vector3.up, Stats.height, Stats.groundLayer);
        }

        public void HandleRotation() {

        }
        private void HandleMovement(float delta) {
            var moveDirection = new Vector3(TargetPosition.x, 0, TargetPosition.z);
            var strafeMult = RelativeDirection == Vector3.right || RelativeDirection == Vector3.left ? _stats.strafeMultSpeed : 1;
            var backwardsMult = RelativeDirection == Vector3.back ? _stats.backwardsMultSpeed : 1;
            AppliedMovement = moveDirection * (CurrentMovementSpeed * strafeMult * backwardsMult);// * delta;
            AppliedMovement = new Vector3(AppliedMovement.x, 0, AppliedMovement.z);
        }
    }
}