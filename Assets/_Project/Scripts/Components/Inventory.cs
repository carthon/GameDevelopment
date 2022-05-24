using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Components.Items;
using _Project.Scripts.UI;
using Google.Protobuf.WellKnownTypes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class Inventory {
        private List<ItemStack> items;

        public event Action<int, Inventory> OnItemAddedToSlot;
        public event Action<int, Inventory> OnItemTakenFromSlot;
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
            for (int i = 0; i < Size; i++) {
                items.Add(new ItemStack(this, i));
            }
        }

        public ItemStack GetItem(int slot) {
            ItemStack item = ItemStack.EMPTY;
            if (slot >= 0 && slot < Size && !items[slot].IsEmpty())
                item = items[slot];
            return item;
        }

        public int AddItemToSlot(ItemStack itemStack, int slot) {
            ItemStack itemInInventory = items.Find(item => itemStack.Item == item.Item);
            if (itemInInventory!= null) slot = itemInInventory.GetSlotID();
            int difference = 0;
            if (itemInInventory != null) {
                difference = itemInInventory.AddToStack(itemStack.GetCount());
                return difference;
            }
            else if (slot >= 0 && slot < Size){
                slot = items.FindIndex(item => item.IsEmpty());
                items[slot].SetStack(itemStack);
            }
            OnItemAddedToSlot?.Invoke(slot, this);
            return difference;
        }
        public int AddItemToSlot(Item item, int slot) {
            ItemStack itemStack = new ItemStack(item, slot, this);
            itemStack.AddToStack(1);
            return AddItemToSlot(itemStack, slot);
        }

        public int AddItem(Item item) {
            ItemStack itemStack = new ItemStack(item, 0, this);
            itemStack.AddToStack(1);
            return AddItemToSlot(item, 0);
        }
        public int AddItem(ItemStack item) {
            return AddItemToSlot(item, 0);
        }

        public int TakeAmount(ItemStack itemStack, int amount)
        {
            int itemSlot = GetItemStackSlot(itemStack);
            OnItemTakenFromSlot?.Invoke(itemSlot, this);
            return items[itemSlot].TakeFromStack(amount);
        }

        public ItemStack TakeStack(ItemStack itemStack) {
            int indexOf = GetItemStackSlot(itemStack);
            return TakeStackFromSlot(indexOf);
        }
        
        public ItemStack TakeStackFromSlot(int slot) {
            ItemStack itemStack = null;
            if (slot >= 0 && slot < Size) {
                itemStack = items[slot].TakeStack();
            }
            OnItemTakenFromSlot?.Invoke(slot, this);
            return itemStack;
        }
        public int GetItemStackSlot(ItemStack itemStack) => items.IndexOf(itemStack);
        public List<ItemStack> GetInventorySlots() => items;

        public int GetItemsCount()
        {
            int count = 0;
            foreach (ItemStack itemStack in items) {
                if (!itemStack.IsEmpty ())
                    count++;
            }
            return count;
        }
    }
}