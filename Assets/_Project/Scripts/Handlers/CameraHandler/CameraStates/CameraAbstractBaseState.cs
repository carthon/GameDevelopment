using Cinemachine;

namespace _Project.Scripts.Handlers.CameraHandler.CameraStates {
    public abstract class CameraAbstractBaseState {
        protected CameraHandler cameraHandler;
        protected CameraAbstractBaseState(CameraHandler cameraHandler) {
            this.cameraHandler = cameraHandler;
        }
        public abstract void EnterState();
        public abstract void ExitState();
        protected abstract void UpdateState();
        public void UpdateStates() {
            UpdateState();
            FromAnyState();
        }
        private void FromAnyState() {
            if (InputHandler.Singleton.SwapView && cameraHandler.cameraCurrentState.GetType() != typeof(OrbitalCameraState)) {
                cameraHandler.ChangeState(new OrbitalCameraState(cameraHandler, cameraHandler.cameraCurrentState));
            }
            if (InputHandler.Singleton.IsInInventory && cameraHandler.cameraCurrentState.GetType() != typeof(InventoryCameraState)) {
                cameraHandler.ChangeState(new InventoryCameraState(cameraHandler, cameraHandler.cameraCurrentState));
            }
        }
        protected abstract CinemachineVirtualCamera Camera();
        public abstract string StateName();
    }
}