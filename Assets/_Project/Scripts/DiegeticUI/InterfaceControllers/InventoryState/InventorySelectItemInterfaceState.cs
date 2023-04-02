using System;
using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using QuickOutline.Scripts;
using UnityEngine;

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
            Inventory inventory = ContainerRenderer.Singleton.Inventory.Inventories[inventoryId];
            itemSelected = count > -1 ? inventory.GetItemStack(slot, count) : inventory.GetItemStack(slot);
            if (!itemSelected.IsEmpty()) {
                _inventoryInterfaceState.GrabbedItems = new List<Transform>();
                _inventoryInterfaceState.LastGrabbedItemsLocalPosition = new List<Vector3>();
                for (int i = 0; i < obj.transform.childCount; i++) {
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
                    InputHandler.Singleton.Clicked = false;
                    SwitchState(Factory.InventoryGrabItemState(itemSelected));
                }
            }
        }
        public override string StateName() => "SelectItemState";
    }
}