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
        private UIItemSlot handSlot;
        public event Action<UIItemSlot> OnInventoryEquipItem;
        private int activeSlot = 0;
        private void Awake() {
            uiSlots = hotbarTransform.GetComponentsInChildren<UIItemSlot>().ToList();

            foreach (UIItemSlot uiItemSlot in uiSlots) {
                uiItemSlot.OnItemClicked += HandleItemSelection;
                uiItemSlot.OnItemBeginDrag += HandleBeginDrag;
                uiItemSlot.OnItemEndDrag += HandleEndDrag;
                uiItemSlot.OnItemDroppedOn += HandleSwap;
                uiItemSlot.OnRightMouseBtnClick += HandleShowItemActions;
            }
        }
        public void HandleClick() {
            Debug.Log("Test");
        }
        protected override void HandleSwap(UIItemSlot obj) {
            draggedItem = UIHandler.instance.draggedItem;
            int index = uiSlots.IndexOf(obj);
            if (uiSlots[index] != null && draggedItem != null)
                uiSlots[index].SetData(draggedItem.GetItemStack());
        }
        protected override void HandleItemSelection(UIItemSlot itemSlot) {
            uiSlots[activeSlot].Deselect();
            itemSlot.Select();
            activeSlot = uiSlots.IndexOf(itemSlot);
            OnInventoryEquipItem?.Invoke(itemSlot);
        }
        public void UpdateHotbarSlot(ItemStack itemStack) {
            //uiSlots[index].SetData();
        }

        public void UpdateEquippedItem(EquipmentSlotHandler slot) {
            Item item = slot.currentItemOnSlot;
            //if (slot.isLeftHandSlot)
                //handSlots[0].sprite = item.itemIcon;
            //else
                //handSlots[1].sprite = item.itemIcon;
        }
    }
}