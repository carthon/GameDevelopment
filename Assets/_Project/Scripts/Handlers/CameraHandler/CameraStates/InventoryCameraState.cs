using _Project.Scripts.Handlers;
using Cinemachine;
using UnityEngine;
using static Cinemachine.CinemachineBlendDefinition;

namespace _Project.Scripts.StateMachine.CameraStates {
    public class InventoryCameraState : CameraAbstractBaseState {
        private CinemachineVirtualCamera _camera;
        private readonly CameraAbstractBaseState _lastState;
        public InventoryCameraState(CameraHandler cameraHandler, CameraAbstractBaseState lastState) : base(cameraHandler) { _lastState = lastState; }
        public override void EnterState() {
            _camera = cameraHandler.inventoryCamera;
            cameraHandler.MainCamera.cullingMask = 1 << cameraHandler.layerInventory | 1 << cameraHandler.layerController;
            cameraHandler.EnableCameraRotation = false;
            cameraHandler.mainCameraBrain.m_DefaultBlend.m_Style = Style.Cut;
            _camera.Priority += cameraHandler.ActiveCameraPriorityModifier;
        }
        public override void ExitState() {
            _camera.Priority -= cameraHandler.ActiveCameraPriorityModifier;
            cameraHandler.EnableCameraRotation = true;
            cameraHandler.MainCamera.cullingMask = ~(1 << cameraHandler.layerInventory);
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