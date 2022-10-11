using System.Collections.Generic;
using _Project.Scripts.Components;
using UnityEngine;

public class Grabler : MonoBehaviour {
    [SerializeField] private LayerMask itemMask;
    public InventoryManager LinkedInventoryManager { get; set; }
    public bool CanPickUp { get; set; }

    public LootTable TryPickItems(Grabbable closestGrabbable) {
        if (!CanPickUp)
            return new LootTable();

        var itemTable = closestGrabbable.GetLootTable();
        if (itemTable == null)
            return new LootTable();

        var leftOvers = new List<ItemStack>();
        foreach (var itemInLootTable in itemTable.LootTables) {
            var leftOver = LinkedInventoryManager.AddItemStack(new ItemStack(itemInLootTable.Item, itemInLootTable.Count));
            leftOvers.Add(leftOver);
            itemInLootTable.Count = leftOver.GetCount();
        }
        if (itemTable.IsEmpty()) Destroy(closestGrabbable.gameObject);
        itemTable = new LootTable();
        foreach (var itemStack in leftOvers) itemTable.AddToLootTable(itemStack);
        return itemTable;
    }

    public Grabbable GetPickableInRange(Ray rayOrigin, float pickUpDistance) {
        RaycastHit hitInfo;
        Grabbable pickable = null;
        if (Physics.Raycast(rayOrigin, out hitInfo, pickUpDistance, itemMask)) {
            Debug.DrawRay(rayOrigin.origin, rayOrigin.direction * pickUpDistance, Color.yellow, 30f);
            pickable = hitInfo.collider.GetComponent<Grabbable>();
        }
        return pickable;
    }

    // public Grabbable GetClosestItem() {
    //     var results = new Collider[10];
    //     var size = Physics.OverlapBoxNonAlloc(itemPickCenter.position, new Vector3(areaWidth, areaHeight, areaWidth), results, Quaternion.identity, itemMask);
    //     Grabbable closestItem = null;
    //     if (size > 0) {
    //         var minDistance = float.MaxValue;
    //         for (var i = 0; i < size && results[i] != null; i++) {
    //             var pickable = results[i].GetComponent<Grabbable>();
    //             if (results[i].gameObject != null && pickable != null) {
    //                 var distance = Vector3.Distance(results[i].transform.position, transform.position);
    //                 if (distance < minDistance) {
    //                     minDistance = distance;
    //                     closestItem = pickable;
    //                 }
    //             }
    //         }
    //     }
    //     return closestItem;
    // }
}