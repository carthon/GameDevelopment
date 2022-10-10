using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class ItemStackSpawner : MonoBehaviour {
        public PrefabTuple item;
        public int count;
        public void Update() {
            if (NetworkManager.Singleton.IsServer) {
                var itemRendered = Instantiate(item.model, transform);
                var pickable = itemRendered.AddComponent<Grabbable>();
                if (pickable) {
                    var lootTable = new LootTable();
                    lootTable.AddToLootTable(item.item, count);
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