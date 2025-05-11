using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Handlers;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class Inventory {
        public const int MaxStackSize = 64;

        private int _freeSpace;
        private InventorySlot[,] _itemsGrid;
        private Dictionary<Item, List<ItemStack>> _itemsDict;
        public IEntity Owner { get; set; }
        public int Id { get; set; }
        public event Action<InventorySlot> OnSlotChange;
        public string Name { get; set; }
        public int Height { get; }
        public int Width { get; }

        public Inventory(string name, IEntity owner, int width, int height) {
            Name = name;
            Width = width;
            Height = height;
            Owner = owner;
            Init();
        }
        private void Init() {
            _freeSpace = Width * Height;
            _itemsGrid = new InventorySlot[Width, Height];
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++) {
                _itemsGrid[x, y] = new InventorySlot();
            }
            _itemsDict = new Dictionary<Item, List<ItemStack>>();
        }
        public ItemStack TakeItemStack(Vector2Int slotIndex) {
            var itemStack = new ItemStack(this);
            int slotX = slotIndex.x, slotY = slotIndex.y;
            if (IsValidSlot(slotIndex)) {
                itemStack = _itemsGrid[slotX, slotY].ItemStack.GetCopy();
                InventorySlot itemSlot = _itemsGrid[slotX, slotY];
                if (!itemStack.IsEmpty()) {
                    Item item = itemSlot.ItemStack.Item;
                    _itemsGrid[slotX, slotY].ItemStack.SetCount(0);
                    _itemsGrid[slotX, slotY].IsOrigin = false;
                    _itemsGrid[slotX, slotY].IsFlipped = false;
                    _itemsDict[item].Remove(_itemsGrid[slotX, slotY].ItemStack);
                    _freeSpace += item.Width + item.Height;
                    OnSlotChange?.Invoke(itemSlot);
                }
            }
            return itemStack;
        }
        public ItemStack AddItemStack(ItemStack itemStack) {
            ItemStack leftOver = itemStack.GetCopy();
            Item item = itemStack.Item;
            InventorySlot itemSlot = new InventorySlot();
            bool foundPos = false;
            if (_itemsDict.TryGetValue(itemStack.Item, out List<ItemStack> itemStackList)) {
                int findIndex;
                do {
                    findIndex = itemStackList.FindIndex(stack => stack.GetCount() < item.GetMaxStackSize());
                    if (findIndex <= -1)
                        continue;
                    ItemStack itemStackFree = itemStackList[findIndex];
                    int finalCount = itemStackFree.GetCount() + itemStack.GetCount();
                    int itemsTaken = finalCount - item.GetMaxStackSize();
                    finalCount = Math.Min(finalCount, item.GetMaxStackSize());
                    itemStackFree.SetCount(finalCount);
                    leftOver.SetCount(Math.Max(itemsTaken, 0));
                    itemSlot = _itemsGrid[itemStackFree.OriginalSlot.x, itemStackFree.OriginalSlot.y];
                } while (findIndex > -1 && leftOver.GetCount() > 0);
            }
            if (leftOver.GetCount() > 0) {
                for (int y = 0; y < Height && !foundPos; y++)
                for (int x = 0; x < Width && !foundPos; x++) {
                    bool isFlipped = false;
                    for (int flip = 0; flip <= 1; flip++) {
                        Vector2Int slot = new Vector2Int(x, y);
                        if (CanPlaceItemStackAt(item, slot, isFlipped)) {
                            PlaceItemStack(leftOver, slot, isFlipped);
                            itemSlot = _itemsGrid[x, y];
                            leftOver.SetCount(0);
                            foundPos = true;
                            break;
                        }
                        isFlipped = !isFlipped;
                    }
                }
            }
            if (itemSlot.ItemStack is not null) {
                if (leftOver.GetCount() == 0) {
                    _freeSpace -= item.Height * item.Width;
                    itemSlot.ItemStack.SetInventory(this);
                }
                OnSlotChange?.Invoke(itemSlot);
            }
            return leftOver;
        }
        public void SetInventorySlot(InventorySlot inventorySlot) {
            PlaceItemStack(inventorySlot.ItemStack, inventorySlot.ItemStack.OriginalSlot, inventorySlot.IsFlipped, false);
        }
        private void PlaceItemStack(ItemStack itemStack, Vector2Int pos, bool isFlipped, bool addToDict = true) {
            int width = itemStack.Item.Width, height = itemStack.Item.Height;
            ItemStack itemStackCopy = itemStack.GetCopy();
            itemStackCopy.OriginalSlot = pos;
            bool markedAsOrigin = false;
            if (isFlipped) {
                width = itemStack.Item.Height;
                height = itemStack.Item.Width;
            }
            for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++) {
                int x = pos.x + i, y = pos.y + j;
                _itemsGrid[x, y].ItemStack = itemStackCopy;
                _itemsGrid[x, y].IsFlipped = isFlipped;
                if (markedAsOrigin)
                    continue;
                _itemsGrid[x, y].IsOrigin = true;
                markedAsOrigin = true;
            }
            if (!addToDict)
                return;
            if (_itemsDict.TryGetValue(itemStack.Item, out List<ItemStack> itemStacks)) {
                itemStacks.Add(itemStackCopy);
            }
            else
                _itemsDict.Add(itemStack.Item, new List<ItemStack> { itemStackCopy });
        }
        public ItemStack TakeItemsFromSlot(Vector2Int slot, int count) {
            var itemsTaken = new ItemStack(this);
            int slotX = slot.x, slotY = slot.y;
            if (IsValidSlot(slot) && _itemsGrid[slotX, slotY].ItemStack is not null) {
                ItemStack itemStackReference = _itemsGrid[slotX, slotY].ItemStack;
                itemsTaken = new ItemStack(itemStackReference);
                int resultCount = itemStackReference.GetCount() - count;
                if (resultCount <= 0) {
                    resultCount = 0;
                    _freeSpace += itemStackReference.Item.Height * itemStackReference.Item.Width;
                }
                itemStackReference.SetCount(resultCount);
                OnSlotChange?.Invoke(_itemsGrid[slotX, slotY]);
            }
            return itemsTaken;
        }
        public void DropItemInSlot(Vector2Int slot, Vector3 worldPos, Quaternion rotation) {
            DropItemInSlot(slot, GetInventorySlot(slot).ItemStack.GetCount(), worldPos, rotation);
        }
        public void DropItemInSlot(Vector2Int slot, int count, Vector3 worldPos, Quaternion rotation) {
            var itemStack = TakeItemsFromSlot(slot, count);
            if (!itemStack.IsEmpty())
                GameManager.SpawnItem(itemStack, worldPos, rotation, Owner);
        }
        public bool TryGetItemStack(Item item, out ItemStack itemStack) {
            List<ItemStack> itemStacks;
            itemStack = ItemStack.EMPTY;
            if (_itemsDict.TryGetValue(item, out itemStacks)) {
                itemStack = itemStacks.First();
                return true;
            }
            return false;
        }
        public InventorySlot GetInventorySlot(Vector2Int slot) {
            if (IsValidSlot(slot))
                return _itemsGrid[slot.x, slot.y];
            return new InventorySlot();
        }
        public ItemStack GetItemStackFromSlot(Vector2Int slot) {
            if (IsValidSlot(slot))
                return _itemsGrid[slot.x, slot.y].ItemStack;
            return ItemStack.EMPTY;
        }
        public int GetFreeSpace() {
            return _freeSpace;
        }
        public bool ValidateSwap(Inventory otherInventory, Vector2Int itemStackSlot, Vector2Int otherItemStackSlot, bool wasFlipped, out List<InventorySlot> collidingSlots) {
            collidingSlots = new List<InventorySlot>();
            if (!IsValidSlot(itemStackSlot) || !otherInventory.IsValidSlot(otherItemStackSlot))
                return false;
            InventorySlot inventorySlot = GetInventorySlot(itemStackSlot);
            //InventorySlot otherInventoryslot = otherInventory.GetInventorySlot(otherItemStackSlot);
            if (otherInventory.CanPlaceItemStackAt(inventorySlot.ItemStack, otherItemStackSlot, wasFlipped, collidingSlots)) {
                Item item = inventorySlot.ItemStack.Item;
                int maxSizeAllowed = item.Width * item.Height;
                int collidingSize = 0;
                foreach (InventorySlot collidingSlot in collidingSlots) {
                    if (collidingSlot.ItemStack.Equals(inventorySlot.ItemStack))
                        continue;
                    Item collidingItem = collidingSlot.ItemStack.Item;
                    collidingSize += collidingItem.Width * collidingItem.Height;
                    if (collidingSize > maxSizeAllowed)
                        return false;
                }
            }
            else 
                return false;
            return true;
        }
        public bool SwapItemsInInventory(Inventory otherInventory, Vector2Int itemStackSlot, Vector2Int otherItemStackSlot, bool wasFlipped) {
            List<InventorySlot> collidingSlots;
            //Verificamos que se están intercambiando objetos con tamaños similares
            if (ValidateSwap(otherInventory, itemStackSlot, otherItemStackSlot, wasFlipped, out collidingSlots)) {
                InventorySlot stackSlot = GetInventorySlot(itemStackSlot);
                bool isFlipped = stackSlot.IsFlipped;
                List<Vector2Int> slotPositions = new List<Vector2Int>();
                List<Vector2Int> otherSlotPositions = new List<Vector2Int>();
                int width = stackSlot.ItemStack.Item.Width;
                int height = stackSlot.ItemStack.Item.Height;
                for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++) {
                    slotPositions.Add(isFlipped ? new Vector2Int(j, i) : new Vector2Int(i, j));
                    otherSlotPositions.Add(wasFlipped ? new Vector2Int(j, i) : new Vector2Int(i, j));

                }
                int collidingSlotsIndex = 0;
                ItemStack itemStack = TakeItemStack(itemStackSlot);
                foreach (InventorySlot collidingSlot in collidingSlots) {
                    Vector2Int otherOriginalSlot = collidingSlot.ItemStack.OriginalSlot;
                    InventorySlot otherInventorySlot = otherInventory.GetInventorySlot(otherOriginalSlot);
                    bool otherInventorySlotIsFlipped = otherInventorySlot.IsFlipped;
                    ItemStack otherItemStack = otherInventory.TakeItemStack(otherOriginalSlot);
                    PlaceItemStack(otherItemStack, itemStackSlot + slotPositions[collidingSlotsIndex], otherInventorySlotIsFlipped);
                    otherItemStack.SetInventory(this);
                    InventorySlot newInventorySlot = GetInventorySlot(itemStackSlot + slotPositions[collidingSlotsIndex]);
                    OnSlotChange?.Invoke(newInventorySlot);
                    collidingSlotsIndex++;
                }
                otherInventory.PlaceItemStack(itemStack, otherItemStackSlot, wasFlipped);
                itemStack.SetInventory(otherInventory);
                foreach (Vector2Int slotPosition in otherSlotPositions) {
                    stackSlot = otherInventory.GetInventorySlot(otherItemStackSlot + slotPosition);
                    otherInventory.OnSlotChange?.Invoke(stackSlot);
                }
                return true;
            }
            return false;
        }
        public bool CanPlaceItemStackAt(ItemStack itemStack, Vector2Int pos, bool isFlipped, List<InventorySlot> colliders) {
            int width = itemStack.Item.Width, height = itemStack.Item.Height;
            if (colliders is null) {
                colliders = new List<InventorySlot>();
            }
            if (isFlipped) {
                width = itemStack.Item.Height;
                height = itemStack.Item.Width;
            }
            for (int i = 0; i < height; i++) 
            for (int j = 0; j < width; j++) {
                int row = pos.y + i;
                int col = pos.x + j;
                if (row >= Width || col >= Height)
                    return false;
                if (_itemsGrid[col, row].ItemStack is not null && !_itemsGrid[col, row].ItemStack.IsEmpty()) {
                    if (_itemsGrid[col, row].IsOrigin)
                        colliders.Add(_itemsGrid[col, row]);
                    else if (!_itemsGrid[col, row].ItemStack.Equals(itemStack) && 
                             !colliders.Exists(slot => slot.ItemStack.Equals(_itemsGrid[col, row].ItemStack)))
                        return false;
                }
            }
            return true;
        }
        public bool CanPlaceItemStackAt(Item item, Vector2Int pos, bool isFlipped) {
            int width = item.Width, height = item.Height;
            if (isFlipped) {
                width = item.Height;
                height = item.Width;
            }
            for (int i = 0; i < height; i++) 
            for (int j = 0; j < width; j++) {
                int row = pos.y + i;
                int col = pos.x + j;
                if (row >= Width || col >= Height)
                    return false;
                if (_itemsGrid[col, row].ItemStack is not null && !_itemsGrid[col,row].ItemStack.IsEmpty())
                    return false;
            }
            return true;
        }
        public bool TryGetItemStacksByType(Item item, out List<ItemStack> itemStacks) {
            itemStacks = new List<ItemStack>();
            return _itemsDict.TryGetValue(item, out itemStacks);
        }
        public bool IsValidSlot(Vector2Int slot) {
            return slot.x >= 0 && slot.x < Width && slot.y >= 0 && slot.y < Height;
        }
        public bool IsEmpty() {
            return _freeSpace == Width * Height;
        }
        public override string ToString() {
            StringBuilder str = new StringBuilder();
            str.Append($"Name:{Name} FreeSpace:{_freeSpace}");
            foreach (InventorySlot inventorySlot in _itemsGrid) {
                str.Append(inventorySlot.ItemStack.ToString() + "\n");
            }
            return str.ToString();
        }
    }
}