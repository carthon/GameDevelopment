using System;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.UI;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class EquipmentSlotHandler : MonoBehaviour {
        public Transform parentOverride;
        public Inventory currentInventory;
        private UIEquipmentSlot parentUI;

        public GameObject currentItemModel;
        [SerializeField] private EquipmentSlot _equipmentSlot;
        public EquipmentSlot GetEquipmentSlot() => _equipmentSlot;
        
        private void Start() {
            currentInventory = new Inventory("EquipmentSlot " + _equipmentSlot.GetBodyPart().ToString(), 1);
            currentInventory.OnInventoryChange += HandleInventoryChange;
        }

        private void OnEnable() {
            //currentInventory.OnInventorySwap += HandleItemSwap;
        }
        private void HandleInventoryChange(Inventory inventory, int slot) {
            GetParent().ResetData();
            ReloadItemModel();
            GetParent().SetData(inventory.GetItem(slot));
        }
        private void HandleItemSwap(ItemStack itemStack, ItemStack otherItemStack) {
            UnloadItemAndDestroy();
            LoadItemModel(currentInventory.GetItem(0));
        }
        private void HandleItemEquip(int slot) {
            LoadItemModel(currentInventory.GetItem(slot));
        }
        private void HandleItemUnEquip(int slot) {
            UnloadItemAndDestroy();
        }
        public void UnloadItemModel() {
            if (currentItemModel != null)
                currentItemModel.SetActive(false);
        }
        public void UnloadItemAndDestroy() {
            if (currentItemModel != null) {
                currentInventory.SetStack(0, ItemStack.EMPTY);
                Destroy(currentItemModel);
            }
        }
        public void DestroyItemInstance() {
            if (currentItemModel != null) {
                Destroy(currentItemModel);
            }
        }

        public void ReloadItemModel() {
            ItemStack itemStack = currentInventory.GetItem(0);
            DestroyItemInstance();
            if (itemStack.IsEmpty()) {
                UnloadItemModel();
                return;
            }
            GameObject model = Instantiate(itemStack.Item.modelPrefab) as GameObject;
            if (model != null) model.transform.parent = parentOverride != null ? parentOverride : transform;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            currentItemModel = model;
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
        public void SetParent(UIEquipmentSlot newParentUI) => parentUI = newParentUI;
        public UIEquipmentSlot GetParent() => parentUI;
    }
}