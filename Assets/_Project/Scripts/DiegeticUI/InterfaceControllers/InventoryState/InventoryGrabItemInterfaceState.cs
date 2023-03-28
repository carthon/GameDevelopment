using System;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Handlers;
using UnityEngine;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState {
    public class InventoryGrabItemInterfaceState : InterfaceAbstractBaseState {
        private InventoryInterfaceState _inventoryInterfaceState;
        public InventoryGrabItemInterfaceState(ItemStack grabbedItem, InterfaceStateFactory factory, UIHandler context) : base(factory, context) {
            itemSelected = grabbedItem;
            EnterState();
        }
        protected override void UpdateState() {
            Context.UpdateWatchedVariables("grabbedItems", $"GrabbedItemsCount:{_inventoryInterfaceState.GrabbedItems.Count}");
            if (_inventoryInterfaceState.GrabbedItems.Count > 0) {
                for (int i = 0; i < _inventoryInterfaceState.GrabbedItems.Count; i++) {
                    _inventoryInterfaceState.GrabbedItems[i].transform.position = _inventoryInterfaceState.LastGrabbedItemsLocalPosition[i] + _inventoryInterfaceState.HitPoint.point
                        + Vector3.up * 0.2f;
                }
            }
            CheckSwitchStates();
        }
        protected sealed override void EnterState() {
            _inventoryInterfaceState = (InventoryInterfaceState) CurrentSuperState;
        }
        protected override void ExitState() {
            if (_inventoryInterfaceState.GrabbedItems.Count > 0) {
                for (int i = 0; i < _inventoryInterfaceState.GrabbedItems.Count; i++) {
                    _inventoryInterfaceState.GrabbedItems[i].transform.position = _inventoryInterfaceState.GrabbedItems[i].transform.parent.position + 
                        _inventoryInterfaceState.LastGrabbedItemsLocalPosition[i];
                }
            }
            _inventoryInterfaceState.GrabbedItems.Clear();
        }
        public override void CheckSwitchStates() {
            if (InputHandler.Singleton.RClicked) {
                SwitchState(Factory.InventorySelectItemState());
            }
            if (InputHandler.Singleton.Clicked) {
                SwapOrDropItemInSlot();
            }
        }
        public override void InitializeSubState() {
        }
        private void SwapOrDropItemInSlot() {
            Transform clickedSlot = _inventoryInterfaceState.HitPoint.collider.transform;
            Inventory selectedInventory = itemSelected.GetInventory();
            if (selectedInventory.SwapItemsInInventory(selectedInventory, itemSelected.GetSlotID(), Int32.Parse(clickedSlot.name))) {
                InputHandler.Singleton.Clicked = false;
                SwitchState(Factory.InventorySelectItemState());
            }
        }
        public override string StateName() => "GrabItemState";
    }
}