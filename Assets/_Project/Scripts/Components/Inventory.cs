using System;
using System.Collections.Generic;
using _Project.Scripts.Components;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class Inventory {

    private int _freeSpace;
    private List<ItemStack> _items;
    public int Id { get; set; }
    public event Action<int, ItemStack> OnSlotChange;
    public event Action<int, int, int> OnSlotSwap;
    
    public Inventory(string name, int size) {
        Name = name;
        Size = size;
        Init();
    }
    public Inventory(int size) {
        Name = "No name";
        Size = size;
        Init();
    }
    public string Name { get; set; }
    public int Size { get; }


    private void Init() {
        _freeSpace = Size;
        _items = new List<ItemStack>();
        for (var i = 0; i < Size; i++) {
            var itemStack = new ItemStack(this, i);
            _items.Add(itemStack);
        }
    }

    public ItemStack GetItemStack(int slot) {
        var item = new ItemStack(this, slot);
        if (IsValidSlot(slot))
            item = _items[slot];
        else
            item.SetSlot(-1);
        return item;
    }

    public ItemStack AddItemStackToSlot(ItemStack itemStack, int slot) {
        var itemLeftOver = new ItemStack(itemStack);
        itemLeftOver.SetCount(0);
        if (_freeSpace >= 1) {
            if (IsValidSlot(slot)) {
                if (!itemStack.Item.Equals(null)) {
                    if (itemStack.Item.Equals(_items[slot].Item) &&
                        itemStack.GetCount() <= _items[slot].Item.GetMaxStackSize()) {
                        itemLeftOver.SetCount(AddToStack(slot, itemStack.GetCount()));
                    }
                    else {
                        _freeSpace -= 1;
                        _items[slot].SetStack(itemStack);
                    }
                }
                if (itemLeftOver.GetCount() > 0) itemLeftOver = AddItemStack(itemLeftOver);
                OnSlotChange?.Invoke(slot, _items[slot]);
            }
        }
        else {
            Debug.Log("NO FREE SPACE!");
        }
        return itemLeftOver;
    }

    public ItemStack AddItemStack(ItemStack itemStack) {
        var slot = FindItemStackSlot(itemStack);
        var difference = new ItemStack(itemStack);
        difference.SetCount(0);
        if (_freeSpace >= 1) {
            if (slot == -1 || _items[slot].IsFull())
                slot = _items.FindIndex(item => item.IsEmpty());
            difference = AddItemStackToSlot(itemStack, slot);
        }
        else {
            Debug.Log("No free space!");
        }
        return difference;
    }

    /// <summary>
    ///     Adds an amount of items on a slot
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="amount"></param>
    /// <returns>The amount of items that couldn't be added to that slot due overflow</returns>
    private int AddToStack(int slot, int amount) {
        var itemCount = _items[slot].GetCount();
        var maxStackSize = _items[slot].Item.GetMaxStackSize();
        var difference = itemCount + amount - maxStackSize;
        //Si la diferencia es negativa, significa que no se ha llenado el stack
        if (difference < 0) {
            _items[slot].SetCount(itemCount + amount);
            difference = 0;
        }
        else {
            _items[slot].SetCount(maxStackSize);
        }
        return difference;
    }

    public ItemStack TakeItemsFromSlot(int slot, int count) {
        var itemStackCopy = new ItemStack(this, slot);
        if (IsValidSlot(slot) && !_items[slot].IsEmpty()) {
            itemStackCopy = new ItemStack(_items[slot]);
            var newCount = _items[slot].GetCount() - count;
            if (newCount <= 0) {
                _freeSpace += 1;
                newCount = 0;
            }
            _items[slot].SetCount(newCount);
            OnSlotChange?.Invoke(slot, itemStackCopy);
        }
        return itemStackCopy;
    }

    public ItemStack TakeStack(ItemStack itemStack) {
        var indexOf = FindItemStackSlot(itemStack);
        return TakeStackFromSlot(indexOf);
    }

    public ItemStack TakeStackFromSlot(int slot) {
        var itemStackCopy = new ItemStack(this, slot);
        if (slot >= 0 && slot < Size && !_items[slot].IsEmpty()) itemStackCopy = TakeItemsFromSlot(slot, _items[slot].Item.GetMaxStackSize());
        return itemStackCopy;
    }
    public void DropItemInSlot(int slot, Vector3 worldPos) {
        var lootTable = new LootTable();
        var itemStack = TakeStackFromSlot(slot);
        lootTable.AddToLootTable(itemStack);
        //GetParent().SetData(itemStack.GetInventory().GetItem(itemStack.GetSlotID()));
        var worldItem = Object.Instantiate(itemStack.Item.modelPrefab, worldPos, Quaternion.identity);
        var pickableItem = worldItem.AddComponent<Grabbable>();
        pickableItem.SetLootTable(lootTable);
        worldItem.AddComponent<Rigidbody>();
    }
    public int FindItemStackSlot(ItemStack itemStack) {
        return _items.FindIndex(item => item.Item != null && item.Item.Equals(itemStack.Item) && !item.IsFull());
    }

    public List<ItemStack> GetInventorySlots() {
        return _items;
    }

    public int GetFreeSpace() {
        return _freeSpace;
    }
    public bool SwapItemsInInventory(Inventory otherInventory, int itemStackSlot, int otherItemStackSlot) {
        if (itemStackSlot == -1 && otherItemStackSlot == -1)
            return false;

        var thisItemStack = TakeStackFromSlot(itemStackSlot);
        var otherItemStack = otherInventory.TakeStackFromSlot(itemStackSlot);
        if (itemStackSlot != -1) AddItemStackToSlot(otherItemStack, otherItemStackSlot);
        otherInventory.AddItemStackToSlot(thisItemStack, itemStackSlot);
        if(otherInventory == this)
            OnSlotSwap?.Invoke(Id, itemStackSlot, otherItemStackSlot);
        return true;
    }
    public List<ItemStack> GetItemStacksByType(Item item) {
        return _items.FindAll(itemStack => itemStack.Item != null && itemStack.Item.Equals(item));
    }
    public bool IsValidSlot(int slot) {
        return slot >= 0 && slot < Size;
    }
    public bool IsEmpty() {
        return _freeSpace == Size;
    }
}