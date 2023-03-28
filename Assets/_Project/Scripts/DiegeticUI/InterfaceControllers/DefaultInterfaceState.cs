using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.Client;
using UnityEngine;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers {
    public class DefaultInterfaceState : InterfaceAbstractBaseState {
        private Player _player;
        private Grabbable LastGrabbable { get; set; }

        public DefaultInterfaceState(InterfaceStateFactory factory, UIHandler context) : base(factory, context) {
            _isRootState = true;
            EnterState();
        }
        protected override void UpdateState() {
            Grabbable currentGrabbable = _player.GetNearGrabbable();
            if(currentGrabbable != null) {
                if (!currentGrabbable.Equals(LastGrabbable)) {
                    LastGrabbable = currentGrabbable;
                    currentGrabbable.SetOutline(true);
                }
            }
            else if(LastGrabbable != null) {
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