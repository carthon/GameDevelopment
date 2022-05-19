using System;
using UnityEngine;

namespace _Project.Scripts.Components.Items {
    public class ItemStack{
        public Item Item { get; set; }

        public int Count { get; set; }
        
        public ItemStack(Item item) {
            Item = item;
            Count = 0;
        }
        public ItemStack(Item item, int count) {
            Item = item;
            Count = count;
        }
    }
}