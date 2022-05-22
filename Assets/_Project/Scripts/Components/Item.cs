using UnityEngine;

namespace _Project.Scripts.Components {
    public enum ItemSize {
        BIG = 3,
        MEDIUM = 2,
        SMALL = 1
    }
    public class Item : ScriptableObject {
        [Header("Item Information")]
        public Sprite itemIcon;
        public string Name { get; set; }
        public GameObject modelPrefab;
        public ItemSize Size;
        [field: TextArea]
        public string Description { get; set; }
        public bool isStackable;
        public int intID => GetInstanceID();

        private void Init(Sprite itemIcon, string itemName, GameObject modelPrefab, ItemSize itemSize) {
            this.itemIcon = itemIcon;
            this.Name = itemName;
            this.modelPrefab = modelPrefab;
            this.Size = itemSize;
        }

        public static Item CreateItem(Sprite itemIcon, string itemName, GameObject modelPrefab, ItemSize itemSize) {
            var item = CreateInstance<Item>();
            item.Init(itemIcon, itemName, modelPrefab, itemSize);
            return item;
        }
    }
}
