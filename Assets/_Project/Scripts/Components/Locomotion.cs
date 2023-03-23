using System;
using _Project.Scripts.DataClasses;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.StateMachine.LocomotionStates;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class Locomotion : MonoBehaviour {
        [SerializeField]
        private LocomotionStats _stats;

        private LocomotionStateFactory _states;

        public AbstractBaseState CurrentState { get; set; }

        public Vector3 WorldDirection { get; set; }

        public Vector3 AppliedMovement { get; set; }

        public Vector3 RelativeDirection { get; set; }

        public float CurrentMovementSpeed { get; set; }

        public LocomotionStats Stats => _stats;
        public Rigidbody Rb { get; private set; }

        public bool IsGrounded { get; set; }

        public bool IsMoving { get; set; }

        public bool IsJumping { get; set; }

        public bool IsSprinting { get; set; }

        public string state;

        public void SetUp() {
            Rb = GetComponent<Rigidbody>();
            _states = new LocomotionStateFactory(this);
            CurrentState = _states.Grounded();
            CurrentState.EnterState();
        }
        private void FixedUpdate() {
            IsGrounded = Physics.Raycast(transform.position,-Vector3.up, Stats.height, Stats.groundLayer);
        }

        public void FixedTick() {
            CurrentState.UpdateStates();
            state = CurrentState.StateName;
        }

        public void HandleMovement(float delta, Vector3 relativeDirection, Transform relativeTransform) {
            var calculatedDirection = relativeDirection.z * relativeTransform.forward +
                relativeDirection.x * relativeTransform.right;
            calculatedDirection.y = 0;
            RelativeDirection = relativeDirection;
            WorldDirection = calculatedDirection.normalized;
            HandleMovement(delta);
        }
        
        private void HandleMovement(float delta) {
            var moveDirection = new Vector3(WorldDirection.x, 0, WorldDirection.z);
            var strafeMult = RelativeDirection == Vector3.right || RelativeDirection == Vector3.left ? _stats.strafeMultSpeed : 1;
            var backwardsMult = RelativeDirection == Vector3.back ? _stats.backwardsMultSpeed : 1;
            AppliedMovement = moveDirection * (CurrentMovementSpeed * strafeMult * backwardsMult) * delta;
            AppliedMovement = new Vector3(AppliedMovement.x, 0, AppliedMovement.z);
        }
    }
}