using System;
using System.Collections.Generic;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.DiegeticUI {
    public class ContainerRenderer : MonoBehaviour {
        [SerializeField]
        private float slotSize = 0.1f;
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
#if UNITY_EDITOR
            if (_parent == null)
                return;
            
            float slotsPerRow = (float)Math.Sqrt(_inventory.Size);
            float itemsPerRow = (float)Math.Sqrt(64);
            
            Vector3 parentPosition = _parent.transform.position;
            Vector3 centerOfGrid = parentPosition - new Vector3((slotsPerRow - 1) * slotSize, 0, (slotsPerRow - 1) * slotSize) / 2;
            List<ItemStack> items = _inventory.GetInventorySlots();
            float itemSize = slotSize / itemsPerRow;
            foreach (ItemStack item in items) {
                // ReSharper disable once PossibleLossOfFraction
                Vector3 slotPosition = new Vector3(item.GetSlotID() % slotsPerRow, 0, item.GetSlotID() / (int) slotsPerRow) * slotSize;
                if (!item.IsEmpty()) {
                    for (int i = 0; i < item.GetCount(); i++) {
                        Gizmos.DrawWireCube(WorldCellPositionOfGrid(i, (int)itemsPerRow, centerOfGrid + slotPosition, itemSize), Vector3.one * itemSize);
                    }
                }
                Gizmos.DrawWireCube(WorldCellPositionOfGrid(item.GetSlotID(), (int) slotsPerRow, parentPosition, slotSize), Vector3.one * slotSize);
            }
#endif
        }
        private Vector3 WorldCellPositionOfGrid(int cellIndex, int cellPerRow, Vector3 gridPosition, float cellSize) {
            Vector3 centerOfGrid = gridPosition - new Vector3((cellPerRow - 1) * cellSize, 0, (cellPerRow - 1) * cellSize) / 2;
            // ReSharper disable once PossibleLossOfFraction
            Vector2 cellPosition = new Vector2(cellIndex % cellPerRow, cellIndex / cellPerRow) * cellSize;
            return centerOfGrid + new Vector3(cellPosition.x, 0, cellPosition.y);
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
                    slotIDItemsDict.Add(item.GetSlotID(), SpawnItemsInStack(item));
                }
            }
        }
        private List<GameObject> SpawnItemsInStack(ItemStack itemStack) {
            List<GameObject> listOfRenders = new List<GameObject>();
            int slotId = itemStack.GetSlotID();
            float slotsPerRow = (float) Math.Sqrt(_inventory.Size);
            int startListSize = 0;
            
            Vector3 parentPosition = _parent.transform.position;
            Vector3 centerOfGrid = parentPosition - new Vector3((slotsPerRow - 1) * slotSize, 0, (slotsPerRow - 1) * slotSize) / 2;
            
            if (slotIDItemsDict.TryGetValue(slotId, out List<GameObject> objList)) {
                startListSize = objList.Count;
                listOfRenders = objList;
            }
            for (int i = startListSize; i < itemStack.GetCount(); i++) {
                float itemsPerRow = (float)Math.Sqrt(itemStack.Item.GetMaxStackSize());
                float itemSize = slotSize / itemsPerRow;
                // ReSharper disable once PossibleLossOfFraction
                Vector3 slotPosition = new Vector3(itemStack.GetSlotID() % slotsPerRow, 0, itemStack.GetSlotID() / (int) slotsPerRow) * slotSize;
                GameObject renderedItem = CreateItemModel(itemStack);
                renderedItem.transform.position = WorldCellPositionOfGrid(i, (int) itemsPerRow,centerOfGrid + slotPosition, itemSize);
                renderedItem.transform.localScale *= itemSize;
                listOfRenders.Add(renderedItem);
            }
            return listOfRenders;
        }
        private void UpdateInventorySlot(int slotId, ItemStack itemStack) {
            if (slotIDItemsDict.TryGetValue(slotId, out List<GameObject> objectsList)) {
                objectsList.ForEach(obj => obj.SetActive(false));
                slotIDItemsDict[slotId] = SpawnItemsInStack(itemStack);
            } else {
                slotIDItemsDict.Add(itemStack.GetSlotID(), SpawnItemsInStack(itemStack));
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
        private GameObject CreateItemModel(ItemStack itemStack) {
            GameObject renderedItem = Instantiate(itemStack.Item.modelPrefab, _parent);
            //if (renderedItem.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            renderedItem.SetActive(false);
            return renderedItem;
        }
    }
}