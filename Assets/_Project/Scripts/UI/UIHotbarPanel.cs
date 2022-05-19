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
        [SerializeField] private PlayerManager playerManager;
        private UIItemSlot handSlot;
        private UIItemSlot draggedItem;
        public event Action<UIItemSlot> OnInventoryEquipItem;
        public int activeSlot = 0;
        private void Awake() {
            uiSlots = hotbarTransform.GetComponentsInChildren<UIItemSlot>().ToList();

            foreach (UIItemSlot uiItemSlot in uiSlots) {
                uiItemSlot.OnItemClicked += HandleItemSelection;
                uiItemSlot.OnItemBeginDrag += HandleBeginDrag;
                uiItemSlot.OnItemEndDrag += HandleEndDrag;
                uiItemSlot.OnRightMouseBtnClick += HandleShowItemActions;
            }
            uiSlots[activeSlot].Select();
        }
        protected override void HandleSwap(UIItemSlot obj) {
            draggedItem = UIHandler.instance.draggedItem;
            Debug.Log("Handling Swap with" + obj);
            if (draggedItem != null) {
                int index = uiSlots.IndexOf(obj);
                if (uiSlots[index] != null && draggedItem != null) {
                    uiSlots[index].SetData(draggedItem.GetItemStack());
                    UIHandler.instance.draggedItem = null;
                }
                if (activeSlot == index)
                    OnInventoryEquipItem?.Invoke(obj);
            }
        }
        public void UpdateHotbarSlot(ItemStack itemStack) {
            //uiSlots[index].SetData();
        }

        public UIItemSlot GetItemInSlot(int slot) => uiSlots[slot];

        public void UpdateEquippedItem(EquipmentSlotHandler slot) {
            Item item = slot.currentItemOnSlot;
            //if (slot.isLeftHandSlot)
                //handSlots[0].sprite = item.itemIcon;
            //else
                //handSlots[1].sprite = item.itemIcon;
        }
        public int GetSlotFromItem(UIItemSlot item) => uiSlots.IndexOf(item);
    }
}