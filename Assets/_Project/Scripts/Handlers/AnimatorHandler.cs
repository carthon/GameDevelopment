using _Project.Scripts.Components;
using _Project.Scripts.Constants;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class AnimatorHandler : MonoBehaviour {

        public static readonly int Jumped = Animator.StringToHash("Jumped");
        public static readonly int IsInteracting = Animator.StringToHash("isInteracting");
        public static readonly int IsSprinting = Animator.StringToHash("isSprinting");
        public static readonly int IsFalling = Animator.StringToHash("isFalling");
        public static readonly int IsCrouching = Animator.StringToHash("isCrouching");
        public static readonly int Vertical = Animator.StringToHash("Vertical");
        public static readonly int Horizontal = Animator.StringToHash("Horizontal");
        public static readonly int IsMoving = Animator.StringToHash("isMoving");
        public static readonly int IsPicking = Animator.StringToHash("isPicking");
        public static readonly int IsSearching = Animator.StringToHash("isSearching");
        private Animator _animator;
        //private Locomotion _locomotion;

        private void OnAnimatorMove() {
            if (_animator.GetBool(IsInteracting) == false)
                return;

            var delta = Time.deltaTime;
            //_locomotion.Rb.drag = 0;
            var deltaPosition = _animator.deltaPosition;
            deltaPosition.y = 0;
            var velocity = deltaPosition / delta;
            //_locomotion.Rb.velocity = velocity;
        }

        public void Initialize() {
            _animator = GetComponent<Animator>();
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
        }

        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement) {

            #region Vertical

            float v = 0;

            if (verticalMovement > 0 && verticalMovement < 0.55f)
                v = 0.5f;
            else if (verticalMovement > 0.55f)
                v = 1;
            else if (verticalMovement < 0 && verticalMovement > -0.55f)
                v = -0.5f;
            else if (verticalMovement < -0.55f)
                v = -1;
            else
                v = 0;

            #endregion

            #region Horizontal

            float h = 0;

            if (horizontalMovement > 0 && horizontalMovement < 0.55f)
                h = 0.5f;
            else if (horizontalMovement > 0.55f)
                h = 1;
            else if (horizontalMovement < 0 && horizontalMovement > -0.55f)
                h = -0.5f;
            else if (horizontalMovement < -0.55f)
                h = -1;
            else
                h = 0;

            #endregion
            _animator.SetFloat(Vertical, v, 0.1f, Time.deltaTime);
            _animator.SetFloat(Horizontal, h, 0.1f, Time.deltaTime);
        }

        public void PlayTargetAnimation(string targetAnim, bool isInteracting, int layer) {
            _animator.applyRootMotion = isInteracting;
            _animator.SetBool(IsInteracting, isInteracting);
            _animator.CrossFade(targetAnim, .2f, layer);
        }
        public void PlayTargetAnimation(string targetAnim, bool isInteracting) {
            _animator.applyRootMotion = isInteracting;
            _animator.SetBool(IsInteracting, isInteracting);
            _animator.CrossFade(targetAnim, .2f);
        }
        public void SetBool(string targetBool, bool value) => _animator.SetBool(targetBool, value);
        public void SetBool(int targetBool, bool value) => _animator.SetBool(targetBool, value);
        public void SetTrigger(string targetTrigger) => _animator.SetTrigger(targetTrigger);
        public void SetFloat(string targetTrigger, float value) => _animator.SetFloat(targetTrigger, value);
    }
}