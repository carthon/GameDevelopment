using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.Handlers;
using UnityEngine;

namespace _Project.Scripts.UI {
    public partial class UIHotbarPanel : UIPanelsBase {
        [SerializeField] private Transform hotbarTransform;
        [SerializeField] private Transform handsGroup;
        [SerializeField] private List<UIEquipmentSlot> equipmentSlots;
        private List<UIHotbarSlot> hotbarSlots;
        public int activeSlot = 0;
        
        protected override void Start() {
            base.Start();
            hotbarSlots = hotbarTransform.GetComponentsInChildren<UIHotbarSlot>().ToList();
            equipmentSlots = handsGroup.GetComponentsInChildren<UIEquipmentSlot>().ToList();
            int i = 0;
            foreach (UIHotbarSlot uiItemSlot in hotbarSlots) {
                uiItemSlot.SlotID = i;
                uiItemSlot.SetParent(this);
                uiItemSlot.OnItemClicked += HandleItemSelection;
                i++;
            }
            foreach (UIEquipmentSlot uiEquipmentSlot in equipmentSlots) {
                Debug.Log("Setting equipment to " + uiEquipmentSlot.GetEquipmentSlot().GetBodyPart() + "\nto UIElement: " + uiEquipmentSlot);
                equipmentSlotHandlers[uiEquipmentSlot.GetEquipmentSlot().GetBodyPart()].SetParent(uiEquipmentSlot);
            }
            hotbarSlots[activeSlot].Select();
        }

        public override void HandleSwap(UIItemSlot obj) {
            UIItemSlot draggedItemSlot = UIHandler.instance.mouseFollower.GetData();
            ItemStack draggedItem = draggedItemSlot.GetItemStack();
            bool itemSwaped = false;
            Debug.Log("Handling Swap with" + obj);
            if (!draggedItem.IsEmpty()) {
                int index = obj.SlotID;
                int indexInHotbar = hotbarSlots.FindIndex(itemPerSlot => draggedItem.Equals(itemPerSlot.GetItemStack()));
                if (!obj.IsEmpty()) {
                    hotbarSlots[draggedItemSlot.SlotID].SetData(obj.GetItemStack());
                    itemSwaped = true;
                }
                if (!itemSwaped && indexInHotbar != -1) {
                    hotbarSlots[indexInHotbar].ResetData();
                }
                hotbarSlots[index].SetData(draggedItem);
            }
        }
        public void UseItem(int hotbarInput, bool isLeft) {
            BodyPart bodyPart = isLeft ? BodyPart.LEFT_HAND : BodyPart.RIGHT_HAND;
            int hotbarSlotIndex = hotbarInput != -1 ? hotbarInput : activeSlot;
            EquipmentSlotHandler equipmentSlotHandler = equipmentSlotHandlers[bodyPart];
            if (hotbarSlots[hotbarSlotIndex].GetItemStack().Item == null && equipmentSlotHandler == null)
                return;
            hotbarSlots[activeSlot].Deselect();
            hotbarSlots[hotbarSlotIndex].Select();
            ItemStack itemStack = hotbarSlots[hotbarSlotIndex].GetItemStack();
            if (itemStack == null)
                return;
            Item itemToUse = itemStack.Item;
            if (isLeft) bodyPart = BodyPart.LEFT_HAND;
            if (itemToUse.GetType() == typeof(Wereable)) bodyPart = ((Wereable) itemToUse).GetBodyPart();
            UIEquipmentSlot equipmentSlot = equipmentSlots.Find(slot => slot.GetEquipmentSlot().GetBodyPart() == bodyPart);
            if (!itemStack.IsEmpty()){
                equipmentSlot.EquipItem(itemStack, equipmentSlotHandler);
            } else if (equipmentSlotHandler.currentInventory.GetItem(0).Equals(itemStack)) {
                equipmentSlot.UnEquipItem(equipmentSlotHandler);
            } else {
                SwapEquipmentSlots(equipmentSlotHandlers[bodyPart], equipmentSlotHandlers[BodyPart.RIGHT_HAND]);
            }
            activeSlot = hotbarSlotIndex;
        }
        private void SwapEquipmentSlots(EquipmentSlotHandler equipmentSlotHandler, EquipmentSlotHandler otherEquipmentSlotHandler) {
            ItemStack itemStack = equipmentSlotHandler.currentInventory.GetItem(0);
            ItemStack otherItemStack = otherEquipmentSlotHandler.currentInventory.GetItem(0);
            equipmentSlotHandler.currentInventory.SwapItemsInInventory(otherEquipmentSlotHandler.currentInventory, itemStack, otherItemStack);
        }

        public UIHotbarSlot GetItemHolderInSlot(int slot) => hotbarSlots[slot];

        public ItemStack GetItemStackInSlot(int slot) => hotbarSlots[slot].GetItemStack();
        public int GetSlotFromItemHolder(UIHotbarSlot itemHolder) => hotbarSlots.IndexOf(itemHolder);
        public List<UIEquipmentSlot> GetEquipmentSlots() => equipmentSlots;
    }
}