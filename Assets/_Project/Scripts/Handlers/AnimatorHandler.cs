using System;
using _Project.Scripts.Components;
using UnityEngine;
using UnityEngine.LowLevel;

namespace _Project.Scripts.Handlers {
    public class AnimatorHandler : MonoBehaviour {
        public Animator animator;
        public Locomotion locomotion;
        public bool canRotate;
        
        private static readonly int IsInteracting = Animator.StringToHash("isInteracting");
        private static readonly int Vertical = Animator.StringToHash("Vertical");
        private static readonly int Horizontal = Animator.StringToHash("Horizontal");

        public void Initialize() {
            animator = GetComponent<Animator>();
            locomotion = GetComponentInParent<Locomotion>();
        }
        
        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement, bool isSprinting) {

            #region Vertical
            float v = 0;

            if (verticalMovement > 0 && verticalMovement < 0.55f) {
                v = 0.5f;
            }else if (verticalMovement > 0.55f) {
                v = 1;
            }else if (verticalMovement < 0 && verticalMovement > -0.55f) {
                v = -0.5f;
            }else if (verticalMovement < -0.55f) {
                v = -1;
            }
            else {
                v = 0;
            }
            #endregion
            #region Horizontal
            float h = 0;

            if (horizontalMovement > 0 && horizontalMovement < 0.55f) {
                h = 0.5f;
            }else if (horizontalMovement > 0.55f) {
                h = 1;
            }else if (horizontalMovement < 0 && horizontalMovement > -0.55f) {
                h = -0.5f;
            }else if (horizontalMovement < -0.55f) {
                h = -1;
            }
            else {
                h = 0;
            }
            #endregion

            if (isSprinting) {
                v = 2;
                h = horizontalMovement;
            }
            animator.SetFloat(Vertical, v, 0.1f, Time.deltaTime);
            animator.SetFloat(Horizontal, h, 0.1f, Time.deltaTime);
        }

        public void PlayTargetAnimation(string targetAnim, bool isInteracting, int layer) {
            animator.applyRootMotion = isInteracting;
            animator.SetBool(IsInteracting, isInteracting);
            animator.CrossFade(targetAnim,.2f, layer);
        }
        public void PlayTargetAnimation(string targetAnim, bool isInteracting) {
            animator.applyRootMotion = isInteracting;
            animator.SetBool(IsInteracting, isInteracting);
            animator.CrossFade(targetAnim,.2f);
        }
        private void OnAnimatorMove() {
            if (animator.GetBool(IsInteracting) == false)
                return;

            float delta = Time.deltaTime;
            locomotion.rigidbody.drag = 0;
            Vector3 deltaPosition = animator.deltaPosition;
            deltaPosition.y = 0;
            Vector3 velocity = deltaPosition / delta;
            locomotion.rigidbody.velocity = velocity;
        }
        public void CanRotate() => canRotate = true;
        public void StopRotation() => canRotate = false;
    }
}