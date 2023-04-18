using System;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.Client;
using Unity.Mathematics;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;
using Object = UnityEngine.Object;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState {
    public class InventoryGrabItemInterfaceState : InterfaceAbstractBaseState {
        private InventoryInterfaceState _inventoryInterfaceState;
        public InventoryGrabItemInterfaceState(ItemStack grabbedItem, InterfaceStateFactory factory, UIHandler context) : base(factory, context) {
            itemSelected = grabbedItem;
        }
        protected override void UpdateState() {
            if (_inventoryInterfaceState.GrabbedItems.Count > 0) {
                bool lookingAtSlot = _inventoryInterfaceState.HitPoint.collider.CompareTag("UISlot");
                Context.slotSelectionVisualizer.SetActive(lookingAtSlot);
                Transform hitPointCollider = _inventoryInterfaceState.HitPoint.collider.transform;
                if (lookingAtSlot) {
                    Context.slotSelectionVisualizer.transform.position = hitPointCollider.position;
                    Context.slotSelectionVisualizer.transform.rotation = hitPointCollider.rotation;
                }
                for (int i = 0; i < _inventoryInterfaceState?.GrabbedItems?.Count; i++) {
                    _inventoryInterfaceState.GrabbedItems[i].position = _inventoryInterfaceState.HitPoint.point + _inventoryInterfaceState.LastGrabbedItemsLocalPosition[i] 
                        + Vector3.up * 0.2f;
                }
            }
            CheckSwitchStates();
        }
        protected sealed override void EnterState() {
            _inventoryInterfaceState = (InventoryInterfaceState) CurrentSuperState;
            Context.slotSelectionVisualizer.SetActive(true);
            
            Context.slotSelectionVisualizer.transform.localScale = Vector3.one * ContainerRenderer.Singleton.slotSize;
            Context.itemGrabberTransform.transform.rotation = _inventoryInterfaceState?.GrabbedItems?.Count > 0 ? _inventoryInterfaceState.GrabbedItems[0].rotation : Quaternion.identity;
        }
        protected override void ExitState() {
            if (_inventoryInterfaceState.GrabbedItems.Count > 0) {
                for (int i = 0; i < _inventoryInterfaceState?.GrabbedItems?.Count; i++) {
                    _inventoryInterfaceState.GrabbedItems[i].localPosition = _inventoryInterfaceState.LastGrabbedItemsLocalPosition[i];
                }
            }
            Context.slotSelectionVisualizer.SetActive(false);
            _inventoryInterfaceState.GrabbedItems.Clear();
            InputHandler.Singleton.Clicked = false;
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
            bool switchState = true;
            if (Int32.TryParse(_inventoryInterfaceState.HitPoint.collider.transform.name, out int clickedSlot)) {
                if (itemSelected.GetSlotID() != clickedSlot) {
                    switchState = selectedInventory.SwapItemsInInventory(selectedInventory, itemSelected.GetSlotID(), clickedSlot);
                    InputHandler.Singleton.Clicked = false;
                }
            }
            else {
                Client.DropItemStack(itemSelected,_inventoryInterfaceState.HitPoint.point, Quaternion.identity);
            }
            if(switchState) SwitchState(Factory.InventorySelectItemState());
        }
        public override string StateName() => "GrabItemState";
    }
}