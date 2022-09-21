using UnityEngine;

namespace _Project.Scripts.Components {
    public class Grabbable : MonoBehaviour {
        [SerializeField]
        private LootTable _lootTable;

        public void SetLootTable(LootTable lootTable) {
            _lootTable = lootTable;
        }
        public LootTable GetLootTable() {
            return _lootTable;
        }
    }
}