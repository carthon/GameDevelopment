using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.Client;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState {
    public class InventoryGrabItemInterfaceState : InterfaceAbstractBaseState {
        private InventoryInterfaceState _inventoryInterfaceState;
        private bool oringinalyFlipped;
        private bool cellIndicatorUpdate;
        private int width;
        private int height;
        public InventoryGrabItemInterfaceState(InventorySlot grabbedItem, InterfaceStateFactory factory, ContainerRenderer context) : base(factory, context) {
            itemSelected = grabbedItem;
        }
        private void RotateItem() {
            for (int i = 0; i < _inventoryInterfaceState?.GrabbedItemsCollider?.Count; i++) {
                _inventoryInterfaceState.GrabbedItemsCollider[i].transform.Rotate(0, oringinalyFlipped ? 90 : -90, 0);
                oringinalyFlipped = !oringinalyFlipped;
            }
            cellIndicatorUpdate = true;
        }
        protected override void UpdateState() {
            if (_inventoryInterfaceState.GrabbedItemsCollider.Count > 0) {
                Item item = itemSelected.ItemStack.Item;
                if (cellIndicatorUpdate) {
                    width = oringinalyFlipped ? item.Width : item.Height;
                    height = oringinalyFlipped ? item.Height : item.Width;
                    UIHandler.Instance.inventoryCellIndicatorMeshFilter.mesh = MeshUtils.CreateFrameMesh(width, height, Context.inventoryGrid.cellSize.x, 0.05f);
                    cellIndicatorUpdate = false;
                }
                Vector3Int gridPosition = Context.inventoryGrid.WorldToCell(_inventoryInterfaceState.ProjectedPositionOverPlane);
                Vector2Int selectedSlot = new Vector2Int(gridPosition.x, -gridPosition.z);
                bool validSwap = Context.Inventory.ValidateSwap(
                    itemSelected.ItemStack.GetInventory(),
                    itemSelected.ItemStack.OriginalSlot,
                    selectedSlot,
                    oringinalyFlipped,
                    out List<InventorySlot> collidingSlots
                );
                StringBuilder colliding = new StringBuilder();
                foreach (InventorySlot collidingSlot in collidingSlots) {
                    colliding.Append(collidingSlot.ItemStack.OriginalSlot.ToString());
                }
                UIHandler.Instance.UpdateWatchedVariables("GrabItems", $"SelectedItem:{_inventoryInterfaceState.GrabbedItemsCollider[0].name} " +
                    $"OccupiedSlots: {itemSelected.ItemStack.OriginalSlot} IsFlipped: {itemSelected.IsFlipped} IsValidSwap: {validSwap} {colliding.ToString()}");
                if (validSwap) {
                    UIHandler.Instance.inventoryCellIndicatorMeshRenderer.sharedMaterial.color = Context.okIndicator;
                } else if (!Context.Inventory.IsValidSlot(selectedSlot)) {
                    UIHandler.Instance.inventoryCellIndicatorMeshRenderer.sharedMaterial.color = Context.dropIndicator;
                } else {
                    UIHandler.Instance.inventoryCellIndicatorMeshRenderer.sharedMaterial.color = Context.errorIndicator;
                }
                if (_inventoryInterfaceState.RayCastHit.collider is not null) {
                    bool lookingAtSlotOccupiedSlot = _inventoryInterfaceState.RayCastHit.collider.CompareTag(global::Constants.TAG_UISLOT);
                    if (Context.renderedItems.TryGetValue(_inventoryInterfaceState.RayCastHit.collider.gameObject, out Vector2Int slot)) {
                        InventorySlot inventorySlot = Context.Inventory.GetInventorySlot(slot);
                        item = Context.Inventory.GetInventorySlot(slot).ItemStack.Item;
                        width = inventorySlot.IsFlipped ? item.Width : item.Height;
                        height = inventorySlot.IsFlipped ? item.Height : item.Width;
                        
                        //UIHandler.Instance.inventoryCellIndicatorMeshFilter.mesh = MeshUtils.CreateFrameMesh(width, height, Context.inventoryGrid.cellSize.x, 0.05f);
                    }
                    UIHandler.Instance.inventoryCellIndicator.SetActive(lookingAtSlotOccupiedSlot);
                }
                for (int i = 0; i < _inventoryInterfaceState?.GrabbedItemsCollider?.Count; i++) {
                    _inventoryInterfaceState.GrabbedItemsCollider[i].transform.position = _inventoryInterfaceState.ProjectedPositionOverPlane + 
                        Vector3.up * 0.2f;
                }
            }
            CheckSwitchStates();
        }
        protected sealed override void EnterState() {
            _inventoryInterfaceState = (InventoryInterfaceState) CurrentSuperState;
            cellIndicatorUpdate = true;
            InputHandler.Singleton.OnItemRotation += RotateItem;
            if (_inventoryInterfaceState.GrabbedItemsCollider.Count > 0) {
                for (int i = 0; i < _inventoryInterfaceState?.GrabbedItemsCollider?.Count; i++) {
                    _inventoryInterfaceState.GrabbedItemsCollider[i].enabled = false;
                }
            }
            oringinalyFlipped = itemSelected.IsFlipped;
            UIHandler.Instance.inventoryCellIndicator.SetActive(true);
        }
        protected override void ExitState() {
            InputHandler.Singleton.OnItemRotation -= RotateItem;
            if (_inventoryInterfaceState is not null) {
                if (_inventoryInterfaceState.GrabbedItemsCollider.Count > 0) {
                    for (int i = 0; i < _inventoryInterfaceState?.GrabbedItemsCollider?.Count; i++) {
                        _inventoryInterfaceState.GrabbedItemsCollider[i].transform.localPosition = _inventoryInterfaceState.LastGrabbedItemsLocalPosition[i];
                        _inventoryInterfaceState.GrabbedItemsCollider[i].enabled = true;
                    }
                }
                UIHandler.Instance.inventoryCellIndicator.SetActive(false);
                _inventoryInterfaceState.HitMouseOutline = null;
                _inventoryInterfaceState.GrabbedItemsCollider?.Clear();
                _inventoryInterfaceState.LastGrabbedItemsLocalPosition?.Clear();
            }
            InputHandler.Singleton.Clicked = false;
        }
        public override void CheckSwitchStates() {
            if (InputHandler.Singleton.RClicked) {
                if (oringinalyFlipped != itemSelected.IsFlipped)
                    RotateItem();
                SwitchState(Factory.InventorySelectItemState());
            }
            if (InputHandler.Singleton.Clicked) {
                SwapOrDropItemInSlot();
            }
        }
        public override void InitializeSubState() {
        }
        private void SwapOrDropItemInSlot() {
            bool switchState = true;
            if (!itemSelected.ItemStack.IsEmpty() && _inventoryInterfaceState.GrabbedItemsCollider.Count > 0) {
                Vector3Int gridPosition = Context.inventoryGrid.WorldToCell(_inventoryInterfaceState.ProjectedPositionOverPlane);
                Vector2Int gridSlot = new Vector2Int(gridPosition.x, gridPosition.z * -1);
                if (Context.Inventory.IsValidSlot(gridSlot)) {
                    Inventory selectedInventory = itemSelected.ItemStack.GetInventory();
                    switchState = selectedInventory.SwapItemsInInventory(selectedInventory, itemSelected.ItemStack.OriginalSlot, gridSlot, oringinalyFlipped);
                }
                else {
                    Inventory inventory = itemSelected.ItemStack.GetInventory();
                    inventory.InventoryManager.DropItemStack(inventory.Id, gridSlot,
                        _inventoryInterfaceState.ProjectedPositionOverPlane + Vector3.up * 0.4f, Quaternion.identity);
                    _inventoryInterfaceState.GrabbedItemsCollider.Clear();
                    _inventoryInterfaceState.LastGrabbedItemsLocalPosition.Clear();
                }
            }
            if(switchState) SwitchState(Factory.InventorySelectItemState());
        }
        public override string StateName() => "GrabItemState";
    }
}