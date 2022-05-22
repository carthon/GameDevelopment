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
        public event Action<UIItemSlot> OnInventoryEquipItem;
        public int activeSlot = 0;
        private void Awake() {
            uiSlots = hotbarTransform.GetComponentsInChildren<UIItemSlot>().ToList();
            equipmentSlots = handsGroup.GetComponentsInChildren<UIItemSlot>().ToList();
            foreach (UIItemSlot uiItemSlot in uiSlots) {
                uiItemSlot.OnItemClicked += HandleItemSelection;
                uiItemSlot.OnItemBeginDrag += HandleBeginDrag;
                uiItemSlot.OnItemEndDrag += HandleEndDrag;
                uiItemSlot.OnRightMouseBtnClick += HandleShowItemActions;
            }
            uiSlots[activeSlot].Select();
        }
        protected override void HandleSwap(UIItemSlot obj) {
            draggedItemSlot = UIHandler.instance.draggedItem;
            ItemStack draggedItem = draggedItemSlot.GetItemStack();
            bool itemSwaped = false;
            Debug.Log("Handling Swap with" + obj);
            if (draggedItemSlot != null) {
                int index = uiSlots.IndexOf(obj);
                int indexDragged = uiSlots.FindIndex(itemPerSlot => draggedItem == itemPerSlot.GetItemStack());
                if (uiSlots[index] != null && draggedItemSlot != null) {
                    if (!uiSlots[index].IsEmpty() && indexDragged != -1) {
                        uiSlots[indexDragged].SetData(obj);
                        itemSwaped = true;
                    }
                    uiSlots[index].SetData(draggedItemSlot);
                    if (indexDragged != -1 && !itemSwaped) {
                        uiSlots[indexDragged].ResetData();
                    }
                }
                if (activeSlot == index)
                    OnInventoryEquipItem?.Invoke(obj);
            }
        }
        public void UpdateHotbarSlot(ItemStack itemStack) {
            //uiSlots[index].SetData();
        }

        public UIItemSlot GetItemInSlot(int slot) => uiSlots[slot];

        public void UpdateEquippedItem(UIItemSlot slot, EquipmentSlotHandler equipmentSlot) {
            if (equipmentSlot != null) {
                BodyPart bodyPart = equipmentSlot.GetEquipmentBodyPart();
                if (bodyPart == BodyPart.LEFT_HAND || bodyPart == BodyPart.RIGHT_HAND) {
                    equipmentSlots[(int)bodyPart].SetData(slot);
                    slot.PullItemStack();
                }
            }
        }
        public int GetSlotFromItem(UIItemSlot item) => uiSlots.IndexOf(item);
    }
}