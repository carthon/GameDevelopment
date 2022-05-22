using System;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.UI;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class EquipmentSlotHandler : MonoBehaviour {
        public Transform parentOverride;
        public ItemStack currentItemOnSlot;

        public GameObject currentItemModel;
        public Inventory lastInventory;
        [SerializeField] private EquipmentSlot _equipmentSlot;
        public EquipmentSlot GetEquipmentSlot() => _equipmentSlot;
        public void UnloadItemModel() {
            if (currentItemModel != null)
                currentItemModel.SetActive(false);
        }


        public void UnloadItemAndDestroy() {
            if (currentItemModel != null) {
                currentItemOnSlot = null;
                lastInventory = null;
                Destroy(currentItemModel);
            }
        }

        public void LoadItemModel(UIItemSlot item) {
            LoadItemModel(item.PullItemStack());
            lastInventory = item.Parent;
        }

        public void LoadItemModel(ItemStack itemStack, Inventory lastInventory) {
            LoadItemModel(itemStack);
            this.lastInventory = lastInventory;
        }

        public void LoadItemModel(ItemStack itemStack) {
            UnloadItemAndDestroy();
            if (itemStack == null) {
                UnloadItemModel();
                return;
            }
            currentItemOnSlot = itemStack;
            GameObject model = Instantiate(itemStack.Item.modelPrefab) as GameObject;
            if (model != null)
                if (parentOverride != null)
                    model.transform.parent = parentOverride;
                else
                    model.transform.parent = transform;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            currentItemModel = model;
        }

        public BodyPart GetEquipmentBodyPart() => _equipmentSlot.GetBodyPart();
    }
}