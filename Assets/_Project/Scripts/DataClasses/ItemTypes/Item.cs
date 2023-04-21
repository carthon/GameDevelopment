using System;
using _Project.Scripts.DataClasses.ItemActions;
using EditorAttributes;
using UnityEngine;

namespace _Project.Scripts.DataClasses.ItemTypes {
    public enum ItemSize: int {
        SMALL = 0,
        MEDIUM,
        BIG
    }
    [CreateAssetMenu(menuName = "Items/Stackable Item", fileName = "Stackable Item")]
    [Serializable]
    public class Item : ScriptableObject, IEquatable<Item> {
        [Header("Item Information")]
        public string itemName;
        public GameObject modelPrefab;
        public ItemSize Size;
        [ScriptableObjectId]
        public string id;
        [field: TextArea]
        public string description;
        private IAction _mainAction;
        private IAction _secondaryAction;

        [SerializeField] private int maxStackSize;
        public bool Equals(Item other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return base.Equals(other) &&
                itemName == other.itemName &&
                Equals(modelPrefab, other.modelPrefab) &&
                Size == other.Size && description == other.description && maxStackSize == other.maxStackSize;
        }
        public bool IsStackable() {
            return maxStackSize != 1;
        }

        public int GetMaxStackSize() {
            return maxStackSize;
        }
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((Item) obj);
        }
        public override int GetHashCode() {
            unchecked {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (itemName != null ? itemName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (modelPrefab != null ? modelPrefab.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Size;
                hashCode = (hashCode * 397) ^ (description != null ? description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ maxStackSize;
                return hashCode;
            }
        }
        public override string ToString() {
            return $"Id:{id} Name:{itemName} Size:{Size} Description:{description}";
        }
        public bool TryDoMainAction() => _mainAction.TryDoAction();
        public bool TryDoSecondaryAction() => _secondaryAction.TryDoAction();
    }
}