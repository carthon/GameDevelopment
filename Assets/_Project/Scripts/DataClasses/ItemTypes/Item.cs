using System;
using _Project.Scripts.DataClasses.ItemActions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.DataClasses.ItemTypes {
    [CreateAssetMenu(menuName = "Items/Stackable Item", fileName = "Stackable Item")]
    [Serializable]
    public class Item : ScriptableObject, IEquatable<Item> {
        [Header("Item Information")]
        public string itemName;
        public GameObject itemPrefab;
        public GameObject model;
        public int Width;
        public int Height;
        [SerializeField, HideInInspector]
        private string _assetGuid;
        public string Id => _assetGuid;
        [field: TextArea]
        public string description;
        private IAction _mainAction;
        private IAction _secondaryAction;

        [SerializeField] private int maxStackSize;
        
        private void OnValidate()
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);
            _assetGuid   = AssetDatabase.AssetPathToGUID(path);
            EditorUtility.SetDirty(this);
#endif
        }
        public bool Equals(Item other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return base.Equals(other) &&
                itemName == other.itemName &&
                Equals(itemPrefab, other.itemPrefab) &&
                Width == other.Width && Height == other.Height && description == other.description && maxStackSize == other.maxStackSize;
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
                hashCode = (hashCode * 397) ^ (itemPrefab != null ? itemPrefab.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
                hashCode = (hashCode * 397) ^ (description != null ? description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ maxStackSize;
                return hashCode;
            }
        }
        public override string ToString() {
            return $"Id:{Id} Name:{itemName} Size:{Width}x{Height} Description:{description}";
        }
        public bool TryDoMainAction() => _mainAction.TryDoAction();
        public bool TryDoSecondaryAction() => _secondaryAction.TryDoAction();
    }
}