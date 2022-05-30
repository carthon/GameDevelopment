using System;
using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.Handlers;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace _Project.Scripts.UI {
    public class UIInventoryPanel : UIPanelsBase {
        private Inventory inventory;
        [Header("Header")]
        [SerializeField] private Transform headerArea;
        [SerializeField] private TextMeshProUGUI titleField;
        [Header("Body")]
        [SerializeField] private Transform inventoryHolder;
        [SerializeField] private GameObject itemSlotPrefab;
        private List<UIItemSlot> uiSlots;

        public void CreateInventoryUI() {
            if (inventory.GetInventorySlots() != null)
                uiSlots = new List<UIItemSlot>();
            for (int i = 0; i < inventory.Size; i++) {
                UIItemSlot uiItemSlot = Instantiate(itemSlotPrefab, inventoryHolder).GetComponent<UIItemSlot>();
                uiItemSlot.SetData(new ItemStack(inventory, i));
                uiItemSlot.SetParent(this);
                uiItemSlot.SlotID = i;
                uiSlots.Add(uiItemSlot);
                uiItemSlot.OnItemClicked += HandleItemSelection;
            }
        }
        private void OnEnable() {
        }

        private void OnDisable() {
            //DeSubscribeEvents();
        }
        private void SubscribeEvents() {
            inventory.OnItemAddedToSlot += HandleNewItem;
            inventory.OnItemTakenFromSlot += HandleItemTaken;
        }
        private void DeSubscribeEvents() {
            inventory.OnItemAddedToSlot -= HandleNewItem;
            //inventory.OnItemTakenFromSlot -= HandleItemTaken;
        }
        protected override void HandleItemSelection(UIItemSlot itemSlot) {
            base.HandleItemSelection(itemSlot);
        }
        public override void HandleSwap(UIItemSlot obj) {
            UIItemSlot draggedItem = UIHandler.instance.mouseFollower.GetData();
            // Item que va a ser sustituido
            int newSlotIndex = obj.SlotID;
            // Item agarrado
            int lastSlotIndex = uiSlots.FindIndex(uiSlot => uiSlot.GetItemStack().Equals(draggedItem.GetItemStack()));
            if (newSlotIndex == -1 || lastSlotIndex == -1) return;
            SwapSlotItems(obj, uiSlots[lastSlotIndex]);
        }
        private void SwapSlotItems(UIItemSlot grabbedItemSlot, UIItemSlot otherItemSlot) {
            ItemStack grabbedItem = grabbedItemSlot.GetItemStack();
            ItemStack otherItem = otherItemSlot.GetItemStack();
            grabbedItem.GetInventory().SwapItemsInInventory(otherItem.GetInventory(), grabbedItem, otherItem);
            grabbedItemSlot.SetData(grabbedItem);
            otherItemSlot.SetData(otherItem);
        }
        private void HandleNewItem(int slot) {
            ItemStack item = inventory.GetItem(slot);
            uiSlots[slot].SetData(item);
            SyncInventorySlot(slot);
        }
        private void HandleItemTaken(int slot) {
            uiSlots[slot].ResetData();
            SyncInventorySlot(slot);
        }

        public void SyncInventoryToUI() {
            int i = 0;
            foreach (UIItemSlot uiItemSlot in uiSlots) {
                uiItemSlot.ResetData();
                ItemStack itemStack = inventory.GetItem(i);
                itemStack.SetInventory(inventory);
                itemStack.SetSlot(i);
                if (!itemStack.IsEmpty()) 
                    uiItemSlot.SetData(itemStack);
                i++;
            }
        }
        public void SyncInventorySlot(int slot) {
            ItemStack itemStack = inventory.GetItem(slot);
            uiSlots[slot].SetData(itemStack);
        }

        public void Display(bool display) {
            gameObject.SetActive(display);
        }
        public void SetInventory(Inventory newInventory) {
            inventory = newInventory;
            SubscribeEvents();
        }

        public Inventory GetInventory() => inventory;
    }
}