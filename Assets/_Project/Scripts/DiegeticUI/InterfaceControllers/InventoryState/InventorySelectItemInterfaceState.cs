using System;
using System.Collections.Generic;
using _Project.Libraries.QuickOutline.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using UnityEngine;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState {
    public class InventorySelectItemInterfaceState : InterfaceAbstractBaseState {
        private InventoryInterfaceState _inventoryInterfaceState;
        public InventorySelectItemInterfaceState(InterfaceStateFactory factory, ContainerRenderer context) : base(factory, context) {
        }
        protected override void UpdateState() {
            CheckSwitchStates();
        }
        private bool TrySelectItem(Outline obj, int count = -1) {
            if (Context.renderedItems.TryGetValue(obj.transform.gameObject, out Vector2Int inventorySlot)) {
                Inventory inventory = Context.Inventory;
                itemSelected = inventory.GetInventorySlot(inventorySlot);
                if (itemSelected.ItemStack.IsEmpty())
                    return !itemSelected.ItemStack.IsEmpty();
                _inventoryInterfaceState.GrabbedItemsCollider.Clear();
                _inventoryInterfaceState.LastGrabbedItemsLocalPosition.Clear();
                Collider child = obj.GetComponent<Collider>();
                _inventoryInterfaceState.GrabbedItemsCollider.Add(child);
                _inventoryInterfaceState.LastGrabbedItemsLocalPosition.Add(child.transform.localPosition);
            }
            return !itemSelected.ItemStack.IsEmpty();
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