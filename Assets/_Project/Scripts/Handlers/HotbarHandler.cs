using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.UI;
using Unity.Barracuda;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class HotbarHandler : MonoBehaviour {
        [SerializeField] private EquipmentSlotHandler leftHandSlot;
        [SerializeField] private EquipmentSlotHandler rightHandSlot;
        [SerializeField] private int hotbarSize = 10;
        private List<ItemSlot> hotbarItems;

        public event Action<ItemSlot> OnItemChanged;
        public event Action<EquipmentSlotHandler> OnItemEquiped;
        
        public int activeSlot;

        private void Start() {
            EquipmentSlotHandler[] itemSlotHandlers = GetComponentsInChildren<EquipmentSlotHandler>();
            activeSlot = -1;
            foreach (EquipmentSlotHandler itemSlotHandler in itemSlotHandlers) {
                if (itemSlotHandler.isLeftHandSlot)
                    leftHandSlot = itemSlotHandler;
                if (itemSlotHandler.isRightHandSlot)
                    rightHandSlot = itemSlotHandler;
            }
            hotbarItems = new List<ItemSlot>();
            UIHandler.instance.LoadHotbarUI(this);
        }
        
        public void LoadWeaponOnSlot(Item item, bool isLeft) {
            EquipmentSlotHandler equipmentSlot;
            if (isLeft)
                equipmentSlot = leftHandSlot;
            else
                equipmentSlot = rightHandSlot;
            equipmentSlot.LoadItemModel(item);
            OnItemEquiped?.Invoke(equipmentSlot);
        }

        public Item GetActiveItem() {
            Item item = null;
            if (activeSlot != -1)
                item = hotbarItems[activeSlot].Item;
            return item;
        }
        
        

        public void LinkItemFromInventory(ItemSlot itemSlot) {
            hotbarItems.Add(itemSlot);
            OnItemChanged?.Invoke(itemSlot);
        }

        public List<ItemSlot> GetItemsInHotbar() => hotbarItems;
    }
}
