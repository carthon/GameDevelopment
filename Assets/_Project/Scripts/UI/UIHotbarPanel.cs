using System;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.Handlers;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI {
    public class UIHotbarPanel : MonoBehaviour {
        [SerializeField] private Transform hotbarTransform;
        [SerializeField] private Transform handsTransform;
        private HotbarHandler hotbarHandler;
        private UIInventoryItem[] hotbarSlots;
        private UIInventoryItem[] handSlots;
        private void Awake() {
            hotbarSlots = hotbarTransform.GetComponentsInChildren<UIInventoryItem>();
            handSlots = handsTransform.GetComponentsInChildren<UIInventoryItem>();
        }
        public void HandleClick() {
            Debug.Log("Test");
        }

        public void SetHotbarHandler(HotbarHandler hotbar) {
            hotbarHandler = hotbar;
            hotbar.OnItemChanged += UpdateHotbarSlot;
            hotbar.OnItemEquiped += UpdateEquippedItem;
        }

        public void UpdateHotbarSlot(ItemSlot itemSlot) {
            int index = hotbarHandler.GetItemsInHotbar().FindIndex(item => itemSlot.Item == item.Item);
            hotbarSlots[index].SetData(itemSlot.Item);
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