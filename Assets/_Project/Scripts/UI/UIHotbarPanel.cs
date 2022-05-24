using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.Handlers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI {
    public class UIHotbarPanel : UIItemSlotEventHandler {
        [SerializeField] private Transform hotbarTransform;
        [SerializeField] private Transform handsGroup;
        [SerializeField] private List<UIItemSlot> equipmentSlots;
        private UIItemSlot draggedItemSlot;
        public int activeSlot = 0;
        private void Awake() {
            uiSlots = hotbarTransform.GetComponentsInChildren<UIItemSlot>().ToList();
            equipmentSlots = handsGroup.GetComponentsInChildren<UIItemSlot>().ToList();
            int i = 0;
            foreach (UIItemSlot uiItemSlot in uiSlots) {
                uiItemSlot.OnItemClicked += HandleItemSelection;
                uiItemSlot.OnItemBeginDrag += HandleBeginDrag;
                uiItemSlot.OnItemEndDrag += HandleEndDrag;
                uiItemSlot.OnRightMouseBtnClick += HandleShowItemActions;
                uiItemSlot.SlotID = i;
                i++;
            }
            uiSlots[activeSlot].Select();
        }
        protected override void HandleSwap(UIItemSlot obj) {
            draggedItemSlot = UIHandler.instance.draggedItem;
            ItemStack draggedItem = draggedItemSlot.GetItemStack();
            bool itemSwaped = false;
            Debug.Log("Handling Swap with" + obj);
            if (!draggedItem.IsEmpty()) {
                int index = obj.SlotID;
                int indexInHotbar = uiSlots.FindIndex(itemPerSlot => draggedItem.Equals(itemPerSlot.GetItemStack()));
                if (!obj.IsEmpty()) {
                    uiSlots[draggedItemSlot.SlotID].SetData(obj.GetItemStack());
                    itemSwaped = true;
                }
                if (!itemSwaped && indexInHotbar != -1) {
                    uiSlots[indexInHotbar].ResetData();
                }
                uiSlots[index].SetData(draggedItem);
            }
        }
        public void UpdateHotbarSlot(ItemStack itemStack) {
            //uiSlots[index].SetData();
        }

        public UIItemSlot GetItemHolderInSlot(int slot) => uiSlots[slot];

        public ItemStack GetItemStackInSlot(int slot) => uiSlots[slot].GetItemStack ();
        public int GetSlotFromItemHolder(UIItemSlot itemHolder) => uiSlots.IndexOf(itemHolder);
        public List<UIItemSlot> GetEquipmentSlots() => equipmentSlots;
    }
}