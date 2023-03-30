using System;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.Client;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState {
    public class InventoryGrabItemInterfaceState : InterfaceAbstractBaseState {
        private InventoryInterfaceState _inventoryInterfaceState;
        private int _clickedSlot = -1;
        public InventoryGrabItemInterfaceState(ItemStack grabbedItem, InterfaceStateFactory factory, UIHandler context) : base(factory, context) {
            itemSelected = grabbedItem;
            EnterState();
        }
        protected override void UpdateState() {
            Context.UpdateWatchedVariables("grabbedItems", $"GrabbedItemsCount:{_inventoryInterfaceState.GrabbedItems.Count}");
            if (_inventoryInterfaceState.GrabbedItems.Count > 0) {
                bool lookingAtSlot = _inventoryInterfaceState.HitPoint.collider.CompareTag("UISlot");
                Context.slotSelectionVisualizer.SetActive(lookingAtSlot);
                if (lookingAtSlot) {
                    Transform hitPointCollider = _inventoryInterfaceState.HitPoint.collider.transform;
                    Context.slotSelectionVisualizer.transform.position = hitPointCollider.position;
                    Context.slotSelectionVisualizer.transform.rotation = hitPointCollider.rotation;
                }
                for (int i = 0; i < _inventoryInterfaceState.GrabbedItems.Count; i++) {
                    _inventoryInterfaceState.GrabbedItems[i].transform.position = _inventoryInterfaceState.LastGrabbedItemsLocalPosition[i] + _inventoryInterfaceState.HitPoint.point
                        + Vector3.up * 0.2f;
                }
            }
            CheckSwitchStates();
        }
        protected sealed override void EnterState() {
            _inventoryInterfaceState = (InventoryInterfaceState) CurrentSuperState;
            Context.slotSelectionVisualizer.SetActive(true);
            Context.slotSelectionVisualizer.transform.localScale = Vector3.one * ContainerRenderer.Singleton.slotSize;
            
        }
        protected override void ExitState() {
            if (_inventoryInterfaceState.GrabbedItems.Count > 0) {
                for (int i = 0; i < _inventoryInterfaceState.GrabbedItems.Count; i++) {
                    _inventoryInterfaceState.GrabbedItems[i].transform.position = _inventoryInterfaceState.GrabbedItems[i].transform.parent.position + 
                        _inventoryInterfaceState.LastGrabbedItemsLocalPosition[i];
                }
            }
            Context.slotSelectionVisualizer.SetActive(false);
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
            Inventory selectedInventory = itemSelected.GetInventory();
            if (Int32.TryParse(_inventoryInterfaceState.HitPoint.collider.transform.name, out int clickedSlot)) {
                selectedInventory.SwapItemsInInventory(selectedInventory, itemSelected.GetSlotID(), clickedSlot);
            }
            else {
                Client.DropItemStack(itemSelected,_inventoryInterfaceState.HitPoint.point, Quaternion.identity);
            }
            InputHandler.Singleton.Clicked = false;
            SwitchState(Factory.InventorySelectItemState());
        }
        public override string StateName() => "GrabItemState";
    }
}