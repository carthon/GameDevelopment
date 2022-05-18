using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI {
    public class UIInventoryPanel: MonoBehaviour {
        private Inventory inventory;
        private List<UIInventoryItem> inventorySlotsUI;
        [Header("Header")]
        [SerializeField] private Transform headerArea;
        [SerializeField] private TextMeshProUGUI titleField;
        [Header("Body")]
        [SerializeField] private Transform inventoryHolder;
        [SerializeField] private GameObject itemSlotPrefab;
        
        private Item draggedItem;

        public void CreateInventoryUI() {
            if (inventory.GetInventorySlots() != null)
                inventorySlotsUI = new List<UIInventoryItem>();
            for (int i = inventory.Count; i < inventory.Size; i++) {
                inventory.OnItemAddedToSlot += HandleNewItem;
                UIInventoryItem itemSlot = Instantiate(itemSlotPrefab, inventoryHolder).GetComponent<UIInventoryItem>();
                inventorySlotsUI.Add(itemSlot);
                itemSlot.OnItemClicked += HandleItemSelection;
                itemSlot.OnItemBeginDrag += HandleBeginDrag;
                itemSlot.OnItemEndDrag += HandleEndDrag;
                itemSlot.OnItemDroppedOn += HandleSwap;
                itemSlot.OnRightMouseBtnClick += HandleShowItemActions;
            }
        }
        private void HandleItemSelection(UIInventoryItem itemSlot) {
            Debug.Log(itemSlot.name);
        }
        private void HandleShowItemActions(UIInventoryItem obj) {
        }
        private void HandleSwap(UIInventoryItem obj) {
            Debug.Log("Dropped on " + obj.GetAsignedItem());
            // Item que va a ser sustituido
            int swapSlotIndex = inventorySlotsUI.IndexOf(obj);
            // Item agarrado
            int lastSlotIndex = inventorySlotsUI.FindIndex(slot => slot.GetAsignedItem() == draggedItem);
            if (swapSlotIndex == -1) return;

            inventorySlotsUI[lastSlotIndex].SetData(inventorySlotsUI[swapSlotIndex].GetAsignedItem());
            inventorySlotsUI[swapSlotIndex].SetData(draggedItem); ;
        }
        private void HandleEndDrag(UIInventoryItem obj) {
            MouseFollower mouseFollower = UIHandler.instance.mouseFollower;
            mouseFollower.Toggle(false);
        }
        private void HandleBeginDrag(UIInventoryItem obj) {
            MouseFollower mouseFollower = UIHandler.instance.mouseFollower;
            mouseFollower.Toggle(true);
            if (obj.GetAsignedItem() != null) {
                mouseFollower.SetData(obj.GetAsignedItem());
                draggedItem = obj.GetAsignedItem();
            }
            else
                Debug.Log("Couldnt grab an empty object!");
        }
        private void HandleNewItem(int slot) {
            Item item = inventory.GetItem(slot);
            inventorySlotsUI[slot].SetData(item);
        }

        public void Display(bool display) {
            gameObject.SetActive(display);
        }
        public void SetInventoryPanel(Inventory inventory) {
            this.inventory = inventory;
        }
    }
}