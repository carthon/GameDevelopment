using UnityEngine;

namespace _Project.Scripts.Components {
    public class ItemStackSpawner : MonoBehaviour {
        public Item item;
        public int count;
        public void Update() {
            if (NetworkManager.Singleton.IsServer) {
                var itemRendered = Instantiate(item.modelPrefab, transform);
                var pickable = itemRendered.GetComponent<Grabbable>();
                if (pickable) {
                    var lootTable = new LootTable();
                    lootTable.AddToLootTable(item, count);
                    pickable.SetLootTable(lootTable);
                    pickable.Initialize(Grabbable.nextId, item);
                    Grabbable.nextId++;
                }
                Destroy(this);
            } else if (NetworkManager.Singleton.IsClient)
                Destroy(this.gameObject);
        }
    }
}