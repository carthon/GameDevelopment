using System;
using _Project.Scripts.Components;
using _Project.Scripts.UI;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class EquipmentSlotHandler : MonoBehaviour {
        public Transform parentOverride;
        public Item currentItemOnSlot;

        public GameObject currentItemModel;

        public void UnloadItemModel() {
            if (currentItemModel != null)
                currentItemModel.SetActive(false);
        }

        public void UnloadItemAndDestroy() {
            if (currentItemModel != null) {
                currentItemOnSlot = null;
                Destroy(currentItemModel);
            }
        }

        public void LoadItemModel(UIItemSlot item) {
            LoadItemModel(item.GetItemStack().Item);
        }

        public void LoadItemModel(Item item) {
            UnloadItemAndDestroy();
            if (item == null) {
                UnloadItemModel();
                return;
            }
            currentItemOnSlot = item;
            GameObject model = Instantiate(item.modelPrefab) as GameObject;
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
    }
}