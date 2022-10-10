using System;
using JetBrains.Annotations;

[Serializable]
public class ItemStack {
    public static readonly ItemStack EMPTY = new ItemStack();

    private int count;

    private Inventory parent;
    private int slotID;
    public ItemStack(ItemStack itemStack) {
        Item = itemStack.Item;
        count = itemStack.GetCount();
        parent = itemStack.parent;
        slotID = itemStack.slotID;
    }

    public ItemStack(Inventory parent, int slotID) {
        Item = null;
        this.slotID = slotID;
        count = 0;
        this.parent = parent;
    }

    public ItemStack() {
        Item = null;
        slotID = -1;
        count = 0;
        parent = null;
    }
    public ItemStack(Item item, int count) {
        Item = item;
        this.count = count;
        slotID = -1;
        parent = null;
    }
    public ItemStack(Item item, int slotID, Inventory parent) {
        Item = item;
        this.slotID = slotID;
        this.parent = parent;
        count = 0;
    }
    public Item Item { get; set; }

    public int GetCount() {
        return count;
    }
    public void SetCount(int newCount) {
        count = newCount;
    }
    public void SetStack(ItemStack itemStack) {
        Item = itemStack.Item;
        count = itemStack.GetCount();
    }
    public bool IsEmpty() {
        return count <= 0;
    }
    public bool IsFull() {
        return count >= Item.GetMaxStackSize();
    }
    public int GetSlotID() {
        return slotID;
    }
    public void SetSlot(int newValue) {
        slotID = newValue;
    }
    public Inventory GetInventory() {
        return parent;
    }
    public override int GetHashCode() {
        return (Item, slotID, Count: count).GetHashCode();
    }
    public override bool Equals([CanBeNull] object obj) {
        return Equals(obj as ItemStack);
    }
    public bool Equals(ItemStack other) {
        return other != null &&
            Item == other.Item;
    }
    public ItemStack GetCopy() {
        return new ItemStack(this);
    }
    public void SetInventory(Inventory inventory) {
        parent = inventory;
    }
}