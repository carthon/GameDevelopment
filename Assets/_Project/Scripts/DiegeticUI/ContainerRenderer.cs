using System;
using System.Collections.Generic;
using _Project.Libraries.QuickOutline.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Handlers;
using UnityEngine;
using Logger = _Project.Scripts.Utils.Logger;

namespace _Project.Scripts.DiegeticUI {
    public class ContainerRenderer : MonoBehaviour {
        [SerializeField]
        public float slotSize = 0.1f;
        public InventoryManager inventory;
        private Transform _parent;
        private List<Dictionary<int, List<(Renderer, MeshFilter)>>> slotIDItemsRendererDict;
        private List<Dictionary<int, Outline>> slotIDOutlinesDict;
        private List<Dictionary<int, Bounds>> slotIDItemBounds;
        private static ContainerRenderer _singleton;
        private Vector3 originalParentPosition;
        private Vector3 inventoryDownRaycastDirection = Vector3.down;
        private bool toggled;
        
        private int itemsPerRow = 0;
        private int itemsPerColumn = 0;
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
            inventory = inventoryManager;
            slotIDItemsRendererDict = new List<Dictionary<int, List<(Renderer, MeshFilter)>>>();
            slotIDOutlinesDict = new List<Dictionary<int, Outline>>();
            slotIDItemBounds = new List<Dictionary<int, Bounds>>();
            foreach (Inventory inv in inventory.Inventories) {
                _parent = new GameObject(inv.Id.ToString()).transform;
                _parent.SetParent(parent);
                _parent.localPosition = Vector3.zero;
                slotIDItemBounds.Add(new Dictionary<int, Bounds>());
                slotIDItemsRendererDict.Add(new Dictionary<int, List<(Renderer, MeshFilter)>>());
                slotIDOutlinesDict.Add(new Dictionary<int, Outline>());
                inv.OnSlotChange += UpdateInventorySlot;
                inv.OnSlotSwap += OnSlotSwap;
                float slotsPerRow = (float) Math.Sqrt(inv.Size);
                toggled = false;
                for (int i = 0; i < inv.Size; i++) {
                    Transform slot = CreateSlot(inv.Id, i, slotsPerRow);
                    List<(Renderer, MeshFilter)> list = new List<(Renderer, MeshFilter)>();
                    for (int j = 0; j < Inventory.MaxStackSize; j++) {
                        (Renderer, MeshFilter) tuple = CreateItemModel(j.ToString(), slot);
                        list.Add(tuple);
                    }
                    slotIDItemsRendererDict[inv.Id].Add(i, list);
                    slotIDItemBounds[inv.Id].Add(i, new Bounds());
                    slotIDOutlinesDict[inv.Id][i].ReloadRenderers();
                }
            }
            _parent = parent;
            originalParentPosition = _parent.localPosition;
        }
        private void OnSlotSwap(int inventoryId, int otherInventoryId, int slot, int otherSlot) {
            UpdateInventorySlot(otherSlot, otherInventoryId);
            UpdateInventorySlot(slot, inventoryId);
        }
        private Bounds GetItemBounds(ItemStack itemStack) {
            Bounds bounds = new Bounds();
            if (itemStack.Item.modelPrefab.TryGetComponent(out Renderer itemRenderer)) {
                bounds = itemRenderer.bounds;
            }
            else {
                itemRenderer = itemStack.Item.modelPrefab.GetComponentInChildren<Renderer>();
                if (!(itemRenderer is null))
                    bounds = itemRenderer.bounds;
            }
            return bounds;
        }
        private void UpdateRender(ItemStack itemStack, bool render = true) {
            int slot = itemStack.GetSlotID();
            int inventoryId = itemStack.GetInventory().Id;
            if (itemStack.IsEmpty()) {
                slotIDItemBounds[inventoryId][slot] = new Bounds();
                foreach ((Renderer, MeshFilter) obj in slotIDItemsRendererDict[inventoryId][slot]) {
                    obj.Item2.sharedMesh = null;
                    obj.Item2.gameObject.SetActive(false);
                }
                return;
            }
            Renderer itemRenderer = itemStack.Item.modelPrefab.GetComponentInChildren<Renderer>();
            MeshFilter mesh = itemStack.Item.modelPrefab.GetComponentInChildren<MeshFilter>();
            Transform meshTransform = mesh.transform;
            Quaternion childRotation = meshTransform.localRotation;
            Vector3 childScale = meshTransform.localScale;
            float itemSize = slotSize;
            Bounds itemBounds = GetItemBounds(itemStack);
            slotIDItemBounds[inventoryId][slot] = itemBounds;
            Vector3 itemExtents = itemBounds.extents * slotSize;
            Logger.Singleton.Log($"Update mesh:{mesh} render: {itemRenderer}, Bounds:{itemBounds.extents}", Logger.Type.DEBUG);
            
            for (int i = 0; i < itemStack.GetCount(); i++) {
                slotIDItemsRendererDict[inventoryId][slot][i].Item2.sharedMesh = mesh.sharedMesh;
                slotIDItemsRendererDict[inventoryId][slot][i].Item1.sharedMaterial = itemRenderer.sharedMaterial;
                slotIDItemsRendererDict[inventoryId][slot][i].Item1.gameObject.SetActive(render);
                Transform child = slotIDItemsRendererDict[inventoryId][slot][i].Item1.transform;
                child.localRotation = childRotation;
                child.localScale = childScale * itemSize;
                SetRenderPositionInSlot(i, child, itemExtents);
            }
            itemsPerColumn = 0;
            itemsPerRow = 0;
            itemPileHeight = 0;
        }
        private void SetRenderPositionInSlot(int slot, Transform renderedItem, Vector3 itemExtents) {
            Bounds cellBounds = new Bounds(Vector3.zero, Vector3.one * slotSize / 2);
            Bounds itemBounds = new Bounds(Vector3.zero, itemExtents * 2);
            renderedItem.localPosition = Vector3.zero;
            renderedItem.position += ModelPositionFromBounds(slot, Vector3.zero, itemBounds, cellBounds);
        }
        private Vector3 ModelPositionFromBounds(int cellIndex, Vector3 centerOfGrid, Bounds modelBounds, Bounds cellBounds) {
            Vector3 leftBottomCellCorner = centerOfGrid - cellBounds.extents;
            Vector3 leftBottomItemCenter = leftBottomCellCorner + modelBounds.extents;
            Vector3 modelExtents = modelBounds.extents * 2;
            modelBounds.center = leftBottomItemCenter + new Vector3(modelExtents.x, 0 , 0) * cellIndex;
            if (modelBounds.Intersects(cellBounds)) {
                return modelBounds.center;
            }
            if (itemsPerRow == 0) {
                itemsPerRow = cellIndex;
            }
            modelBounds.center = leftBottomItemCenter + new Vector3(modelExtents.x, 0 , 0) * (cellIndex % itemsPerRow)
                + new Vector3(0, 0, modelExtents.z) * (cellIndex / itemsPerRow % itemsPerRow);
            if (!modelBounds.Intersects(cellBounds) && itemsPerColumn == 0) itemsPerColumn = cellIndex - 1;
            if (itemsPerColumn != 0)
                modelBounds.center = leftBottomItemCenter + new Vector3(modelExtents.x, 0 , 0) * (cellIndex % itemsPerRow)
                    + new Vector3(0, 0, modelExtents.z) * (cellIndex / itemsPerColumn % itemsPerColumn);
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
            ItemStack stackInSlot = inventory.Inventories[inventoryId].GetItemStack(slotId);
            UpdateRender(stackInSlot, toggled);
        }
        public void ToggleRender(bool render, int inventoryId = 0) {
            if (toggled == render)
                return;

            if (!render) _parent.localPosition = originalParentPosition;
            else if (Physics.Raycast(_parent.position + Vector3.up * 5f, inventoryDownRaycastDirection, out RaycastHit hit, 6f, 1 << LayerMask.NameToLayer("Ground"))) {
                _parent.transform.position = hit.point;
            }
            for (int i = 0; i < inventory.Inventories[inventoryId].Size; i++) {
                for (int j = 0; j < inventory.Inventories[inventoryId].GetItemStack(i).GetCount(); j++) {
                    slotIDItemsRendererDict[inventoryId][i][j].Item1.gameObject.SetActive(render);
                }
            }
            toggled = render;
        }
        private Transform CreateSlot(int inventoryId, int slotId, float slotsPerRow) {
            GameObject slotObj = new GameObject(slotId.ToString());
            Transform slotParent = slotObj.transform;
            Vector3 centerOfSlot = CellPositionInGrid(slotId, (int) slotsPerRow, Vector3.zero, slotSize);
            slotParent.SetParent(_parent);
            slotParent.localPosition = centerOfSlot + Vector3.up * slotSize / 2;
            slotParent.rotation = _parent.rotation;
            slotParent.tag = "UISlot";
            BoxCollider boxCollider = slotParent.gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(1, 0.2f, 1) * slotSize;
            boxCollider.isTrigger = true;
            if(!slotIDOutlinesDict[inventoryId].ContainsKey(slotId)) {
                slotIDOutlinesDict[inventoryId].Add(slotId, UIHandler.AddOutlineToObject(slotParent.gameObject, Color.white));
            }
            return slotParent;
        }
        private (Renderer,MeshFilter) CreateItemModel(string itemName, Transform slotParent) {
            GameObject renderedItem = new GameObject(itemName);
            renderedItem.transform.SetParent(slotParent);
            renderedItem.SetActive(toggled);
            Renderer render = renderedItem.AddComponent<MeshRenderer>();
            MeshFilter mesh = renderedItem.AddComponent<MeshFilter>();
            return (render, mesh);
        }
    }
}