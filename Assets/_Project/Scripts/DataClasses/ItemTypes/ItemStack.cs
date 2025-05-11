using System;
using _Project.Scripts.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace _Project.Scripts.DataClasses.ItemTypes {
    [Serializable]
    public class ItemStack {
        public static readonly ItemStack EMPTY = new ItemStack();

        private int count;

        private Inventory parent;
        public Item Item { get; set; }
        public Vector2Int OriginalSlot;
        public ItemStack(ItemStack itemStack) {
            Item = itemStack.Item;
            count = itemStack.GetCount();
            parent = itemStack.parent;
            OriginalSlot = itemStack.OriginalSlot;
        }
        public ItemStack(Inventory parent, Vector2Int originalSlot) {
            Item = null;
            OriginalSlot = originalSlot;
            count = 0;
            this.parent = parent;
        }
        public ItemStack(Inventory parent) {
            Item = null;
            OriginalSlot = Vector2Int.zero;
            count = 0;
            this.parent = parent;
        }
        public ItemStack() {
            Item = null;
            OriginalSlot = Vector2Int.zero;
            count = 0;
            parent = null;
        }
        public ItemStack(Item item, int count) {
            Item = item;
            this.count = count;
            OriginalSlot = Vector2Int.zero;
            parent = null;
        }
        public ItemStack(Item item, int count, Vector2Int originalSlot) {
            Item = item;
            this.count = count;
            OriginalSlot = originalSlot;
            parent = null;
        }
        public ItemStack(Item item, Vector2Int originalSlot, Inventory parent) {
            Item = item;
            this.OriginalSlot = originalSlot;
            this.parent = parent;
            count = 0;
        }

        public int GetCount() {
            return count;
        }
        public void SetCount(int newCount) {
            count = newCount;
        }
        public void SetStack(ItemStack itemStack) {
            Item = itemStack.Item;
            count = itemStack.GetCount();
            OriginalSlot = itemStack.OriginalSlot;
            parent = itemStack.parent;
        }
        public bool IsEmpty() {
            return count <= 0;
        }
        public bool IsFull() {
            return count >= Item.GetMaxStackSize();
        }
        public Inventory GetInventory() {
            return parent;
        }
        public override int GetHashCode() {
            return (Item, slotID: OriginalSlot, Count: count).GetHashCode();
        }
        public override bool Equals([CanBeNull] object obj) {
            return Equals(obj as ItemStack);
        }
        public bool Equals(ItemStack other) {
            return other != null &&
                Item == other.Item && OriginalSlot == other.OriginalSlot && 
                parent == other.parent;
        }
        public ItemStack GetCopy() {
            return new ItemStack(this);
        }
        public void SetInventory(Inventory inventory) {
            parent = inventory;
        }
        public override string ToString() {
            return $"Item:{Item.ToString()} Count:{count} OriginalSlot:{OriginalSlot}";
        }
    }
}