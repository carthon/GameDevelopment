using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Handlers;
using _Project.Scripts.Utils;
using QuickOutline.Scripts;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers {
    public abstract class InterfaceAbstractBaseState {
        protected InterfaceAbstractBaseState CurrentSubState;
        protected InterfaceAbstractBaseState CurrentSuperState;
        public Outline HitMouseOutline;
        protected ItemStack itemSelected;
        protected bool _isRootState = false;
        protected InterfaceStateFactory Factory;
        protected UIHandler Context;
        protected abstract void UpdateState();
        protected abstract void EnterState();
        protected abstract void ExitState();
        public abstract string StateName();
        
        protected InterfaceAbstractBaseState(InterfaceStateFactory factory, UIHandler context) {
            this.Factory = factory;
            this.Context = context;
        }
        
        protected void SwitchState(InterfaceAbstractBaseState newState) {
            ExitState();
            newState.EnterState();
            if (_isRootState) {
                Context.LastState = Context.CurrentState;
                Context.CurrentState = newState;
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