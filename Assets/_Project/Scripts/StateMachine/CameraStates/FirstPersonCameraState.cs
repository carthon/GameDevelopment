using _Project.Scripts.Handlers;
using Cinemachine;

namespace _Project.Scripts.StateMachine.CameraStates {
    public class FirstPersonCameraState : CameraAbstractBaseState {
        private CinemachineVirtualCamera _camera;
        public FirstPersonCameraState(CameraHandler cameraHandler) : base(cameraHandler) { }
        public override void EnterState() {
            _camera = cameraHandler.firstPersonCamera;
            cameraHandler.EnableCameraRotation = true;
            cameraHandler.MainCamera.cullingMask |= 1 << cameraHandler.layerFirstPerson;
            _camera.Priority += cameraHandler.ActiveCameraPriorityModifier;
        }
        protected override void UpdateState() {
            if (InputHandler.Singleton.SwapPerson) {
                cameraHandler.ChangeState(new ThirdPersonCameraState(cameraHandler));
            }
        }
        public override void ExitState() {
            _camera.Priority -= cameraHandler.ActiveCameraPriorityModifier;
        }
        public override string StateName() => "First Person Camera";
        protected override CinemachineVirtualCamera Camera() => _camera;
    }
}