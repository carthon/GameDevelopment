using System;
using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Handlers;
using _Project.Scripts.Utils;
using QuickOutline.Scripts;
using UnityEngine;

namespace _Project.Scripts.DiegeticUI {
    public class ContainerRenderer : MonoBehaviour {
        [SerializeField]
        private float slotSize = 0.1f;
        private Inventory _inventory;
        private Transform _parent;
        private Dictionary<int, List<GameObject>> slotIDItemsDict;
        private Dictionary<int, Outline> slotIDOutlinesDict;
        private Dictionary<int, Bounds> slotIDItemBounds;
        private static ContainerRenderer _singleton;
        private bool toggled;
        private bool needsUpdate;
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
        
        public void InitializeRenderer(Inventory inventory, Transform parent) {
            _inventory = inventory;
            _parent = parent;
            slotIDItemsDict = new Dictionary<int, List<GameObject>>();
            slotIDOutlinesDict = new Dictionary<int, Outline>();
            slotIDItemBounds = new Dictionary<int, Bounds>();
            _inventory.OnSlotChange += UpdateInventorySlot;
            toggled = false;
            needsUpdate = true;
            UIHandler.Instance.OnSelectedItem += OnSelectedItem;
            List<ItemStack> items = _inventory.GetInventorySlots();
            foreach (ItemStack item in items) {
                if (!item.IsEmpty()) {
                    slotIDItemBounds.Add(item.GetSlotID(), GetItemBounds(item));
                    slotIDItemsDict.Add(item.GetSlotID(), SpawnItemsInStack(item));
                }
            }
        }
        private void OnSelectedItem(Outline obj) {
            ItemStack itemSelected = _inventory.GetItemStack(Int32.Parse(obj.transform.name));
            if (!itemSelected.IsEmpty()) {
                UIHandler.Instance.UpdateWatchedVariables("mouseSelected", $"Selected:{itemSelected.Item.name}");
                UIHandler.Instance.GrabbedItems = new List<Transform>();
                UIHandler.Instance.LastGrabbedItemsLocalPosition = new List<Vector3>();
                for (int i = 0; i < obj.transform.childCount; i++) {
                    Transform child = obj.transform.GetChild(i);
                    UIHandler.Instance.GrabbedItems.Add(child);
                    UIHandler.Instance.LastGrabbedItemsLocalPosition.Add(child.localPosition);
                }
            }
        }

        private void OnDrawGizmos() {
#if UNITY_EDITOR
            if (_parent == null)
                return;
            List<ItemStack> items = _inventory.GetInventorySlots();
            foreach (ItemStack item in items) {
                if (slotIDOutlinesDict.ContainsKey(item.GetSlotID())) {
                    GUIUtils.DrawWorldText(item.GetSlotID().ToString(), slotIDOutlinesDict[item.GetSlotID()].transform.position);
                    Gizmos.DrawWireCube(slotIDOutlinesDict[item.GetSlotID()].transform.position, Vector3.one * slotSize);
                }
            }
#endif
        }
        private Bounds GetItemBounds(ItemStack itemStack) {
            Bounds bounds = new Bounds();
            if (itemStack.Item.modelPrefab.TryGetComponent(out Renderer itemRenderer)) {
                bounds = itemRenderer.bounds;
            }
            else {
                itemRenderer = itemStack.Item.modelPrefab.GetComponentInChildren<Renderer>();
                if (itemRenderer != null)
                    bounds = itemRenderer.bounds;
            }
            return bounds;
        }

        private List<GameObject> SpawnItemsInStack(ItemStack itemStack) {
            List<GameObject> listOfRenders = new List<GameObject>();
            int slotId = itemStack.GetSlotID();
            float slotsPerRow = (float) Math.Sqrt(_inventory.Size);
            
            Transform slotParent = new GameObject(slotId.ToString()).transform;
            Vector3 centerOfSlot = CellPositionFromGrid(itemStack.GetSlotID(), (int) slotsPerRow, Vector3.zero, slotSize);
            slotParent.SetParent(_parent);
            slotParent.localPosition = centerOfSlot;
            slotParent.rotation = _parent.rotation;
            if(!slotIDOutlinesDict.ContainsKey(slotId)) {
                slotIDOutlinesDict.Add(slotId, UIHandler.AddOutlineToObject(slotParent.gameObject, Color.white));
            }
            
            float itemsPerSlotRow = itemStack.Item.IsStackable() ? (float)Math.Sqrt(itemStack.Item.GetMaxStackSize()) / 4 : 1;
            float itemsPerSlot = itemsPerSlotRow * itemsPerSlotRow;
            float itemSize = slotSize / itemsPerSlotRow;
            Vector3 itemExtents = slotIDItemBounds[slotId].extents * 2;
            int startListSize = 0;

            if (slotIDItemsDict.TryGetValue(slotId, out List<GameObject> objList)) {
                startListSize = objList.Count;
                listOfRenders = objList;
            }
            centerOfSlot = Vector3.zero;
            for (int i = startListSize, j = 0; i < itemStack.GetCount(); i++) {
                GameObject renderedItem = CreateItemModel(itemStack, slotParent);
                renderedItem.transform.localScale *= itemSize;
                Vector3 cellPosition = CellPositionFromGrid(i, (int) itemsPerSlotRow, centerOfSlot, itemExtents.x * itemSize) + new Vector3(0, itemExtents.y,0) * itemSize * j;
                if (!itemStack.Item.IsStackable()) cellPosition = Vector3.zero;
                renderedItem.transform.localPosition = cellPosition;
                listOfRenders.Add(renderedItem);
                if (i % itemsPerSlot + 1 >= itemsPerSlot) j++;
            }
            return listOfRenders;
        }
        private Vector3 CellPositionFromGrid(int cellIndex, int cellPerRow, Vector3 gridPosition, float cellSize) {
            Vector3 centerOfGrid = gridPosition - new Vector3((cellPerRow - 1) * cellSize, 0, (cellPerRow - 1) * cellSize) / 2;
            // ReSharper disable once PossibleLossOfFraction
            Vector3 cellPosition = new Vector3(cellIndex % cellPerRow, 0, (cellIndex / cellPerRow) % cellPerRow) * cellSize;
            return centerOfGrid + cellPosition;
        }
        private void UpdateInventorySlot(int slotId, ItemStack itemStack) {
            if (slotIDItemsDict.TryGetValue(slotId, out List<GameObject> objectsList)) {
                objectsList.ForEach(obj => obj.SetActive(false));
                slotIDItemBounds[slotId] = GetItemBounds(itemStack);
                slotIDItemsDict[slotId] = SpawnItemsInStack(itemStack);
            } else {
                slotIDItemBounds.Add(itemStack.GetSlotID(), GetItemBounds(itemStack));
                slotIDItemsDict.Add(itemStack.GetSlotID(), SpawnItemsInStack(itemStack));
            }
            needsUpdate = true;
        }
        public void ToggleRender(bool render) {
            if (toggled == render)
                return;
            foreach (KeyValuePair<int, List<GameObject>> keyValuePair in slotIDItemsDict) {
                keyValuePair.Value.ForEach(obj => obj.SetActive(render));
                if (render && needsUpdate) {
                    slotIDOutlinesDict[keyValuePair.Key].ReloadRenderers();
                }
            }
            needsUpdate = false;
            toggled = render;
        }
        private GameObject CreateItemModel(ItemStack itemStack, Transform slotParent) {
            GameObject renderedItem = Instantiate(itemStack.Item.modelPrefab, slotParent);
            if (renderedItem.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            renderedItem.SetActive(false);
            return renderedItem;
        }
    }
}