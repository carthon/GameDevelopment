using System;
using _Project.Scripts.Network.MessageDataStructures;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Handlers {
    public class InputHandler : MonoBehaviour {
        [Flags]
        public enum PlayerActions : ulong {
            None            = 0UL,
            Moving          = 1UL << 0,
            Jumping         = 1UL << 1,
            DoubleJumping   = 1UL << 2,
            Sprinting       = 1UL << 3,
            Crouching       = 1UL << 4,
            InInventory     = 1UL << 5,
            Clicked         = 1UL << 6,
            Searching       = 1UL << 7,
            Attacking       = 1UL << 8,
        } // Hasta 1UL << 63 si se necesita
        private static InputHandler _singleton;
        [Header("Basic Inputs")]
        private bool _b_Input;
        private Vector2 _cameraInput;
        private bool _dropItem;
        private bool _menu;

        private PlayerControlls _inputActions;

        private Vector2 _movementInput;
        private Action<bool> _OnActivateUI;
        private Action<InputAction.CallbackContext> OnClick;
        public Action OnItemRotation;
        public Action OnPickAction;
        public Action OnJumpAction;
        public Action OnToggleInventory;
        private Action<int> _OnHotbarEquip;
        private Action<int> _OnLeftHandEquip;
        private bool _rb_Input;
        private float _rollInputTimer;
        private float _jumpInputTimer;
        private bool _rt_Input;

        public float MouseX { get; private set; }

        public float MouseY { get; private set; }

        public bool SwapView { get; private set; }

        public bool SwapPerson { get; private set; }
        public bool Clicked { get; set; }
        public bool RClicked { get; set; }
        public static InputHandler Singleton
        {
            get => _singleton;
            private set {
                if (_singleton == null)
                    _singleton = value;
                else if(_singleton != null) {
                    Debug.Log($"{nameof(InputHandler)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        public Vector2 MovementInput { get => _movementInput; }
        public float Horizontal { get; private set; }

        public float Vertical { get; private set; }

        public bool IsMoving { get => Math.Abs(Vertical) > 0 || Math.Abs(Horizontal) > 0; }
        public bool IsSprinting { get; private set; }

        public bool IsRolling { get; private set; }

        public bool IsJumping { get; private set; }
        public bool IsDoubleJumping { get; private set; }
        public bool IsCrouching { get; private set; }

        public bool IsUIEnabled { get; private set; }
        public bool IsInMenu { get; private set; }
        public bool IsInInventory { get; private set; }
        public PlayerActions GetActions() {
            PlayerActions mask = PlayerActions.None;
            if (IsMoving) mask |= PlayerActions.Moving;
            if (IsJumping) mask |= PlayerActions.Jumping;
            if (IsDoubleJumping) mask |= PlayerActions.DoubleJumping;
            if (IsSprinting) mask |= PlayerActions.Sprinting;
            if (IsCrouching) mask |= PlayerActions.Crouching;
            if (IsInInventory) mask |= PlayerActions.InInventory;
            if (Clicked) mask |= PlayerActions.Clicked;
            return mask;
        }

        [field: Header("UI")]
        public int HotbarSlot { get; private set; }

        public bool EquipInput { get; private set; }

        public void ClearInputs() {
            SwapView = false;
            SwapPerson = false;
            IsRolling = false;
            EquipInput = false;
        }
        public void Awake() {
            _singleton = this;
        }
        public void OnEnable() {
            if (_inputActions == null) {
                _inputActions = new PlayerControlls();
                _inputActions.PlayerSpaceMovement.Movement.performed += i => _movementInput = i.ReadValue<Vector2>();
                _inputActions.PlayerSpaceMovement.Camera.performed += i => _cameraInput = i.ReadValue<Vector2>();
                _inputActions.PlayerActions.Crouch.started += i => IsCrouching = true;
                _inputActions.PlayerActions.Crouch.canceled += i => IsCrouching = false;
                _inputActions.PlayerActions.RB.started += i => _rb_Input = true;
                _inputActions.PlayerActions.RB.canceled += i => _rb_Input = false;
                _inputActions.PlayerActions.PickItem.performed += context => OnPickAction?.Invoke();
                _inputActions.PlayerActions.Jump.performed += context => OnJumpAction?.Invoke();
                _inputActions.PlayerActions.Jump.canceled += context => IsJumping = false;
                _inputActions.PlayerActions.Jump.canceled += context => IsDoubleJumping = false;
                OnJumpAction += HandleJumpInput;
                
                _inputActions.Camera.OrbitalView.performed += i => SwapView = true;
                _inputActions.Camera.SwapPersonCamera.performed += i => SwapPerson = true;
                
                _inputActions.UIActions.Menu.performed += HandleMenuCommand;
                _inputActions.UIActions.PlayerOverview.performed += ToggleInventory;
                _inputActions.UIActions.Click.started += i => Clicked = true;
                _inputActions.UIActions.Click.canceled += i => Clicked = false;
                _inputActions.UIActions.RClick.started += i => RClicked = true;
                _inputActions.UIActions.RClick.canceled += i => RClicked = false;
                _inputActions.UIActions.RotateInventorItem.performed += context => OnItemRotation.Invoke();
                _rt_Input = _inputActions.PlayerActions.RB.phase == InputActionPhase.Started;
                _inputActions.UIActions.HotbarInput.performed += i => {
                    HotbarSlot = (int) i.ReadValue<float>();
                    EquipInput = true;
                };
                _inputActions.UIActions.EquipLeftHand.performed += i => {
                    HotbarSlot = -1;
                    EquipInput = true;
                };
            }

            _inputActions.Enable();
        }
        private void ToggleInventory(InputAction.CallbackContext obj) {
            if (obj.performed) {
                IsInInventory = !IsInInventory;
                OnToggleInventory?.Invoke();
            }
        }
        private void HandleMenuCommand(InputAction.CallbackContext obj) {
            if (obj.performed) {
                IsInMenu = !IsInMenu;
                Cursor.lockState = IsInMenu ? CursorLockMode.None : CursorLockMode.Locked;
            }
        }

        private void OnDisable() {
            _singleton = null;
            _inputActions.Disable();
            OnJumpAction -= HandleJumpInput;
        }

        public void Update() {
            float delta = Time.deltaTime;
            _jumpInputTimer += Time.deltaTime;
            MoveInput(delta);
            HandleCameraInput(delta);
            HandleRollAndSprintInput(delta);
            HandleUIInput();
        }
        private void HandleCameraInput(float delta) {
            if (CameraHandler.CameraHandler.Singleton == null)
                return;
            MouseX = _cameraInput.x;
            MouseY = _cameraInput.y;
        }

        private void HandleUIInput() {
            IsUIEnabled = IsInMenu || IsInInventory;
            _dropItem = _inputActions.UIActions.DropItem.phase == InputActionPhase.Started;
        }

        private void MoveInput(float delta) {
            Horizontal = _movementInput.x;
            Vertical = _movementInput.y;
        }

        private void HandleJumpInput() {
            IsJumping = true;
            if (_jumpInputTimer is > 0 and < .2f) {
                IsDoubleJumping = true;
                _jumpInputTimer = 0;
            }
            else if (_jumpInputTimer > 0) {
                _jumpInputTimer = 0;
            }
        }

        private void HandleRollAndSprintInput(float delta) {
            _b_Input = _inputActions.PlayerActions.Roll.phase == InputActionPhase.Performed;
            if (_b_Input) {
                _rollInputTimer += delta;
                if (Vertical >= 0)
                    IsSprinting = true;
            }
            else {
                if (_rollInputTimer is > 0 and < .5f) {
                    IsRolling = true;
                    _rollInputTimer = 0;
                }
                else if (_rollInputTimer > 0) {
                    _rollInputTimer = 0;
                }
                IsSprinting = false;
            }
        }
    }
}