using System;
using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Handlers;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.DiegeticUI {
    public class ContainerRenderer : MonoBehaviour {
        public Inventory Inventory;
        
        public Grid inventoryGrid;
        public Transform inventorySpawn;
        public Dictionary<GameObject, Vector2Int> renderedItems = new Dictionary<GameObject, Vector2Int>();
        public Color okIndicator = Color.green;
        public Color errorIndicator = Color.red;
        public Color dropIndicator = Color.magenta;

        public bool visualizeGrid = false;
        public float radius = 5f;       // Radio en unidades del mundo.
        public Color gizmoColor = Color.green;

        public void AttachToInventory(Inventory inventory) {
            Inventory = inventory;
            Inventory.OnSlotChange += InventoryOnSlotChange;
        }
        public void DetachInventory(Inventory inventory) {
            if (Inventory is null)
                return;
            Inventory.OnSlotChange -= InventoryOnSlotChange;
            Inventory = null;
        }
        private void InventoryOnSlotChange(InventorySlot inventorySlot) {
            // Item is already being rendered
            Vector2Int modelInSlot;
            if (inventorySlot.Model is not null && renderedItems.TryGetValue(inventorySlot.Model, out modelInSlot)) {
                if (inventorySlot.IsOrigin && !Inventory.GetItemStackFromSlot(modelInSlot).IsEmpty()) {
                    renderedItems.Remove(inventorySlot.Model);
                    Destroy(inventorySlot.Model);
                    inventorySlot.Model = null;
                    RenderItem(inventorySlot);
                }
                else if (!inventorySlot.IsOrigin || Inventory.GetItemStackFromSlot(modelInSlot).IsEmpty()) {
                    renderedItems.Remove(inventorySlot.Model);
                    Destroy(inventorySlot.Model);
                    inventorySlot.Model = null;
                }
            }
            // First time item being rendered, we link the GameObject to the dictionary
            else if (inventorySlot.IsOrigin && inventorySlot.ItemStack is not null && !inventorySlot.ItemStack.IsEmpty()) {
                RenderItem(inventorySlot);
            }
        }
        private void RenderItem(InventorySlot inventorySlot) {
            GameObject renderedStack = InstantiateItemRender(inventorySlot);
            if (renderedItems.TryAdd(renderedStack, inventorySlot.ItemStack.OriginalSlot)) {
                inventorySlot.Model = renderedStack;
            } else {
                Debug.LogError("Error trying to add item to render Dict");
            }
        }
        private GameObject InstantiateItemRender(InventorySlot inventorySlot) {
            ItemStack itemStack = inventorySlot.ItemStack;
            Vector3 position = FindPositionFromSlot(itemStack.OriginalSlot);
            GameObject itemRendered = Instantiate(itemStack.Item.model, position, transform.rotation, inventorySpawn);
            Bounds itemBounds = ExtractGameObjectBoundsAndSetTrigger(itemRendered);
            if (inventorySlot.IsFlipped)
                itemRendered.transform.Rotate(0, 90, 0);
            else
                itemRendered.transform.Rotate(0, 180, 0);
            itemRendered.transform.position +=  inventorySpawn.up * (itemBounds.extents.y / 2);
            itemRendered.tag = global::Constants.TAG_UISLOT;
            Debug.Log($"Extents: {itemBounds.extents} && Scale: {itemRendered.transform.localScale}");
            //GameObjectUtils.SetLayerRecursively(itemRendered, LayerMask.NameToLayer("Inventory"), "UISlot");
            UIHandler.AddOutlineToObject(itemRendered, Color.white);
            return itemRendered;
        }
        private Bounds ExtractGameObjectBoundsAndSetTrigger(GameObject obj) {
            Collider[] colliders = obj.GetComponents<Collider>();
            if (colliders.Length > 0)
            {
                Bounds combinedBounds = colliders[0].bounds;
                for (int i = 1; i < colliders.Length; i++) {
                    combinedBounds.Encapsulate(colliders[i].bounds);
                }
                return combinedBounds;
            }
            return new Bounds();
        }
        private Vector3 FindPositionFromSlot(Vector2Int slotPosition) {
            Vector3Int centerCell = new Vector3Int(slotPosition.x, 0, -slotPosition.y);
            Vector3 cellCenterWorld = inventoryGrid.GetCellCenterWorld(centerCell);
            Vector3 localPos = inventoryGrid.transform.InverseTransformPoint(cellCenterWorld);
            localPos.y = 0;
            return inventoryGrid.transform.TransformPoint(localPos);
        }
        private void OnDrawGizmos()
        {
            if (inventoryGrid == null || !visualizeGrid)
                return;

            Gizmos.color = gizmoColor;

            // Tamaño de celda definido en la Grid
            Vector3 cellSize = inventoryGrid.cellSize;
        
            // Convertimos el punto central a coordenadas de celda para obtener la celda central.
            Vector3Int centerCell = inventoryGrid.WorldToCell(inventorySpawn.position);
        
            // Calcula cuántas celdas se requieren para cubrir el radio.
            // Usamos la mayor dimensión de la celda para asegurarnos de cubrir el área.
            int cellsRadius = Mathf.CeilToInt(radius / Mathf.Max(cellSize.x, cellSize.z));

            // Iteramos sobre un rango de celdas que podría cubrir el radio.
            for (int x = -cellsRadius; x <= cellsRadius; x++)
            {
                for (int z = -cellsRadius; z <= cellsRadius; z++)
                {
                    Vector3Int cellPos = centerCell + new Vector3Int(x, 0, z);
                    // Obtenemos el centro de la celda en el mundo.
                    Vector3 cellWorldCenter = inventoryGrid.GetCellCenterWorld(cellPos);
                    // Comprobamos si el centro de la celda está dentro del radio.
                    if (Vector3.Distance(cellWorldCenter, inventorySpawn.position) <= radius)
                    {
                        // Dibujamos la celda (por ejemplo, un cubo con el tamaño de la celda).
                        Gizmos.DrawWireCube(cellWorldCenter, cellSize);
                    }
                }
            }
        }
    }
}