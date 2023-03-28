using System;
using System.Collections.Generic;
using _Project.Scripts.DataClasses.ItemTypes;
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
        private void OnSelectedItem(Outline obj) {
            itemSelected = ContainerRenderer.Singleton.Inventory.GetItemStack(Int32.Parse(obj.transform.name));
            if (!itemSelected.IsEmpty()) {
                Context.UpdateWatchedVariables("mouseSelected", $"Selected:{itemSelected.Item.name}");
                _inventoryInterfaceState.GrabbedItems = new List<Transform>();
                _inventoryInterfaceState.LastGrabbedItemsLocalPosition = new List<Vector3>();
                for (int i = 0; i < obj.transform.childCount; i++) {
                    Transform child = obj.transform.GetChild(i);
                    _inventoryInterfaceState.GrabbedItems.Add(child);
                    _inventoryInterfaceState.LastGrabbedItemsLocalPosition.Add(child.localPosition);
                }
                InputHandler.Singleton.Clicked = false;
                SwitchState(Factory.InventoryGrabItemState(itemSelected));
            }
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
                OnSelectedItem(_inventoryInterfaceState.HitMouseOutline);
            }
        }
        public override string StateName() => "SelectItemState";
    }
}