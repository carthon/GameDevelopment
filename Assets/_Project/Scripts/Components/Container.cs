using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.Components {
    public class Container : MonoBehaviour {
        private Inventory _inventory;
        private ItemSize _itemSize;
        public int MaxContainerSlots() => _inventory.Size;
        public bool IsStatic { get; private set; }
        public Inventory Inventory { get =>_inventory; }

        public void InitializeContainer(bool isStatic, Inventory inventory, ItemSize itemSize) {
            _inventory = inventory;
            _itemSize = itemSize;
            IsStatic = isStatic;
        }
        public ItemStack AddItemStack(ItemStack itemStack) {
            ItemStack leftOver = itemStack;
            if(itemStack.Item.Size == _itemSize)
                leftOver = _inventory.AddItemStack(itemStack);
            Logger.Singleton.Log(_inventory.ToString(), Logger.Type.DEBUG);

            return leftOver;
        }
        public ItemStack TakeItemFromSlot(int slot) {
            return _inventory.TakeStackFromSlot(slot);
        }
        public ItemStack TakeItemFromSlot(int slot, int amount) {
            return _inventory.TakeItemsFromSlot(slot, amount);
        }
        public void SwapItems(int slot, int slot2) {
            _inventory.SwapItemsInInventory(_inventory, slot, slot2);
        }
    }
}