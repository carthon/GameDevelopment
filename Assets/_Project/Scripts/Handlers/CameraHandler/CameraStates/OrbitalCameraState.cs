using _Project.Scripts.Handlers;
using Cinemachine;

namespace _Project.Scripts.StateMachine.CameraStates {
    public class OrbitalCameraState : CameraAbstractBaseState {
        private CinemachineVirtualCamera _camera;
        private readonly CameraAbstractBaseState _lastState;
        public OrbitalCameraState(CameraHandler cameraHandler, CameraAbstractBaseState lastState) : base(cameraHandler) { _lastState = lastState; }
        public override void EnterState() {
            _camera = cameraHandler.orbitalCamera;
            cameraHandler.MainCamera.cullingMask &= ~(1 << cameraHandler.layerFirstPerson);
            cameraHandler.EnableCameraRotation = false;
            _camera.Priority += cameraHandler.ActiveCameraPriorityModifier;
        }
        public override void ExitState() {
            _camera.Priority -= cameraHandler.ActiveCameraPriorityModifier;
            cameraHandler.EnableCameraRotation = true;
        }
        protected override void UpdateState() {
            if (InputHandler.Singleton.SwapView || InputHandler.Singleton.SwapPerson) {
                cameraHandler.ChangeState(_lastState);
            }
        }
        protected override CinemachineVirtualCamera Camera() => _camera;
        public override string StateName() => "Orbital Camera";
    }
}