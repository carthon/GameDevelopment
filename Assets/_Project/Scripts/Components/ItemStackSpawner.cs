using UnityEngine;

namespace _Project.Scripts.Components {
    public class ItemStackSpawner : MonoBehaviour {
        public Item item;
        public int count;
        public void Awake() {
            var itemRendered = Instantiate(item.modelPrefab, transform);
            var pickable = itemRendered.AddComponent<Grabbable>();
            if (pickable) {
                var lootTable = new LootTable();
                lootTable.AddToLootTable(item, count);
                pickable.SetLootTable(lootTable);
            }
            Destroy(this);
        }
    }
}