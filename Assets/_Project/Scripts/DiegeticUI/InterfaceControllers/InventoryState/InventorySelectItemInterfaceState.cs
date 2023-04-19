using System;
using System.Collections.Generic;
using _Project.Libraries.QuickOutline.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState {
    public class InventorySelectItemInterfaceState : InterfaceAbstractBaseState {
        private InventoryInterfaceState _inventoryInterfaceState;
        public InventorySelectItemInterfaceState(InterfaceStateFactory factory, UIHandler context) : base(factory, context) {
        }
        protected override void UpdateState() {
            CheckSwitchStates();
        }
        private bool TrySelectItem(Outline obj, int count = -1) {
            int slot = Int32.Parse(obj.transform.name);
            int inventoryId = Int32.Parse(obj.transform.parent.name);
            Inventory inventory = ContainerRenderer.Singleton.inventory.Inventories[inventoryId];
            itemSelected = count > -1 ? inventory.GetItemStack(slot, count) : inventory.GetItemStack(slot);
            if (!itemSelected.IsEmpty()) {
                _inventoryInterfaceState.GrabbedItems = new List<Transform>();
                _inventoryInterfaceState.LastGrabbedItemsLocalPosition = new List<Vector3>();
                for (int i = 0; i < itemSelected.GetCount(); i++) {
                    Transform child = obj.transform.GetChild(i);
                    _inventoryInterfaceState.GrabbedItems.Add(child);
                    _inventoryInterfaceState.LastGrabbedItemsLocalPosition.Add(child.localPosition);
                }
            }
            return !itemSelected.IsEmpty();
        }
        protected sealed override void EnterState() {
            _inventoryInterfaceState = (InventoryInterfaceState) CurrentSuperState;
        }
        protected override void ExitState() {
        }
        public override void InitializeSubState() {
        }
        public override void CheckSwitchStates() {
            if (InputHandler.Singleton.Clicked && _inventoryInterfaceState?.HitMouseOutline != null) {
                if(TrySelectItem(_inventoryInterfaceState.HitMouseOutline)) {
                    SwitchState(Factory.InventoryGrabItemState(itemSelected));
                }
                InputHandler.Singleton.Clicked = false;
            }
        }
        public override string StateName() => "SelectItemState";
    }
}