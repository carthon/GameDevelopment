using System;
using _Project.Scripts.Components.Items;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class Inventory {
        private ItemSlot[] items;

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
            items = new ItemSlot[Size];
        }

        public Item GetItem(int slot) {
            Item item = null;
            if (items[slot] != null)
                item = items[slot].Item;
            return item;
        }

        public void AddItem(Item item, int slot) {
            items[slot] = new ItemSlot(item, this, slot);
            Count++;
            OnItemAddedToSlot?.Invoke(slot);
        }
        public Item PullItem(int slot) {
            Item item = items[slot].Item;
            items[slot] = null;
            Count--;
            OnItemPulledFromSlot?.Invoke(slot);
            return item;
        }
        public ItemSlot[] GetInventorySlots() => items;
    }
}