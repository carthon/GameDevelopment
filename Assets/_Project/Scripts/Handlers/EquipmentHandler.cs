using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EquipmentHandler : MonoBehaviour {
    [SerializeField]
    private List<EquipmentDisplayer> _equipmentSlots;

    private void Awake() {
        _equipmentSlots = GetComponentsInChildren<EquipmentDisplayer>().ToList();
    }

    public void UnloadItemModel(EquipmentDisplayer equipmentDisplayer) {
        if (equipmentDisplayer.CurrentItemModel != null)
            equipmentDisplayer.CurrentItemModel.SetActive(false);
    }
    public void UnloadItemAndDestroy(EquipmentDisplayer equipmentDisplayer) {
        if (equipmentDisplayer.CurrentItemModel != null) {
            equipmentDisplayer.CurrentEquipedItem = ItemStack.EMPTY;
            Destroy(equipmentDisplayer.CurrentItemModel);
        }
    }
    public void DestroyItemInstance(EquipmentDisplayer equipmentDisplayer) {
        if (equipmentDisplayer.CurrentItemModel != null) Destroy(equipmentDisplayer.CurrentItemModel);
    }

    public void ReloadItemModel(EquipmentDisplayer equipmentDisplayer) {
        var itemStack = equipmentDisplayer.CurrentEquipedItem;
        DestroyItemInstance(equipmentDisplayer);
        if (itemStack.IsEmpty()) {
            UnloadItemModel(equipmentDisplayer);
            return;
        }
        var model = Instantiate(itemStack.Item.modelPrefab);
        if (model != null) model.transform.parent = equipmentDisplayer.OverrideTransform != null ? equipmentDisplayer.OverrideTransform : transform;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;
        equipmentDisplayer.CurrentItemModel = model;
    }

    public void LoadItemModel(ItemStack itemStack, EquipmentDisplayer equipmentDisplayer) {
        UnloadItemAndDestroy(equipmentDisplayer);
        if (itemStack.IsEmpty()) {
            UnloadItemModel(equipmentDisplayer);
            return;
        }
        equipmentDisplayer.CurrentEquipedItem = itemStack;

        var model = Instantiate(itemStack.Item.modelPrefab);
        if (model != null) model.transform.parent = equipmentDisplayer.OverrideTransform != null ? equipmentDisplayer.OverrideTransform : transform;
        foreach (var componentsInChild in model.GetComponentsInChildren<Collider>()) componentsInChild.enabled = false;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;
        equipmentDisplayer.CurrentItemModel = model;
    }

    public void LoadItemModel(ItemStack itemStack, BodyPart bodyPart) {
        var equipmentSlot = GetEquipmentSlotByBodyPart(bodyPart);
        LoadItemModel(itemStack, equipmentSlot);
    }

    public void ReloadItemModel(BodyPart bodyPart) {
        var equipmentSlot = GetEquipmentSlotByBodyPart(bodyPart);
        ReloadItemModel(equipmentSlot);
    }
    public void UnloadItemModel(BodyPart bodyPart) {
        var equipmentSlot = GetEquipmentSlotByBodyPart(bodyPart);
        UnloadItemModel(equipmentSlot);
    }

    public EquipmentDisplayer GetEquipmentSlotByBodyPart(BodyPart bodyPart) {
        return _equipmentSlots.Find(equimentSlot => equimentSlot.GetBodyPart() == bodyPart);
    }
}