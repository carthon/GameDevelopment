using _Project.Scripts.DataClasses.ItemTypes;
using UnityEngine;

namespace _Project.Scripts.DataClasses {
    public class InventorySlot {
        public ItemStack ItemStack = null;
        public GameObject Model = null;
        public bool IsOrigin = false;
        public bool IsFlipped = false;
        public Vector2Int CellSlot;
        public InventorySlot(ItemStack itemStack, bool isFlipped, bool isOrigin) {
            ItemStack = itemStack;
            Model = null;
            IsOrigin = isOrigin;
            IsFlipped = isFlipped;
            CellSlot = new Vector2Int(-1, -1);
        }
        public InventorySlot() {
            ItemStack = null;
            Model = null;
            IsOrigin = false;
            IsFlipped = false;
            CellSlot = new Vector2Int(-1, -1);
        }
        public InventorySlot(Vector2Int cellSlot) {
            ItemStack = null;
            Model = null;
            IsOrigin = false;
            IsFlipped = false;
            CellSlot = cellSlot;
        }
        public override string ToString() {
            return $"{CellSlot} {ItemStack} Origin:{IsOrigin} IsFlipped {IsFlipped}";
        }
    }
}