using System;
using UnityEngine;

namespace _Project.Scripts.Components.Items {
    public class ItemSlot{
        public Item Item { get; set; }

        public bool isLink;

        public Inventory Parent { get; private set; }
        public int NSlot { get; set; }
        
        public ItemSlot(Item item) {
            Item = item;
            Parent = null;
            isLink = false;
        }
        public ItemSlot(Item item, Inventory parent, int nSlot) {
            Item = item;
            Parent = parent;
            NSlot = nSlot;
            isLink = false;
        }
        public ItemSlot(Item item, Inventory parent, int nSlot, bool isLink) {
            Item = item;
            Parent = parent;
            NSlot = nSlot;
            this.isLink = isLink;
        }
    }
}