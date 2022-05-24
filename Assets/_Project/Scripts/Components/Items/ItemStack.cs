using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using Object = System.Object;

namespace _Project.Scripts.Components.Items {
    public class ItemStack {
        public Item Item { get; set; }
        public static readonly ItemStack EMPTY = new ItemStack ();

        private int Count { get; set; }

        private Inventory parent;
        private int slotID;
        public ItemStack(ItemStack itemStack)
        {
            this.Item = itemStack.Item;
            this.Count = itemStack.GetCount ();
            this.parent = itemStack.parent;
        }

        public ItemStack(Inventory parent, int slotID)
        {
            Item = null;
            this.slotID = slotID;
            Count = 0;
            this.parent = parent;
        }

        public ItemStack()
        {
            Item = null;
            slotID = -1;
            Count = 0;
            parent = null;
        }
        public ItemStack(Item item, int slotID) {
            Item = item;
            this.slotID = slotID;
            Count = 0;
            parent = null;
        }
        public ItemStack(Item item, int slotID, Inventory parent) {
            Item = item;
            this.slotID = slotID;
            this.parent = parent;
            Count = 0;
        }
        
        public int GetCount() => Count;
        public void SetCount(int count) => this.Count = count;
        public int AddToStack(int amount)
        {
            int difference = 0;
            difference = (Count + amount) - Item.GetMaxStackSize();
            if (Item.GetMaxStackSize() <= (amount + Count))
                this.Count += amount;
            if (difference < 0) difference = 0;
            return difference;
        }
        public int TakeFromStack(int amount)
        {
            int difference = Mathf.Min(amount, Count);
            Count -= difference;
            return difference;
        }

        public ItemStack TakeStack()
        {
            ItemStack item = new ItemStack(this);
            Count = 0;
            return item;
        }
        public void SetStack(ItemStack itemStack)
        {
            this.Item = itemStack.Item;
            this.Count = itemStack.GetCount ();
            this.parent = itemStack.GetInventory ();
            this.slotID = itemStack.slotID;
        }
        public bool IsEmpty() => Count <= 0;
        public int GetSlotID() => slotID;
        public void SetSlot(int newValue) => this.slotID = newValue;
        public Inventory GetInventory() => parent;

        public static void SwapItemsStack(ItemStack itemStack1, ItemStack itemStack2) {
            ItemStack tempItemStack = itemStack1.parent.TakeStack(itemStack1);
            itemStack1.SetStack(itemStack2.parent.TakeStack(itemStack2));
            itemStack2.SetStack(tempItemStack);
        }
        public override int GetHashCode() => (Item, slotID, Count).GetHashCode();
        public override bool Equals([CanBeNull] object obj) => this.Equals(obj as ItemStack);
        public bool Equals(ItemStack other) => other != null &&
            Item == other.Item;
        public ItemStack GetCopy() {
            return new ItemStack(this);
        }
        public void SetInventory(Inventory inventory) => parent = inventory;
    }
}