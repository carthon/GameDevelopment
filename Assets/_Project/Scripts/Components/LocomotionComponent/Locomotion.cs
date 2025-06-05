using System;
using _Project.Scripts.Components.LocomotionComponent.LocomotionStates;
using _Project.Scripts.DataClasses;
using _Project.Scripts.Network.MessageDataStructures;
using _Project.Scripts.Utils;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Components.LocomotionComponent {
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
        public Transform Trans { get; private set; }
        public CapsuleCollider CapsCollider { get; private set; }

        public bool IsGrounded { get; set; }
        private bool _ignoreGround;
        public bool IgnoreGround
        {
            get => _ignoreGround;
            set
            {
                _ignoreGround = value;
                CapsCollider.isTrigger = value;
            }
        }
        [FormerlySerializedAs("onGroundForward")] [HideInInspector]
        public Vector3 lookForwardDirection;
        [FormerlySerializedAs("onGroundRight")] [HideInInspector]
        public Vector3 lookRightDirection;
        public Vector3 GravityCenter { get; set; }
        public float Gravity { get; set; }
        public RaycastHit groundRayCast;
        private RaycastHit hitInfo;

        public ulong actions;
        
        public float Delta { get; set; }

        public string state;

        public void SetUp(Vector3 gravityCenter, float gravity) {
            Rb = GetComponent<Rigidbody>();
            Trans = transform;
            Gravity = gravity;
            actions = 0;
            GravityCenter = gravityCenter;
            CapsCollider = GetComponent<CapsuleCollider>();
            _states = new LocomotionStateFactory(this);
            CurrentState = _states.Grounded();
            CurrentState.EnterState();
        }

        public void FixedTick() {
            Vector3 centre = Rb.position;
            Vector3 upDir = (centre - GravityCenter).normalized;
            Vector3 castOrigin = centre + upDir * (-CapsCollider.height / 2f + CapsCollider.radius);
            if(!_ignoreGround)
                IsGrounded = Physics.Raycast(castOrigin, -upDir, out groundRayCast, Stats.height, Stats.groundLayer);
            else
                IsGrounded = !_ignoreGround;
            CurrentState.UpdateStates(); //Se actualiza tambien la velocity del rigidbody
            state = CurrentState.StateName;
        }

        public void HandleMovement(InputMessageStruct input, Transform relativeTransform) {
            actions = input.actions;
            CurrentState.ComputeMovement(input, relativeTransform);
        }
        
        private void OnDrawGizmos() {
            if (Rb != null) {
                Vector3 centre = Rb.position;
                Vector3 upDir = (centre - GravityCenter).normalized;
                Vector3 castOrigin = centre + upDir * (-CapsCollider.height / 2f + CapsCollider.radius);
                float groundedRayRadius = CapsCollider.radius * _stats.groundedRayRadius;

                float groundedRayDst = CapsCollider.radius - groundedRayRadius + _stats.height;
                Gizmos.color = (IsGrounded) ? Color.green : Color.red;
                Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);
                Gizmos.DrawSphere(castOrigin, groundedRayRadius);


                Vector3 collisionSphereTip = castOrigin - upDir * (groundedRayRadius + groundedRayDst);
                Gizmos.DrawSphere(collisionSphereTip + upDir * groundedRayRadius, groundedRayRadius);
                Gizmos.color = (IsGrounded) ? Color.green : Color.red;
                Gizmos.DrawRay(castOrigin - upDir * groundedRayRadius, -upDir * groundedRayDst);
            }
        }
    }
}