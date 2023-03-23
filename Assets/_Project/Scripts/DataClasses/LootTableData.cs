using System;
using _Project.Scripts.DataClasses.ItemTypes;
using UnityEngine;

namespace _Project.Scripts.DataClasses {
    [Serializable]
    public class LootTableData {
        [SerializeField]
        private Item item;
        [SerializeField]
        private int count;
        public LootTableData(Item item, int count) {
            Item = item;
            Count = count;
        }
        public Item Item
        {
            get => item;
            set => item = value;
        }
        public int Count
        {
            get => count;
            set => count = value;
        }
    }
}