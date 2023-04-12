using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Handlers;
using QuickOutline.Scripts;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.DiegeticUI {
    public class ContainerRenderer : MonoBehaviour {
        [SerializeField]
        public float slotSize = 0.1f;
        public InventoryManager Inventory;
        private Transform _parent;
        private List<Dictionary<int, List<GameObject>>> slotIDItemsDict;
        private List<Dictionary<int, Outline>> slotIDOutlinesDict;
        private List<Dictionary<int, Bounds>> slotIDItemBounds;
        private static ContainerRenderer _singleton;
        private Vector3 originalParentPosition;
        private Vector3 inventoryDownRaycastDirection = Vector3.down;
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
        
        public void InitializeRenderer(InventoryManager inventoryManager, Transform parent) {
            Inventory = inventoryManager;
            slotIDItemsDict = new List<Dictionary<int, List<GameObject>>>();
            slotIDOutlinesDict = new List<Dictionary<int, Outline>>();
            slotIDItemBounds = new List<Dictionary<int, Bounds>>();
            foreach (Inventory inventory in Inventory.Inventories) {
                _parent = new GameObject(inventory.Id.ToString()).transform;
                _parent.SetParent(parent);
                _parent.localPosition = Vector3.zero;
                slotIDItemBounds.Add(new Dictionary<int, Bounds>());
                slotIDItemsDict.Add(new Dictionary<int, List<GameObject>>());
                slotIDOutlinesDict.Add(new Dictionary<int, Outline>());
                inventory.OnSlotChange += UpdateInventorySlot;
                inventory.OnSlotSwap += OnSlotSwap;
                float slotsPerRow = (float) Math.Sqrt(inventory.Size);
                toggled = false;
                needsUpdate = true;
                List<ItemStack> items = inventory.GetInventorySlots();
                for (int i = 0; i < inventory.Size; i++)
                    CreateSlot(inventory.Id, i, slotsPerRow);
                foreach (ItemStack item in items) {
                    if (!item.IsEmpty()) {
                        slotIDItemBounds[inventory.Id].Add(item.GetSlotID(), GetItemBounds(item));
                        slotIDItemsDict[inventory.Id].Add(item.GetSlotID(), RenderItemStack(item));
                    }
                }
            }
            _parent = parent;
            originalParentPosition = _parent.localPosition;
        }
        private void OnSlotSwap(int inventoryId, int otherInventoryId, int slot, int otherSlot) {
            if (slotIDItemsDict[inventoryId].TryGetValue(slot, out List<GameObject> originObjsInSlot) && originObjsInSlot.Count > 0 &&
                slotIDItemsDict[otherInventoryId].TryGetValue(otherSlot, out List<GameObject> destObjsInSlot) && destObjsInSlot.Count > 0) {
                Logger.Singleton.Log($"Swapped {slot} for {otherSlot}", Logger.Type.DEBUG);
                Transform originParent = slotIDOutlinesDict[inventoryId][slot].transform;
                Transform destParent = slotIDOutlinesDict[otherInventoryId][otherSlot].transform;
                Vector3 otherItemBounds = slotIDItemBounds[inventoryId][slot].extents * 2;
                for (int i= 0; i < originObjsInSlot.Count; i++) {
                    originObjsInSlot[i].transform.SetParent(destParent);
                    originObjsInSlot[i] = SetRenderPositionInSlot(i, originObjsInSlot[i], otherItemBounds);
                }
                itemsPerRow = 0;
                itemPileHeight = 0;
                    Vector3 itemBounds = slotIDItemBounds[otherInventoryId][otherSlot].extents * 2;
                    for (int i= 0; i < destObjsInSlot.Count; i++) {
                        destObjsInSlot[i].transform.SetParent(originParent);
                        destObjsInSlot[i] = SetRenderPositionInSlot(i, destObjsInSlot[i], itemBounds);
                    }
                    itemsPerRow = 0;
                    itemPileHeight = 0;
                    (slotIDItemBounds[inventoryId][slot], slotIDItemBounds[otherInventoryId][otherSlot]) = (slotIDItemBounds[otherInventoryId][otherSlot], slotIDItemBounds[inventoryId][slot]);
                    (slotIDItemsDict[inventoryId][slot], slotIDItemsDict[otherInventoryId][otherSlot]) = (slotIDItemsDict[otherInventoryId][otherSlot], slotIDItemsDict[inventoryId][slot]);
            } else {
                UpdateInventorySlot(otherSlot, otherInventoryId);
                UpdateInventorySlot(slot, inventoryId);
            }
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
            int inventoryId = itemStack.GetInventory().Id;

            //float itemsPerSlotRow = itemStack.Item.IsStackable() ? (float)Math.Sqrt(itemStack.Item.GetMaxStackSize()) : 1;
            Vector3 itemExtents = slotIDItemBounds[inventoryId][slotId].extents * 2;
            float itemSize = slotSize;
            int startListSize = 0;

            if (slotIDOutlinesDict[inventoryId].TryGetValue(slotId, out Outline outline) && slotIDItemsDict[inventoryId].ContainsKey(slotId)) {
                List<GameObject> objList = outline.transform.GetComponentsInChildren<Rigidbody>().Select(obj=> obj.gameObject).ToList();
                if(objList.Count > 0 && objList.Count <= itemStack.GetCount()) {
                    startListSize = objList.Count;
                    listOfRenders = objList;
                }
            }
            for (int i = startListSize; i < itemStack.GetCount(); i++) {
                GameObject renderedItem = CreateItemModel(itemStack, slotIDOutlinesDict[inventoryId][slotId].transform);
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
        private void UpdateInventorySlot(int slotId, ItemStack itemStack) => UpdateInventorySlot(slotId, itemStack.GetInventory().Id);
        public void UpdateInventorySlot(int slotId, int inventoryId) {
            ItemStack stackInSlot = Inventory.Inventories[inventoryId].GetItemStack(slotId);
            if (slotIDItemsDict[inventoryId].TryGetValue(slotId, out List<GameObject> objectsList)) {
                //Actualizar el slot dependiendo de los items que haya
                if (objectsList?.Count > 0) {
                    //Si no hay items en el stack, se destruyen los objetos que hubiera antes
                    if (stackInSlot.IsEmpty()) {
                        DestroyGameObjectsInChildren(slotIDOutlinesDict[inventoryId][slotId].transform);
                        slotIDItemsDict[inventoryId].Remove(slotId);
                        slotIDItemBounds[inventoryId].Remove(slotId);
                        slotIDOutlinesDict[inventoryId][slotId].ResetRenderers();
                    } else if (!objectsList[0].name.Equals(stackInSlot.Item.modelPrefab.name)) {
                        DestroyGameObjectsInChildren(slotIDOutlinesDict[inventoryId][slotId].transform);
                        slotIDItemBounds[inventoryId][slotId] = GetItemBounds(stackInSlot);
                        slotIDItemsDict[inventoryId][slotId] = RenderItemStack(stackInSlot);
                    }
                }
            }
            //Si los objetos que hay no son iguales al prefab entonces hay que instanciar unos nuevos
            else {
                slotIDItemBounds[inventoryId].Add(stackInSlot.GetSlotID(), GetItemBounds(stackInSlot));
                slotIDItemsDict[inventoryId].Add(stackInSlot.GetSlotID(), RenderItemStack(stackInSlot));
            } 
            if(!stackInSlot.IsEmpty()) slotIDOutlinesDict[inventoryId][slotId].ReloadRenderers();
            needsUpdate = true;
        }
        private void DestroyGameObjectsInChildren(Transform parent) {
            for (int i = 0; i < parent.childCount; i++) {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
        public void ToggleRender(bool render, int inventoryId = 0) {
            if (toggled == render)
                return;

            if (!render) _parent.localPosition = originalParentPosition;
            else if (Physics.Raycast(_parent.position + Vector3.up * 5f, inventoryDownRaycastDirection, out RaycastHit hit, 6f, 1 << LayerMask.NameToLayer("Ground"))){
                _parent.transform.position = hit.point;
                Quaternion rotatedParent = Quaternion.FromToRotation(Vector3.up, hit.normal);
                _parent.transform.rotation = rotatedParent;
            }
            foreach (KeyValuePair<int, List<GameObject>> keyValuePair in slotIDItemsDict[inventoryId]) {
                keyValuePair.Value.ForEach(obj => obj.SetActive(render));
                if (render && needsUpdate) {
                    slotIDOutlinesDict[inventoryId][keyValuePair.Key].ReloadRenderers();
                }
            }
            needsUpdate = false;
            toggled = render;
        }
        private void CreateSlot(int inventoryId, int slotId, float slotsPerRow) {
            GameObject slotObj = new GameObject(slotId.ToString());
            Transform slotParent = slotObj.transform;
            Vector3 centerOfSlot = CellPositionInGrid(slotId, (int) slotsPerRow, Vector3.zero, slotSize);
            slotParent.SetParent(_parent);
            slotParent.localPosition = centerOfSlot + Vector3.up * slotSize / 2;
            slotParent.rotation = _parent.rotation;
            slotParent.tag = "UISlot";
            BoxCollider boxCollider = slotParent.gameObject.AddComponent<BoxCollider>();
            boxCollider.size = Vector3.one * slotSize;
            boxCollider.isTrigger = true;
            if(!slotIDOutlinesDict[inventoryId].ContainsKey(slotId)) {
                slotIDOutlinesDict[inventoryId].Add(slotId, UIHandler.AddOutlineToObject(slotParent.gameObject, Color.white));
            }
        }
        private GameObject CreateItemModel(ItemStack itemStack, Transform slotParent) {
            GameObject renderedItem = Instantiate(itemStack.Item.modelPrefab, slotParent);
            if (renderedItem.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            renderedItem.SetActive(toggled);
            return renderedItem;
        }
    }
}