using UnityEngine;

namespace _Project.Scripts.Components {
    public class Item : ScriptableObject {
        [Header("Item Information")]
        public Sprite itemIcon;
        public string itemName;
        public GameObject modelPrefab;

        private void Init(Sprite itemIcon, string itemName, GameObject modelPrefab) {
            this.itemIcon = itemIcon;
            this.itemName = itemName;
            this.modelPrefab = modelPrefab;
        }

        public static Item CreateItem(Sprite itemIcon, string itemName, GameObject modelPrefab) {
            var item = CreateInstance<Item>();
            item.Init(itemIcon, itemName, modelPrefab);
            return item;
        }
    }
}
