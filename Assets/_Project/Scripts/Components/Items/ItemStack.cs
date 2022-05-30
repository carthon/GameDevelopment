using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using Object = System.Object;

namespace _Project.Scripts.Components.Items {
    public class ItemStack {
        public Item Item { get; set; }
        public static readonly ItemStack EMPTY = new ItemStack ();

        private int count;

        private Inventory parent;
        private int slotID;
        public ItemStack(ItemStack itemStack)
        {
            Item = itemStack.Item;
            count = itemStack.GetCount();
            parent = itemStack.parent;
        }

        public ItemStack(Inventory parent, int slotID)
        {
            Item = null;
            this.slotID = slotID;
            count = 0;
            this.parent = parent;
        }

        public ItemStack()
        {
            Item = null;
            slotID = -1;
            count = 0;
            parent = null;
        }
        public ItemStack(Item item, int slotID) {
            Item = item;
            this.slotID = slotID;
            count = 0;
            parent = null;
        }
        public ItemStack(Item item, int slotID, Inventory parent) {
            Item = item;
            this.slotID = slotID;
            this.parent = parent;
            count = 0;
        }
        
        public int GetCount() => count;
        public void SetCount(int newCount) => this.count = newCount;
        public void SetStack(ItemStack itemStack)
        {
            this.Item = itemStack.Item;
            this.count = itemStack.GetCount ();
            this.parent = itemStack.GetInventory ();
            this.slotID = itemStack.slotID;
        }
        public bool IsEmpty() => count <= 0;
        public int GetSlotID() => slotID;
        public void SetSlot(int newValue) => this.slotID = newValue;
        public Inventory GetInventory() => parent;
        public override int GetHashCode() => (Item, slotID, Count: count).GetHashCode();
        public override bool Equals([CanBeNull] object obj) => this.Equals(obj as ItemStack);
        public bool Equals(ItemStack other) => other != null &&
            Item == other.Item;
        public ItemStack GetCopy() {
            return new ItemStack(this);
        }
        public void SetInventory(Inventory inventory) => parent = inventory;
    }
}