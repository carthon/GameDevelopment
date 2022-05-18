using System;
using System.Runtime;
using _Project.Scripts.Handlers;
using Cinemachine.Editor;
using StarterAssets;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class Locomotion : MonoBehaviour {
        private InputHandler inputHandler;

        [HideInInspector]
        public Transform myTransform;
        [HideInInspector]
        public AnimatorHandler animatorHandler;

        public new Rigidbody rigidbody;
        public GameObject normalCamera;


        public bool isSprinting;
        public bool isInteracting;
        public bool isRolling;
        public bool isGrounded;

        [Header("Ground & Air detection Stats")]
        [SerializeField] private float groundDetectionRayStartPoint = .5f;
        [SerializeField] private float minimumDistanceNeededToBeginFall = 1f;
        [SerializeField] private float groundDirectionRayDistance = .2f;
        [SerializeField] private float groundDistance = .2f;
        [SerializeField] private LayerMask ignoreForGroundCheck;

        [Header("Stats")]
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float inAirMovementControl = 10f;
        [SerializeField] private float fallingSpeed = 45f;
        private float moveAmount;
        [SerializeField] private Vector3 moveDirection;

        private void Start() {
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            myTransform = transform;
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            animatorHandler.Initialize();
            isGrounded = true;
            //ignoreForGroundCheck = ~(1 << 8 | 1 << 11);
        }

        #region Movement

        private Vector3 normalVector;
        private Vector3 targetPosition;

        private void HandleRotation(float delta, Vector3 targetDir) {
            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
                targetDir = myTransform.forward;

            float rs = rotationSpeed;
            
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

            myTransform.rotation = targetRotation;
        }
        #endregion
        
        private static readonly int IsInteracting = Animator.StringToHash("isInteracting");
        public void HandleMovement(float delta, Vector3 moveDirection) {
            this.moveDirection = moveDirection;
            moveAmount = Mathf.Clamp01(Mathf.Abs(moveDirection.x) + Mathf.Abs(moveDirection.z));
            if (isRolling || !isGrounded)
                return;
            moveDirection.Normalize();
            moveDirection.y = 0;

            float speed = movementSpeed;
            if (isSprinting) {
                speed = sprintSpeed;
            }
            moveDirection *= speed;

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
            rigidbody.velocity = projectedVelocity;

            animatorHandler.UpdateAnimatorValues(moveAmount, 0, isSprinting);
            
            if (animatorHandler.canRotate)
                HandleRotation(delta, moveDirection);
        }

        public void HandleRollingAndSprinting(float delta, Vector3 moveDirection) {
            isInteracting = animatorHandler.animator.GetBool(IsInteracting);
            if (isInteracting)
                return;

            if (isRolling) {
                if (moveAmount > 0) {
                    animatorHandler.PlayTargetAnimation("Rolling", true, 1);
                    moveDirection.y = 0;
                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;
                }
                else {
                    //animatorHandler.PlayTargetAnimation("Backstep", true);
                }
            }
        }

        public void HandleFalling(float delta, Vector3 moveDirection) {
            RaycastHit hit;
            Vector3 origin = myTransform.position;
            origin.y += groundDetectionRayStartPoint;
            moveDirection.y = 0;

            if (!isGrounded) {
                rigidbody.AddForce(-Vector3.up * fallingSpeed);
            }
            Vector3 dir = moveDirection;
            dir.Normalize();
            origin += dir * groundDirectionRayDistance;

            targetPosition = myTransform.position;
            
            Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeededToBeginFall, Color.red, 0.1f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, ignoreForGroundCheck)) {
                normalVector = hit.normal;
                Vector3 tp = hit.point;
                targetPosition.y = tp.y + groundDistance;
                isGrounded = true;
            }else if(isGrounded)
                isGrounded = false;
            if (!animatorHandler.animator.GetBool("isFalling") && !isGrounded)
                animatorHandler.PlayTargetAnimation("Falling", false, 1);
            if (!isGrounded) {
                Vector3 vel = rigidbody.velocity.normalized;
                vel *= (movementSpeed) / inAirMovementControl; 
                rigidbody.velocity.Set(vel.x, rigidbody.velocity.y, vel.z);
                animatorHandler.animator.SetFloat("FallingSpeed", rigidbody.velocity.magnitude);
            }
            if (isGrounded) {
                myTransform.position = targetPosition;
            }
            animatorHandler.animator.SetBool("isFalling", !isGrounded);
        }
    }
}