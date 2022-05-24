using System;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.UI;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class EquipmentSlotHandler : MonoBehaviour {
        public Transform parentOverride;
        public Inventory currentInventory;

        public GameObject currentItemModel;
        [SerializeField] private EquipmentSlot _equipmentSlot;
        public EquipmentSlot GetEquipmentSlot() => _equipmentSlot;
        
        private void Awake() {
            currentInventory = new Inventory(1);
        }
        public void UnloadItemModel() {
            if (currentItemModel != null)
                currentItemModel.SetActive(false);
        }
        public void UnloadItemAndDestroy() {
            if (currentItemModel != null) {
                currentInventory.TakeStackFromSlot(0);
                Destroy(currentItemModel);
            }
        }

        public void LoadItemModel(ItemStack itemStack) {
            UnloadItemAndDestroy();
            if (itemStack.IsEmpty()) {
                UnloadItemModel();
                return;
            }
            currentInventory.AddItem(new ItemStack(itemStack));
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

        public bool IsEmpty() => currentInventory.GetItem(0).IsEmpty();
        public BodyPart GetEquipmentBodyPart() => _equipmentSlot.GetBodyPart();
    }
}