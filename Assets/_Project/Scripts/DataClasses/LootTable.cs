using System;
using System.Collections.Generic;
using _Project.Scripts.DataClasses.ItemTypes;
using UnityEngine;

namespace _Project.Scripts.DataClasses {
    [Serializable]
    public class LootTable {
        [SerializeField]
        private List<LootTableData> _lootTables;
        public LootTable(List<LootTableData> lootTables) {
            _lootTables = lootTables;
        }
        public LootTable() {
            _lootTables = new List<LootTableData>();
        }

        public List<LootTableData> LootTables
        {
            get => _lootTables;
            set => _lootTables = value;
        }
        public void AddToLootTable(ItemStack itemStack) {
            _lootTables.Add(new LootTableData(itemStack.Item, itemStack.GetCount()));
        }
        public void AddToLootTable(Item item, int count) {
            _lootTables.Add(new LootTableData(item, count));
        }

        public bool IsEmpty() {
            var totalItems = 0;
            foreach (var data in _lootTables) totalItems += data.Count;
            return totalItems <= 0;
        }
    }
}