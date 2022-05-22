using System;
using UnityEngine;

namespace _Project.Scripts.Components.Items {
    
    public enum StackSize {
        BIG = 0,
        MEDIUM = 32,
        SMALL = 64
    }
    public class ItemStack{
        public Item Item { get; set; }

        public int Count { get; set; }

        public StackSize Size;

        public ItemStack(Item item) {
            Item = item;
            Count = 0;
            SetSize();
        }
        public ItemStack(Item item, int count) {
            Item = item;
            Count = count;
            SetSize();
        }
        private void SetSize() {
            switch (Item.Size) {
                case ItemSize.BIG:
                    Size = StackSize.BIG;
                    break;
                case ItemSize.MEDIUM:
                    Size = StackSize.MEDIUM;
                    break;
                case ItemSize.SMALL:
                    Size = StackSize.SMALL;
                    break;
            }
        }
    }
}