using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState;
using _Project.Scripts.Handlers;
using _Project.Scripts.Utils;
using QuickOutline.Scripts;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.DiegeticUI {
    public class ContainerRenderer : MonoBehaviour {
        [SerializeField]
        private float slotSize = 0.1f;
        public Inventory Inventory;
        private Transform _parent;
        private Dictionary<int, List<GameObject>> slotIDItemsDict;
        private Dictionary<int, Outline> slotIDOutlinesDict;
        private Dictionary<int, Bounds> slotIDItemBounds;
        private static ContainerRenderer _singleton;
        private bool toggled;
        private bool needsUpdate;

        private int itemsPerRow = 0;
        private int itemPileHeight = 0;
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
            Inventory = inventory;
            _parent = parent;
            slotIDItemsDict = new Dictionary<int, List<GameObject>>();
            slotIDOutlinesDict = new Dictionary<int, Outline>();
            slotIDItemBounds = new Dictionary<int, Bounds>();
            Inventory.OnSlotChange += UpdateInventorySlot;
            Inventory.OnSlotSwap += OnSlotSwap;
            float slotsPerRow = (float) Math.Sqrt(Inventory.Size);
            toggled = false;
            needsUpdate = true;
            List<ItemStack> items = Inventory.GetInventorySlots();
            for (int i = 0; i < Inventory.Size; i++)
                CreateSlot(i, slotsPerRow);
            foreach (ItemStack item in items) {
                if (!item.IsEmpty()) {
                    slotIDItemBounds.Add(item.GetSlotID(), GetItemBounds(item));
                    slotIDItemsDict.Add(item.GetSlotID(), RenderItemStack(item));
                }
            }
        }

        private void OnDrawGizmos() {
#if UNITY_EDITOR
            if (_parent == null)
                return;
            List<ItemStack> items = Inventory.GetInventorySlots();
            foreach (ItemStack item in items) {
                if (slotIDOutlinesDict.ContainsKey(item.GetSlotID()) && !item.IsEmpty()) {
                    GUIUtils.DrawWorldText(item.GetSlotID().ToString(), slotIDOutlinesDict[item.GetSlotID()].transform.position);
                    Gizmos.DrawWireCube(slotIDOutlinesDict[item.GetSlotID()].transform.position, Vector3.one * slotSize);
                }
            }
#endif
        }
        private void OnSlotSwap(int inventoryId, int otherInventoryId, int slot, int otherSlot) {
            if (slotIDItemsDict.TryGetValue(slot, out List<GameObject> objectsInSlot) && objectsInSlot.Count > 0) {
                Logger.Singleton.Log($"Swapped {slot} for {otherSlot}", Logger.Type.DEBUG);
                Transform parent = slotIDOutlinesDict[slot].transform;
                Transform otherParent = slotIDOutlinesDict[otherSlot].transform;
                Vector3 itemExtents = slotIDItemBounds[slot].extents * 2;
                for (int i= 0; i < objectsInSlot.Count; i++) {
                    objectsInSlot[i].transform.SetParent(otherParent);
                    objectsInSlot[i] = SetRenderPositionInSlot(i, objectsInSlot[i], itemExtents);
                }
                itemsPerRow = 0;
                itemPileHeight = 0;
                if (inventoryId == otherInventoryId && slotIDItemsDict.TryGetValue(otherSlot, out List<GameObject> otherObjectsInSlot) && otherObjectsInSlot.Count > 0) {
                    Vector3 otherItemExtents = slotIDItemBounds[otherSlot].extents * 2;
                    for (int i= 0; i < otherObjectsInSlot.Count; i++) {
                        otherObjectsInSlot[i].transform.SetParent(parent);
                        otherObjectsInSlot[i] = SetRenderPositionInSlot(i, otherObjectsInSlot[i], otherItemExtents);
                    }
                    itemsPerRow = 0;
                    itemPileHeight = 0;
                    (slotIDItemBounds[slot], slotIDItemBounds[otherSlot]) = (slotIDItemBounds[otherSlot], slotIDItemBounds[slot]);
                    (slotIDItemsDict[slot], slotIDItemsDict[otherSlot]) = (slotIDItemsDict[otherSlot], slotIDItemsDict[slot]);
                } else {
                    UpdateInventorySlot(otherSlot, Inventory.GetItemStack(otherSlot));
                    UpdateInventorySlot(slot, Inventory.GetItemStack(slot));
                }
            }
            slotIDOutlinesDict[slot].ReloadRenderers();
            slotIDOutlinesDict[otherSlot].ReloadRenderers();
            needsUpdate = true;
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

        private List<GameObject> RenderItemStack(ItemStack itemStack) {
            List<GameObject> listOfRenders = new List<GameObject>();
            int slotId = itemStack.GetSlotID();

            //float itemsPerSlotRow = itemStack.Item.IsStackable() ? (float)Math.Sqrt(itemStack.Item.GetMaxStackSize()) : 1;
            Vector3 itemExtents = slotIDItemBounds[slotId].extents * 2;
            float itemSize = slotSize;
            int startListSize = 0;

            if (slotIDOutlinesDict.TryGetValue(slotId, out Outline outline) && slotIDItemsDict.ContainsKey(slotId)) {
                List<GameObject> objList = outline.transform.GetComponentsInChildren<Rigidbody>().Select(obj=> obj.gameObject).ToList();
                if(objList.Count > 0 && objList.Count <= itemStack.GetCount()) {
                    startListSize = objList.Count;
                    listOfRenders = objList;
                }
            }
            for (int i = startListSize; i < itemStack.GetCount(); i++) {
                GameObject renderedItem = CreateItemModel(itemStack, slotIDOutlinesDict[slotId].transform);
                renderedItem.transform.localScale *= itemSize;
                listOfRenders.Add(SetRenderPositionInSlot(i, renderedItem, itemExtents));
            }
            itemPileHeight = 0;
            itemsPerRow = 0;
            return listOfRenders;
        }
        private GameObject SetRenderPositionInSlot(int slot, GameObject renderedItem, Vector3 itemExtents) {
            Bounds cellBounds = new Bounds(Vector3.zero, Vector3.one * slotSize);
            Bounds itemBounds = new Bounds(Vector3.zero, itemExtents * slotSize);
            renderedItem.transform.localPosition = ModelPositionFromBounds(slot, Vector3.zero, itemBounds, cellBounds);
            return renderedItem;
        }
        private Vector3 ModelPositionFromBounds(int cellIndex, Vector3 centerOfGrid, Bounds modelBounds, Bounds cellBounds) {
            Vector3 leftBottomCorner = centerOfGrid - cellBounds.extents;
            Vector3 leftBottomItemCenter = leftBottomCorner + modelBounds.extents;
            Vector3 modelExtents = modelBounds.extents * 2;
            modelBounds.center = leftBottomItemCenter + new Vector3(modelExtents.x, 0 , 0) * cellIndex;
            if (modelBounds.Intersects(cellBounds)) {
                return modelBounds.center;
            }
            if (itemsPerRow == 0) itemsPerRow = cellIndex;
            modelBounds.center = leftBottomItemCenter + new Vector3(modelExtents.x, 0 , 0) * (cellIndex % itemsPerRow)
                + new Vector3(0, 0, modelExtents.z) * (cellIndex / itemsPerRow % itemsPerRow);
            if(cellIndex % (itemsPerRow * itemsPerRow) == 0 && cellIndex != 0)
                itemPileHeight++;
            modelBounds.center += new Vector3(0, modelExtents.y, 0) * itemPileHeight;
            return modelBounds.center;
        }
        private Vector3 CellPositionInGrid(int cellIndex, int cellPerRow, Vector3 gridPosition, float cellSize) {
            Vector3 centerOfGrid = gridPosition - new Vector3((cellPerRow - 1) * cellSize, 0, (cellPerRow - 1) * cellSize) / 2;
            // ReSharper disable once PossibleLossOfFraction
            Vector3 cellPosition = new Vector3(cellIndex % cellPerRow, 0, (cellIndex / cellPerRow) % cellPerRow) * cellSize;
            return centerOfGrid + cellPosition;
        }
        private void UpdateInventorySlot(int slotId, ItemStack itemStack) {
            if (slotIDItemsDict.TryGetValue(slotId, out List<GameObject> objectsList) && objectsList.Count > 0) {
                if(itemStack.IsEmpty() || !objectsList[0].Equals(itemStack.Item.modelPrefab)) {
                    objectsList.ForEach(Destroy);
                }
                if (!itemStack.IsEmpty()) {
                    slotIDItemBounds[slotId] = GetItemBounds(itemStack);
                    slotIDItemsDict[slotId] = RenderItemStack(itemStack);
                }
                else
                    slotIDItemsDict.Remove(slotId);
            } else {
                slotIDItemBounds.Add(itemStack.GetSlotID(), GetItemBounds(itemStack));
                slotIDItemsDict.Add(itemStack.GetSlotID(), RenderItemStack(itemStack));
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
        private void CreateSlot(int slotId, float slotsPerRow) {
            Transform slotParent = new GameObject(slotId.ToString()).transform;
            Vector3 centerOfSlot = CellPositionInGrid(slotId, (int) slotsPerRow, Vector3.zero, slotSize);
            slotParent.SetParent(_parent);
            slotParent.localPosition = centerOfSlot + Vector3.up * slotSize / 2;
            slotParent.rotation = _parent.rotation;
            BoxCollider boxCollider = slotParent.gameObject.AddComponent<BoxCollider>();
            boxCollider.size = Vector3.one * slotSize;
            boxCollider.isTrigger = true;
            if(!slotIDOutlinesDict.ContainsKey(slotId)) {
                slotIDOutlinesDict.Add(slotId, UIHandler.AddOutlineToObject(slotParent.gameObject, Color.white));
            }
        }
        private GameObject CreateItemModel(ItemStack itemStack, Transform slotParent) {
            GameObject renderedItem = Instantiate(itemStack.Item.modelPrefab, slotParent);
            if (renderedItem.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            renderedItem.SetActive(false);
            return renderedItem;
        }
    }
}