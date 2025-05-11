using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.Client;
using UnityEngine;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers {
    public class DefaultInterfaceState : InterfaceAbstractBaseState {
        private Player _player;
        private Grabbable LastGrabbable { get; set; }

        public DefaultInterfaceState(InterfaceStateFactory factory, ContainerRenderer context) : base(factory, context) {
            _isRootState = true;
            EnterState();
        }
        protected override void UpdateState() {
            Ray ray = new Ray(_player.HeadPivot.position, _player.HeadPivot.forward);
            Debug.DrawRay(ray.origin, ray.direction);
            Grabbable currentGrabbable = _player.GetNearGrabbable();
            if(currentGrabbable is not null) {
                if (!currentGrabbable.Equals(LastGrabbable)) {
                    if (LastGrabbable && LastGrabbable is not null)
                        LastGrabbable.SetOutline(false);
                    LastGrabbable = currentGrabbable;
                    currentGrabbable.SetOutline(true);
                }
            }
            else if(LastGrabbable is not null) {
                if (LastGrabbable != null)
                    LastGrabbable.SetOutline(false);
                LastGrabbable = null;
            }
            CheckSwitchStates();
        }
        public override void CheckSwitchStates() {
            if (InputHandler.Singleton.IsInInventory) {
                SwitchState(Factory.InventoryState());
            }
        }
        public override void InitializeSubState() {
        }
        protected sealed override void EnterState() {
            Cursor.lockState = CursorLockMode.Locked;
            
            _player = Client.Singleton.Player;
        }
        protected override void ExitState() {
            ResetMouseSelection();
        }
        public override string StateName() => "Default";
    }
}