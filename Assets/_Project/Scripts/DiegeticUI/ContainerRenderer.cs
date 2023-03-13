using System;
using System.Collections.Generic;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.DiegeticUI {
    public class ContainerRenderer : MonoBehaviour {
        [SerializeField]
        private float itemOffset = 5f;
        private Inventory _inventory;
        private Transform _parent;
        private Dictionary<int, List<GameObject>> slotIDItemsDict;
        private static ContainerRenderer _singleton;
        private bool toggled;
        public Transform SpawnPoint { get => _parent; }
        public static ContainerRenderer Singleton
        {
            get => _singleton;
            private set {
                if (_singleton == null)
                    _singleton = value;
                else if(_singleton != null) {
                    Debug.Log($"{nameof(ContainerRenderer)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }
        private void Awake() {
            Singleton = this;
        }

        private void OnDrawGizmos() {
            if (_parent == null)
                return;
            float slotSize = 0.1f;
            float itemsPerRow = (float)Math.Sqrt(_inventory.Size) - 1;
            Vector3 parentPosition = _parent.transform.position;
            Vector3 centerOfGrid = parentPosition - new Vector3(itemsPerRow * itemOffset, 0,itemsPerRow * itemOffset) / 2;
            Gizmos.DrawSphere(parentPosition, 0.1f);
            for (int i = 0, j = 0, total = 0; total < _inventory.Size; total++) {
                Vector3 slotPosition = new Vector3( i * itemOffset, 0,j * itemOffset);
                Gizmos.DrawCube(centerOfGrid + slotPosition, Vector3.one * slotSize);
                GUIUtils.DrawWorldText(total.ToString(),centerOfGrid + slotPosition + Vector3.up * itemOffset);
                if (i < Math.Sqrt(_inventory.Size) - 1) {
                    i++;
                }
                else {
                    i = 0;
                    j++;
                }
            }
        }

        public void InitializeRenderer(Inventory inventory, Transform parent) {
            _inventory = inventory;
            _parent = parent;
            slotIDItemsDict = new Dictionary<int, List<GameObject>>();
            _inventory.OnSlotChange += UpdateInventorySlot;
            toggled = false;
            List<ItemStack> items = _inventory.GetInventorySlots();
            foreach (ItemStack item in items) {
                if (!item.IsEmpty()) {
                    slotIDItemsDict.Add(item.GetSlotID(), FillListWithItems(item, item.GetSlotID()));
                }
            }
        }
        private List<GameObject> FillListWithItems(ItemStack itemStack, int slotId) {
            List<GameObject> listOfRenders = new List<GameObject>();
            int startListSize = 0;
            if (slotIDItemsDict.TryGetValue(slotId, out List<GameObject> objList)) {
                startListSize = objList.Count;
                listOfRenders = objList;
            }
            for (int i = startListSize; i < itemStack.GetCount(); i++) {
                listOfRenders.Add(CreateItemModel(itemStack, new Vector2(0, i)));
            }
            return listOfRenders;
        }
        private void UpdateInventorySlot(int slotId, ItemStack itemStack) {
            if (slotIDItemsDict.TryGetValue(slotId, out List<GameObject> objectsList)) {
                objectsList.ForEach(obj => obj.SetActive(false));
                slotIDItemsDict[slotId] = FillListWithItems(itemStack, slotId);
            } else {
                slotIDItemsDict.Add(itemStack.GetSlotID(), FillListWithItems(itemStack, slotId));
            }
        }
        public void ToggleRender(bool render) {
            if (toggled == render)
                return;
            foreach (List<GameObject> objectsList in slotIDItemsDict.Values) {
                objectsList.ForEach(obj => obj.SetActive(render));
            }
            toggled = render;
        }
        private GameObject CreateItemModel(ItemStack itemStack, Vector2 slotPosition) {
            GameObject renderedItem = Instantiate(itemStack.Item.modelPrefab, _parent);
            renderedItem.transform.position = _parent.position + Vector3.forward * itemStack.GetSlotID() * itemOffset + new Vector3(slotPosition.x, 0, -slotPosition.y);
            if (renderedItem.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            renderedItem.SetActive(false);
            return renderedItem;
        }
    }
}