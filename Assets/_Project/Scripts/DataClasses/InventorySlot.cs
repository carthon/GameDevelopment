using _Project.Scripts.DataClasses.ItemTypes;
using UnityEngine;

namespace _Project.Scripts.DataClasses {
    public class InventorySlot {
        public ItemStack ItemStack = null;
        public GameObject Model = null;
        public bool IsOrigin = false;
        public bool IsFlipped = false;
        public InventorySlot(ItemStack itemStack, bool isFlipped, bool isOrigin) {
            ItemStack = itemStack;
            Model = null;
            IsOrigin = isOrigin;
            IsFlipped = isFlipped;
        }
        public InventorySlot() {
            ItemStack = null;
            Model = null;
            IsOrigin = false;
            IsFlipped = false;
        }
    }
}