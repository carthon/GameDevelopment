using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Components.Items;
using _Project.Scripts.UI;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class Inventory {
        private List<ItemStack> items;

        public event Action<int> OnItemAddedToSlot;
        public event Action<int> OnItemPulledFromSlot;
        public string Name { get; set; }
        public int Size { get; private set; }

        public Inventory(string name, int size) {
            Name = name;
            Size = size;
            Init();
        }

        public Inventory(int size) {
            Name = "No Name";
            Size = size;
            Init();
        }

        private void Init() {
            items = new List<ItemStack>();
        }

        public ItemStack GetItem(int slot) {
            ItemStack item = null;
            if (slot >= 0 && slot < items.Count && items[slot] != null)
                item = items[slot];
            return item;
        }

        public bool AddItemToSlot(ItemStack itemStack, int slot) {
            bool success = false;
            ItemStack itemInInventory = items.Find(item => itemStack.Item == item.Item);
            if (itemInInventory != null) {
                if ((int) itemInInventory.Size < itemInInventory.Count) {
                    itemInInventory.Count += itemStack.Count;
                    success = true;
                }
            }
            else {
                if (items.Count == 0)
                    items.Add(itemStack);
                else
                    items.Insert(slot, itemStack);
                success = true;
            }
            OnItemAddedToSlot?.Invoke(slot);
            return success;
        }
        public bool AddItemToSlot(Item item, int slot) {
            ItemStack itemStack = new ItemStack(item);
            itemStack.Count++;
            return AddItemToSlot(itemStack, slot);
        }

        public bool AddItem(Item item) {
            ItemStack itemStack = new ItemStack(item);
            itemStack.Count++;
            return AddItemToSlot(item, 0);
        }
        public bool AddItem(ItemStack item) {
            return AddItemToSlot(item, 0);
        }

        public bool AddToInventory(ItemStack itemStack, Inventory inventory) {
            bool success = inventory.AddItem(itemStack);
            if (success) {
                success = PullItem(itemStack) !=null;
            }
            return success;
        }

        public ItemStack PullItem(ItemStack itemStack) {
            int indexOf = items.IndexOf(itemStack);
            return PullItem(indexOf);
        }
        
        public ItemStack PullItem(int slot) {
            ItemStack itemStack = null;
            if (slot >= 0 && slot < Size) {
                itemStack = items[slot];
                items[slot].Count--;
                if (items[slot].Count <= 0)
                    items.RemoveAt(slot);
                OnItemPulledFromSlot?.Invoke(slot);
            }
            return itemStack;
        }
        public List<ItemStack> GetInventorySlots() => items;

        public int GetItemsCount() => items.Count;
    }
}