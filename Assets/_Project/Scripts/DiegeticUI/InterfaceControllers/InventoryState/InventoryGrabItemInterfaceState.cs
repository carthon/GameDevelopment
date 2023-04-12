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
        private Transform originalParent;
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
                Transform hitPointCollider = _inventoryInterfaceState.HitPoint.collider.transform;
                if (lookingAtSlot) {
                    Context.slotSelectionVisualizer.transform.position = hitPointCollider.position;
                    Context.slotSelectionVisualizer.transform.rotation = hitPointCollider.rotation;
                }
                Context.itemGrabberTransform.transform.position = _inventoryInterfaceState.HitPoint.point + Vector3.up * 0.2f;
            }
            CheckSwitchStates();
        }
        protected sealed override void EnterState() {
            _inventoryInterfaceState = (InventoryInterfaceState) CurrentSuperState;
            Context.slotSelectionVisualizer.SetActive(true);
            Context.slotSelectionVisualizer.transform.localScale = _inventoryInterfaceState?.GrabbedItems?.Count > 0 ? _inventoryInterfaceState.GrabbedItems[0].GetComponentInChildren<Renderer>().bounds.extents * 2
                    : Vector3.one *  ContainerRenderer.Singleton.slotSize;
            originalParent = _inventoryInterfaceState?.GrabbedItems?.Count > 0 ? _inventoryInterfaceState.GrabbedItems[0].parent : Context.itemGrabberTransform;
            for (int i = 0; i < _inventoryInterfaceState?.GrabbedItems?.Count; i++) {
                _inventoryInterfaceState.GrabbedItems[i].SetParent(Context.itemGrabberTransform);
                _inventoryInterfaceState.GrabbedItems[i].localPosition = Vector3.zero + _inventoryInterfaceState.LastGrabbedItemsLocalPosition[i];
            }
        }
        protected override void ExitState() {
            if (_inventoryInterfaceState.GrabbedItems.Count > 0) {
                for (int i = 0; i < _inventoryInterfaceState?.GrabbedItems?.Count; i++) {
                    if (InputHandler.Singleton.Clicked)
                        GameObject.Destroy(Context.itemGrabberTransform.GetChild(i).gameObject);
                    else {
                        _inventoryInterfaceState.GrabbedItems[i].SetParent(originalParent);
                        _inventoryInterfaceState.GrabbedItems[i].localPosition = _inventoryInterfaceState.LastGrabbedItemsLocalPosition[i];
                    }
                }
            }
            Context.slotSelectionVisualizer.SetActive(false);
            _inventoryInterfaceState.GrabbedItems.Clear();
            InputHandler.Singleton.Clicked = false;
        }
        public override void CheckSwitchStates() {
            if (InputHandler.Singleton.Clicked) {
                SwapOrDropItemInSlot();
            }
            if (InputHandler.Singleton.RClicked || InputHandler.Singleton.Clicked) {
                Logger.Singleton.Log($"{itemSelected.Item.name}: Inventory:{itemSelected.GetInventory().Name} Slot:{itemSelected.GetSlotID()}", Logger.Type.DEBUG);
                SwitchState(Factory.InventorySelectItemState());
            }
        }
        public override void InitializeSubState() {
        }
        private void SwapOrDropItemInSlot() {
            Inventory selectedInventory = itemSelected.GetInventory();
            if (Int32.TryParse(_inventoryInterfaceState.HitPoint.collider.transform.name, out int clickedSlot)) {
                if (itemSelected.GetSlotID() == clickedSlot)
                    return;
                selectedInventory.SwapItemsInInventory(selectedInventory, itemSelected.GetSlotID(), clickedSlot);
            }
            else {
                Vector3 itemExtents = itemSelected.Item.modelPrefab.GetComponentInChildren<Renderer>().bounds.extents * 1.1f;
                Client.DropItemStack(itemSelected,_inventoryInterfaceState.HitPoint.point + Vector3.up * itemExtents.z, Quaternion.identity);
            }
            SwitchState(Factory.InventorySelectItemState());
        }
        public override string StateName() => "GrabItemState";
    }
}