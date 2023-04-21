using _Project.Scripts.Handlers;
using Cinemachine;
using static Cinemachine.CinemachineBlendDefinition;

namespace _Project.Scripts.StateMachine.CameraStates {
    public class ThirdPersonCameraState : CameraAbstractBaseState {
        private CinemachineVirtualCamera _camera;
        public ThirdPersonCameraState(CameraHandler cameraHandler) : base(cameraHandler) {  }
        public override void EnterState() {
            _camera = cameraHandler.thirdPersonCamera;
            cameraHandler.MainCamera.cullingMask &= ~(1 << cameraHandler.layerFirstPerson);
            _camera.Priority += cameraHandler.ActiveCameraPriorityModifier;
            cameraHandler.EnableCameraRotation = true;
        }
        public override void ExitState() {
            _camera.Priority -= cameraHandler.ActiveCameraPriorityModifier;
            cameraHandler.mainCameraBrain.m_DefaultBlend.m_Style = Style.EaseInOut;
        }
        protected override void UpdateState() {
            if (InputHandler.Singleton.SwapPerson) {
                cameraHandler.ChangeState(new FirstPersonCameraState(cameraHandler));
            }
        }
        public override string StateName() => "Third Person Camera";
        protected override CinemachineVirtualCamera Camera() => _camera;
    }
}