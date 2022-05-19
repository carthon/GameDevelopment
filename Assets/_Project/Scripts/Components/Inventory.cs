using System;
using System.Collections.Generic;
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
        public int Count { get; private set; }

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
            items.AddRange(new ItemStack[Size]);
        }

        public ItemStack GetItem(int slot) {
            ItemStack item = null;
            if (items[slot] != null)
                item = items[slot];
            return item;
        }

        public void AddItem(Item item, int slot) {
            ItemStack itemStack = new ItemStack(item);
            items[slot] = itemStack;
            items[slot].Count++;
            Count++;
            OnItemAddedToSlot?.Invoke(slot);
        }
        public Item PullItem(int slot) {
            Item item = items[slot].Item;
            items[slot].Count--;
            if (items[slot].Count <= 0)
                items[slot] = null;
            Count--;
            OnItemPulledFromSlot?.Invoke(slot);
            return item;
        }
        public List<ItemStack> GetInventorySlots() => items;
        public void SwapSlotItems(UIItemSlot draggedItem, UIItemSlot otherItem) {
            int index = draggedItem.Parent.GetInventorySlots().IndexOf(draggedItem.GetItemStack());
            int otherIndex = otherItem.Parent.GetInventorySlots().IndexOf(otherItem.GetItemStack());
            otherItem.Parent.GetInventorySlots()[otherIndex] = draggedItem.GetItemStack();
            draggedItem.Parent.GetInventorySlots()[index] = otherItem.GetItemStack();
        }
    }
}