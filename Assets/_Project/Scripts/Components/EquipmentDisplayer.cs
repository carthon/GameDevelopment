using System;
using UnityEngine;

public class EquipmentDisplayer : MonoBehaviour {
    [SerializeField] private Transform _overrideTransform;
    [SerializeField] private BodyPart bodyPart;
    [SerializeField] private ItemStack _currentEquipedItem;
    public int Id { get; set; }

    public Transform OverrideTransform { get => _overrideTransform; set => _overrideTransform = value; }
    public ItemStack CurrentEquipedItem {
        get => _currentEquipedItem; 
        set { _currentEquipedItem = value; } 
    }
    public GameObject CurrentItemModel { get; set; }
    public bool IsActive => CurrentItemModel != null && CurrentItemModel.activeSelf && !CurrentEquipedItem.Equals(ItemStack.EMPTY);

    private void Start() {
        _overrideTransform = transform;
        _currentEquipedItem = ItemStack.EMPTY;
    }

    public BodyPart GetBodyPart() {
        return bodyPart;
    }
}