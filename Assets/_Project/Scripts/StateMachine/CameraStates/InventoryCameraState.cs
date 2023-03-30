using _Project.Scripts.Handlers;
using Cinemachine;
using static Cinemachine.CinemachineBlendDefinition;

namespace _Project.Scripts.StateMachine.CameraStates {
    public class InventoryCameraState : CameraAbstractBaseState {
        private CinemachineVirtualCamera _camera;
        private readonly CameraAbstractBaseState _lastState;
        public InventoryCameraState(CameraHandler cameraHandler, CameraAbstractBaseState lastState) : base(cameraHandler) { _lastState = lastState; }
        public override void EnterState() {
            _camera = cameraHandler.inventoryCamera;
            cameraHandler.MainCamera.cullingMask |= 1 << cameraHandler.layerFirstPerson;
            _camera.Priority += cameraHandler.ActiveCameraPriorityModifier;
            cameraHandler.EnableCameraRotation = false;
            cameraHandler.mainCameraBrain.m_DefaultBlend.m_Style = Style.Cut;
        }
        public override void ExitState() {
            _camera.Priority -= cameraHandler.ActiveCameraPriorityModifier;
            cameraHandler.EnableCameraRotation = true;
        }
        protected override void UpdateState() {
            if (!InputHandler.Singleton.IsInInventory) {
                cameraHandler.ChangeState(_lastState);
            }
        }
        protected override CinemachineVirtualCamera Camera() => _camera;
        public override string StateName() => "Inventory Camera";
    }
}