using System;
using UnityEngine;

namespace _Project.Scripts.Components.Items {
    public class ItemStack{
        public Item Item { get; set; }
        public static ItemStack EMPTY = new ItemStack ();

        private int Count { get; set; }

        private Inventory parent;
        private int slotID;
        public ItemStack(ItemStack itemStack)
        {
            this.Item = itemStack.Item;
            this.Count = itemStack.GetCount ();
        }

        public ItemStack(Inventory parent)
        {
            Item = null;
            slotID = -1;
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
        public ItemStack(Item item, int slotID, int count) {
            Item = item;
            this.slotID = slotID;
            Count = count;
        }
        public int GetCount() => Count;
        public void SetCount(int count) => this.Count = count;
        public int AddToStack(int amount)
        {
            int difference = 0;
            if (Item.isStackable) {
                difference = (Count + amount) - Item.GetMaxStackSize();
                if (Item.GetMaxStackSize() <= (amount + Count))
                    this.Count += amount;
                if (difference < 0) difference = 0;
            }
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
        public Inventory GetInventory() => parent;

        public static void SwapItemsStack(ItemStack itemStack1, ItemStack itemStack2)
        {
            ItemStack tempItemStack = itemStack1.TakeStack ();
            itemStack1.SetStack(itemStack2.TakeStack());
            itemStack2.SetStack(tempItemStack);
        }
    }
}