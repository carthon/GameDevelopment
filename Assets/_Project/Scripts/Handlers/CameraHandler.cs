using System;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using Cinemachine;
using Scripts.DataClasses;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CameraHandler : MonoBehaviour {
    [SerializeField]
    private int layerFirstPerson = 11;

    [SerializeField]
    private CameraData _cameraData;
    [SerializeField]
    private Transform _cameraPivot;
    [SerializeField]
    private Transform _cameraFollow;
    [SerializeField]
    private float _followSmoothness = 1f;
    private CinemachineVirtualCamera _activeCamera;

    private readonly int _activeCameraPriorityModifier = 31337;
    private float _cameraPitch;
    private float _cameraYaw;
    private CinemachineVirtualCamera _firstPersonCamera;

    private InputHandler _inputHandler;

    private CinemachineVirtualCamera _orbitalCamera;
    private CinemachineInputProvider _orbitalCameraInput;
    private Vector3 _playerLookInput = Vector3.zero;

    [Header("Camera")]
    private Transform _playerTransform;
    private Vector3 _previousLookInput = Vector3.zero;
    private CinemachineVirtualCamera _thirdPersonCamera;
    public bool UsingFirstPersonCamera { get; set; }
    public bool UsingOrbitalCamera { get; set; }
    public Camera MainCamera { get; private set; }
    public CameraData CameraData => _cameraData;

    public void InitializeCamera() {
        _inputHandler = GetComponent<InputHandler>();
        MainCamera = Camera.main;
        SetOrbitalCamera(GameObject.Find("OrbitalCamera").GetComponent<CinemachineVirtualCamera>());
        SetFirstPersonCamera(GameObject.Find("1stPersonCamera").GetComponent<CinemachineVirtualCamera>());
        SetThirdPersonCamera(GameObject.Find("3rdPersonCamera").GetComponent<CinemachineVirtualCamera>());
        _orbitalCamera.Follow = _cameraFollow;
        _firstPersonCamera.Follow = _cameraFollow;
        _thirdPersonCamera.Follow = _cameraFollow;
        _orbitalCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = _cameraData.sensitivityX;
        _orbitalCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = _cameraData.sensitivityY;
        _orbitalCameraInput = _orbitalCamera.GetComponent<CinemachineInputProvider>();
        var main = MainCamera.GetComponent<CinemachineBrain>();
        main.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;
        ChangeCamera();
    }
    public void Tick(float delta) {
        if (_inputHandler.SwapView || _inputHandler.FirstPerson)
            ChangeCamera();
    }
    public void FixedTick() {
        if (!UsingOrbitalCamera) {
            CameraPitch();
            CameraYaw();
            //CameraSnapFollow();
            //CameraSnapRotation();
        }
    }
    private void ChangeCamera() {
        UsingOrbitalCamera = false;
        //var main = MainCamera.GetComponent<CinemachineBrain>();
        if (_inputHandler.SwapView) {
            SetCameraPriorities(_orbitalCamera);
            UsingOrbitalCamera = true;
            UsingFirstPersonCamera = false;
        }
        else if (_firstPersonCamera == _activeCamera) {
            //Sets to thirdPerson
            SetCameraPriorities(_firstPersonCamera, _thirdPersonCamera);
            MainCamera.cullingMask &= ~(1 << layerFirstPerson);
            UsingFirstPersonCamera = false;
        }
        else if (_thirdPersonCamera == _activeCamera) {
            //Sets to firstPerson
            SetCameraPriorities(_thirdPersonCamera, _firstPersonCamera);
            MainCamera.cullingMask |= 1 << layerFirstPerson;
            UsingFirstPersonCamera = true;
        }
        else {
            _firstPersonCamera.Priority += _activeCameraPriorityModifier;
            _activeCamera = _firstPersonCamera;
            UsingFirstPersonCamera = true;
        }
        //main.m_UpdateMethod = UsingFirstPersonCamera ? CinemachineBrain.UpdateMethod.SmartUpdate : CinemachineBrain.UpdateMethod.FixedUpdate;
    }
    private void SetCameraPriorities(CinemachineVirtualCamera newCamera) {
        _activeCamera.Priority -= _activeCameraPriorityModifier;
        newCamera.Priority += _activeCameraPriorityModifier;
        _activeCamera = newCamera;
    }
    private void SetCameraPriorities(CinemachineVirtualCamera current, CinemachineVirtualCamera newCamera) {
        current.Priority -= _activeCameraPriorityModifier;
        newCamera.Priority += _activeCameraPriorityModifier;
        _activeCamera = newCamera;
    }
    public Vector3 GetDirectionFromMouse(float mouseX, float mouseY) {
        _previousLookInput = _playerLookInput;
        _playerLookInput = new Vector3(mouseX * _cameraData.sensitivityX, mouseY * _cameraData.sensitivityY, 0) * Time.fixedDeltaTime;
        //return Vector3.Lerp(_previousLookInput, _playerLookInput, _cameraData.playerLookInputLerpSpeed);
        return _playerLookInput;
    }
    private void CameraSnapRotation() {
        _cameraFollow.rotation = _cameraPivot.rotation;
    }
    private void CameraSnapFollow() {
        var cameraPivot = _cameraPivot.transform.position;
        var cameraFollow = _cameraFollow.transform.position;
        _cameraPivot.transform.position = cameraFollow;
    }
    private void CameraPitch() {
        var rotationValues = _cameraPivot.rotation.eulerAngles;
        _cameraPitch += -1 * _playerLookInput.y;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -_cameraData.pitchLimitTopLimit, _cameraData.pitchLimitBottomLimit);
        _cameraPivot.rotation = Quaternion.Euler(_cameraPitch, rotationValues.y, rotationValues.z);
    }
    private void CameraYaw() {
        var rotationValues = _cameraPivot.rotation.eulerAngles;
        _cameraYaw += _playerLookInput.x;
        //_cameraYaw = Mathf.Clamp(_cameraPitch, -_cameraData.pitchLimitTopLimit, _cameraData.pitchLimitBottomLimit);
        _cameraPivot.rotation = Quaternion.Euler(rotationValues.x, _cameraYaw, rotationValues.z);
    }
    public void SetOrbitalCamera(CinemachineVirtualCamera find) {
        _orbitalCamera = find;
    }
    public void SetFirstPersonCamera(CinemachineVirtualCamera find) {
        _firstPersonCamera = find;
    }
    public void SetThirdPersonCamera(CinemachineVirtualCamera find) {
        _thirdPersonCamera = find;
    }
    public void SetPlayerFollow(Transform player) {
        _playerTransform = player;
    }
    public Transform CameraFollow { get => _cameraFollow; }
    public Transform CameraPivot { get => _cameraPivot; }
    public void SetOrbitalInput(bool state) => _orbitalCameraInput.enabled = state;
}