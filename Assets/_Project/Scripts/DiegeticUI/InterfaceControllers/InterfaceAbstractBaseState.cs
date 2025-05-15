using _Project.Libraries.QuickOutline.Scripts;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Handlers;
using _Project.Scripts.Utils;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers {
    public abstract class InterfaceAbstractBaseState {
        protected InterfaceAbstractBaseState CurrentSubState;
        protected InterfaceAbstractBaseState CurrentSuperState;
        public Outline HitMouseOutline;
        protected InventorySlot itemSelected;
        protected bool _isRootState = false;
        protected InterfaceStateFactory Factory;
        protected ContainerRenderer Context;
        protected abstract void UpdateState();
        protected abstract void EnterState();
        protected abstract void ExitState();
        public abstract string StateName();
        
        protected InterfaceAbstractBaseState(InterfaceStateFactory factory, ContainerRenderer context) {
            this.Factory = factory;
            this.Context = context;
        }
        
        protected void SwitchState(InterfaceAbstractBaseState newState) {
            ExitState();
            if (_isRootState) {
                newState.EnterState();
                UIHandler.Instance.LastState = UIHandler.Instance.CurrentState;
                UIHandler.Instance.CurrentState = newState;
            }
            else if (CurrentSuperState != null) CurrentSuperState.SetSubState(newState);
        }
        
        public abstract void CheckSwitchStates();
        public abstract void InitializeSubState();

        protected void SetSubState(InterfaceAbstractBaseState newSubState) {
            CurrentSubState = newSubState;
            newSubState.SetSuperState(this);
            CurrentSubState.EnterState();
        }
        protected void SetSuperState(InterfaceAbstractBaseState newSuperState) {
            CurrentSuperState = newSuperState;
        }
        public void ResetMouseSelection() {
            if (HitMouseOutline != null) {
                HitMouseOutline.enabled = false;
                HitMouseOutline = null;
            }
        }
        public void UpdateStates() {
            UpdateState();
            if (CurrentSubState != null)
                CurrentSubState.UpdateStates();
        }
        private void ExitStates() {
            ExitState();
            if (CurrentSubState != null)
                CurrentSubState.ExitStates();
        }
    }
}