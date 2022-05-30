using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Components.Items;
using _Project.Scripts.UI;
using Google.Protobuf.WellKnownTypes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace _Project.Scripts.Components {
    [Serializable]
    public class Inventory {
        private List<ItemStack> items;

        public event Action<int> OnItemAddedToSlot;
        public event Action<int> OnItemTakenFromSlot;
        public event Action<Inventory, int> OnInventoryChange;
        public event Action<ItemStack, ItemStack> OnInventorySwap;
        public string Name { get; set; }
        public int Size { get; private set; }

        private int freeSpace;

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
            freeSpace = Size;
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
            int possibleSlot = FindItemStackSlot(itemStack);
            slot = possibleSlot >= 0 ? possibleSlot : slot;
            int difference = 0;
            if (freeSpace >= 1) {
                if (possibleSlot != -1) {
                    difference = AddToStack(slot, itemStack.GetCount());
                }
                else if (slot >= 0 && slot < Size) {
                    slot = items.FindIndex(item => item.IsEmpty());
                    items[slot].SetStack(itemStack);
                    items[slot].SetSlot(slot);
                    freeSpace -= 1;
                }
                OnItemAddedToSlot?.Invoke(slot);
                OnInventoryChange?.Invoke(this, slot);
            }
            else {
                difference = -1;
                Debug.Log("NO FREE SPACE!");
            }
            return difference;
        }
        public int AddItemToSlot(Item item, int slot) {
            ItemStack itemStack = new ItemStack(item, slot, this);
            itemStack.SetCount(1);
            return AddItemToSlot(itemStack, slot);
        }

        public int AddItem(Item item) {
            ItemStack itemStack = new ItemStack(item, 0, this);
            itemStack.SetCount(1);
            return AddItemToSlot(itemStack, 0);
        }
        public int AddItem(ItemStack item) {
            if (item.IsEmpty())
                return -1;
            return AddItemToSlot(item, 0);
        }
        
        private int AddToStack(int slot, int amount)
        {
            int difference = 0;
            int itemCount = items[slot].GetCount();
            int maxStackSize = items[slot].Item.GetMaxStackSize();
            difference = (itemCount + amount) -  maxStackSize;
            if ((amount + itemCount) <= maxStackSize)
                items[slot].SetCount(itemCount + amount);
            if (difference < 0) difference = 0;
            return difference;
        }
        
        public ItemStack TryAddItem(ItemStack itemStack) {
            ItemStack itemStackCopy = new ItemStack(itemStack);
            int difference = AddItem(itemStack);
            if (difference != -1) {
                itemStackCopy = ItemStack.EMPTY;
            }
            return itemStackCopy;
        }

        public ItemStack TakeStack(ItemStack itemStack) {
            int indexOf = FindItemStackSlot(itemStack);
            return TakeStackFromSlot(indexOf);
        }
        
        public ItemStack TakeStackFromSlot(int slot) {
            ItemStack itemStackCopy = new ItemStack(items[slot]);
            if (slot >= 0 && slot < Size && !items[slot].IsEmpty()) {
                int count = items[slot].GetCount();
                int difference = count - items[slot].GetCount();
                int finalCount = difference > 0 ? difference : 0;
                items[slot].SetCount(finalCount);
                itemStackCopy.SetCount(itemStackCopy.GetCount() - difference);
                if (finalCount <= 0)
                    freeSpace += 1;
                OnItemTakenFromSlot?.Invoke(slot);
                OnInventoryChange?.Invoke(this, slot);
            }
            return itemStackCopy;
        }
        public int FindItemStackSlot(ItemStack itemStack) => items.FindIndex(item => item.Equals(itemStack));
        public List<ItemStack> GetInventorySlots() => items;

        public int GetFreeSpace() => freeSpace;
        public void SetStack(int slot, ItemStack itemStack) {
            if (itemStack.IsEmpty())
                freeSpace += 1;
            items[slot].SetStack(itemStack);
            OnInventoryChange?.Invoke(this, slot);
        }
        public bool SwapItemsInInventory(Inventory otherInventory, ItemStack itemStack, ItemStack otherItemStack) {
            int itemStackSlot = FindItemStackSlot(itemStack);
            int otherItemStackSlot = otherInventory.FindItemStackSlot(otherItemStack);
            if (itemStackSlot == -1 && otherItemStackSlot == -1)
                return false;
            ItemStack tempItemStack = itemStack.GetCopy();
            if (itemStackSlot != -1) this.SetStack(itemStackSlot, otherItemStack);
            otherInventory.SetStack(otherItemStackSlot, tempItemStack);
            //OnInventorySwap?.Invoke(itemStack, otherItemStack);
            return true;
        }
    }
}