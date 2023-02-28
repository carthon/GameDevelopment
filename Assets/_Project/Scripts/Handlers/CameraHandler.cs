using _Project.Scripts.StateMachine.Camera;
using Cinemachine;
using Scripts.DataClasses;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class CameraHandler : MonoBehaviour {
        private static CameraHandler _singleton;
    
        public int layerFirstPerson = 11;

        [SerializeField]
        private CameraData _cameraData;
        [SerializeField]
        private Transform _cameraPivot;
        [SerializeField]
        private Transform _cameraFollow;
        [SerializeField]
        private float _followSmoothness = 1f;
        private CinemachineVirtualCamera _activeCamera;

        public readonly int ActiveCameraPriorityModifier = 31337;
        private float _cameraPitch;
        private float _cameraYaw;
    
        public CinemachineVirtualCamera firstPersonCamera;
        public CinemachineVirtualCamera thirdPersonCamera;
        public CinemachineVirtualCamera inventoryCamera;
        public CinemachineVirtualCamera orbitalCamera;

        private CinemachineInputProvider _orbitalCameraInput;
        private Vector3 _playerLookInput = Vector3.zero;

        public CameraAbstractBaseState cameraCurrentState;

        [Header("Camera")]
        private Transform _playerTransform;
        private Vector3 _previousLookInput = Vector3.zero;
        [SerializeField] private string currentCameraStatus;
        public Camera MainCamera { get; private set; }
        public bool EnableCameraRotation { get; set; }
        public CameraData CameraData => _cameraData;
        public static CameraHandler Singleton
        {
            get => _singleton;
            private set {
                if (_singleton == null)
                    _singleton = value;
                else if(_singleton != null) {
                    Debug.Log($"{nameof(CameraHandler)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        public void InitializeCamera(Transform cameraFollow, Transform cameraPivot) {
            MainCamera = Camera.main;
            orbitalCamera = GameObject.Find("OrbitalCamera").GetComponent<CinemachineVirtualCamera>();
            firstPersonCamera = GameObject.Find("1stPersonCamera").GetComponent<CinemachineVirtualCamera>();
            thirdPersonCamera = GameObject.Find("3rdPersonCamera").GetComponent<CinemachineVirtualCamera>();
            inventoryCamera = GameObject.Find("InventoryCamera").GetComponent<CinemachineVirtualCamera>();
            _cameraFollow = cameraFollow;
            _cameraPivot = cameraPivot;
            orbitalCamera.Follow = _cameraFollow;
            inventoryCamera.Follow = _cameraFollow;
            firstPersonCamera.Follow = _cameraFollow;
            thirdPersonCamera.Follow = _cameraFollow;
            orbitalCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = _cameraData.sensitivityX;
            orbitalCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = _cameraData.sensitivityY;
            _orbitalCameraInput = orbitalCamera.GetComponent<CinemachineInputProvider>();
            var main = MainCamera.GetComponent<CinemachineBrain>();
            main.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;
            ChangeState(new ThirdPersonCameraState(this));
        }
        public void Awake() {
            _singleton = this;
        }
        public void Tick(float delta) {
            cameraCurrentState.UpdateStates();
            currentCameraStatus = cameraCurrentState.StateName();
            UpdateLookInput(InputHandler.Singleton.MouseX, InputHandler.Singleton.MouseY);
        }
        public void FixedTick(float fixedTick) {
            if (cameraCurrentState != null && cameraCurrentState.GetType() != typeof(OrbitalCameraState)) {
                CameraPitch(fixedTick);
                CameraYaw(fixedTick);
                CameraSnapRotation();
                CameraSnapFollow();
            }
        }
        public void ChangeState(CameraAbstractBaseState state) {
            if (cameraCurrentState != null) cameraCurrentState.ExitState();
            cameraCurrentState = state;
            cameraCurrentState.EnterState();
        }
        private void UpdateLookInput(float mouseX, float mouseY) {
            if(EnableCameraRotation) {
                _previousLookInput = _playerLookInput;
                _playerLookInput = new Vector3(mouseX * _cameraData.sensitivityX, mouseY * _cameraData.sensitivityY, 0) * Time.fixedDeltaTime;
            }
            //return Vector3.Lerp(_previousLookInput, _playerLookInput, _cameraData.playerLookInputLerpSpeed);
        }
        public Vector3 GetLookDirection() {
            return CameraPivot.rotation.eulerAngles;
        }
        private void CameraSnapRotation() {
            _cameraFollow.rotation = _cameraPivot.rotation;
        }
        private void CameraSnapFollow() {
            var cameraPivot = _cameraPivot.transform.position;
            var cameraFollow = _cameraFollow.transform.position;
            _cameraPivot.transform.position = cameraFollow;
        }
        private void CameraPitch(float fixedTick) {
            var rotationValues = _cameraPivot.rotation.eulerAngles;
            _cameraPitch += -1 * _playerLookInput.y * fixedTick;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -_cameraData.pitchLimitTopLimit, _cameraData.pitchLimitBottomLimit);
            _cameraPivot.rotation = Quaternion.Euler(_cameraPitch, rotationValues.y, rotationValues.z);
        }
        private void CameraYaw(float fixedTick) {
            var rotationValues = _cameraPivot.rotation.eulerAngles;
            _cameraYaw += _playerLookInput.x * fixedTick;
            //_cameraYaw = Mathf.Clamp(_cameraPitch, -_cameraData.pitchLimitTopLimit, _cameraData.pitchLimitBottomLimit);
            _cameraPivot.rotation = Quaternion.Euler(rotationValues.x, _cameraYaw, rotationValues.z);
        }
        public Transform CameraFollow { get => _cameraFollow; }
        public Transform CameraPivot { get => _cameraPivot; }
    }
}