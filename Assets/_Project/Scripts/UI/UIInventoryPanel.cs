using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.Handlers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI {
    public class UIInventoryPanel: UIItemSlotEventHandler {
        private Inventory inventory;
        [Header("Header")]
        [SerializeField] private Transform headerArea;
        [SerializeField] private TextMeshProUGUI titleField;
        [Header("Body")]
        [SerializeField] private Transform inventoryHolder;
        [SerializeField] private GameObject itemSlotPrefab;
        
        private Item draggedItem;

        public void CreateInventoryUI() {
            if (inventory.GetInventorySlots() != null)
                uiSlots = new List<UIItemSlot>();
            for (int i = inventory.Count; i < inventory.Size; i++) {
                UIItemSlot uiItemSlot = Instantiate(itemSlotPrefab, inventoryHolder).GetComponent<UIItemSlot>();
                uiItemSlot.Parent = inventory;
                uiSlots.Add(uiItemSlot);
                inventory.OnItemAddedToSlot += HandleNewItem;
                uiItemSlot.OnItemClicked += HandleItemSelection;
                uiItemSlot.OnItemBeginDrag += HandleBeginDrag;
                uiItemSlot.OnItemEndDrag += HandleEndDrag;
                uiItemSlot.OnItemDroppedOn += HandleSwap;
                uiItemSlot.OnRightMouseBtnClick += HandleShowItemActions;
            }
        }
        private void HandleNewItem(int slot) {
            ItemStack item = inventory.GetItem(slot);
            uiSlots[slot].SetData(item);
        }

        public void Display(bool display) {
            gameObject.SetActive(display);
        }
        public void SetInventoryPanel(Inventory inventory) {
            this.inventory = inventory;
        }
    }
}