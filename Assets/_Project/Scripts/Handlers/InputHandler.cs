using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

namespace _Project.Scripts.Handlers {
    public class InputHandler : MonoBehaviour {
        public float horizontal;
        public float vertical;
        public float mouseX;
        public float mouseY;
        
        public bool b_Input;
        public bool rb_Input;
        public bool rt_Input;
        public bool playerOverview;
        public bool rollFlag;
        public bool sprintFlag;

        private PlayerControlls inputActions;
        
        private Vector2 movementInput;
        private Vector2 cameraInput;
        private float rollInputTimer;

        public void OnEnable() {
            if (inputActions == null) {
                inputActions = new PlayerControlls();
                inputActions.PlayerSpaceMovement.Movement.performed += inputActions => movementInput = inputActions.ReadValue<Vector2>();
                inputActions.PlayerSpaceMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
            }
            
            inputActions.Enable();
        }

        private void OnDisable() {
            inputActions.Disable();
        }

        public void TickInput(float delta) {
            MoveInput(delta);
            HandleRollInput(delta);
            HandleAttackInput(delta);
            HandleUIInput(delta);
        }

        private void HandleUIInput(float delta) {
            inputActions.UIActions.PlayerOverview.performed += i => playerOverview = true;
        }

        private void MoveInput(float delta) {
            horizontal = movementInput.x;
            vertical = movementInput.y;
            mouseX = cameraInput.x;
            mouseY = cameraInput.y;
        }

        private void HandleAttackInput(float delta) {
            inputActions.PlayerActions.RB.performed += i => rb_Input = true;
            inputActions.PlayerActions.RB.performed += i => rt_Input = true;
        }

        private void HandleRollInput(float delta) {
            b_Input = inputActions.PlayerActions.Roll.phase == InputActionPhase.Started;
            if (b_Input) {
                rollInputTimer += delta;
                sprintFlag = true;
            }else{
                if (rollInputTimer > 0 && rollInputTimer < .5f) {
                    sprintFlag = false;
                    rollFlag = true;
                    rollInputTimer = 0;
                }else if (rollInputTimer > 0)
                    rollInputTimer = 0;
            }
        }
    }
}