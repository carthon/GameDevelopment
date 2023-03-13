using System;
using UnityEngine;

public enum ItemSize {
    BIG = 3,
    MEDIUM = 2,
    SMALL = 1
}
[CreateAssetMenu(menuName = "Items/Stackable Item", fileName = "Stackable Item")]
public class Item : ScriptableObject, IEquatable<Item> {
    [Header("Item Information")]
    public Sprite itemIcon;
    public Mesh item3dIcon;
    public Material iconMaterial;
    public string name;
    public GameObject modelPrefab;
    public ItemSize Size;
    [ScriptableObjectId]
    public string id;
    [field: TextArea]
    public string description;

    [SerializeField] private int maxStackSize;
    public int intID => GetInstanceID();
    public bool Equals(Item other) {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return base.Equals(other) &&
            Equals(itemIcon, other.itemIcon) &&
            name == other.name &&
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
            hashCode = (hashCode * 397) ^ (itemIcon != null ? itemIcon.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (name != null ? name.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (modelPrefab != null ? modelPrefab.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (int) Size;
            hashCode = (hashCode * 397) ^ (description != null ? description.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ maxStackSize;
            return hashCode;
        }
    }
    public override string ToString() {
        return $"Id:{id} Name:{name} Size:{Size} Description:{description}";
    }
}