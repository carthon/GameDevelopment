using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class EquipmentHandler : MonoBehaviour {
        [SerializeField]
        private List<EquipmentDisplayer> _equipmentSlots;
        public List<EquipmentDisplayer> EquipmentDisplayers => _equipmentSlots;

        private void Awake() {
            _equipmentSlots = GetComponentsInChildren<EquipmentDisplayer>().ToList();
            for (int i = 0; i < _equipmentSlots.Count; i++) {
                _equipmentSlots[i].Id = i;
            }
        }

        public void UnloadItemModel(EquipmentDisplayer equipmentDisplayer) {
            if (equipmentDisplayer.CurrentItemModel != null) {
                equipmentDisplayer.CurrentItemModel.SetActive(false);
                equipmentDisplayer.CurrentEquipedItem = ItemStack.EMPTY;
            }
        }
        public void UnloadItemModel(int equipmentDisplayId) => UnloadItemModel(_equipmentSlots[equipmentDisplayId]);
        public void UnloadItemAndDestroy(EquipmentDisplayer equipmentDisplayer) {
            if (equipmentDisplayer.CurrentItemModel != null) {
                Destroy(equipmentDisplayer.CurrentItemModel);
                equipmentDisplayer.CurrentEquipedItem = ItemStack.EMPTY;
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

            var model = Instantiate(itemStack.Item.modelPrefab);
            if (model != null) model.transform.parent = equipmentDisplayer.OverrideTransform != null ? equipmentDisplayer.OverrideTransform : transform;
            foreach (var componentsInChild in model.GetComponentsInChildren<Collider>()) componentsInChild.enabled = false;
            if (model.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            equipmentDisplayer.CurrentItemModel = model;
            equipmentDisplayer.CurrentEquipedItem = itemStack;
        }

        public void LoadItemModel(ItemStack itemStack, BodyPart bodyPart) {
            var equipmentSlot = GetEquipmentSlotByBodyPart(bodyPart);
            LoadItemModel(itemStack, equipmentSlot);
        }
        public void LoadItemModel(ItemStack itemStack, int equipmentId) => LoadItemModel(itemStack, _equipmentSlots[equipmentId]);

        public void ReloadItemModel(BodyPart bodyPart) {
            var equipmentSlot = GetEquipmentSlotByBodyPart(bodyPart);
            ReloadItemModel(equipmentSlot);
        }
        public void ReloadItemModel(int equipmentDisplayer) => ReloadItemModel(_equipmentSlots[equipmentDisplayer]);
        public void UnloadItemModel(BodyPart bodyPart) {
            var equipmentSlot = GetEquipmentSlotByBodyPart(bodyPart);
            UnloadItemModel(equipmentSlot);
        }

        public EquipmentDisplayer GetEquipmentSlotByBodyPart(BodyPart bodyPart) {
            return _equipmentSlots.Find(equimentSlot => equimentSlot.GetBodyPart() == bodyPart);
        }
        public EquipmentDisplayer GetEquipmentSlot(int slot) => _equipmentSlots[slot];
    }
}