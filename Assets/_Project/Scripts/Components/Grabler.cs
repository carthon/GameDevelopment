using System.Collections.Generic;
using _Project.Libraries.Marching_Cubes.Scripts;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class Grabler : MonoBehaviour {
        [SerializeField] private LayerMask itemMask;
        public InventoryManager LinkedInventoryManager { get; set; }
        public bool CanPickUp { get; set; }

        public ItemStack TryPickItems(Grabbable closestGrabbable) {
            if (!CanPickUp)
                return ItemStack.EMPTY;

            var itemStack = closestGrabbable.GetItemStack();
            if (itemStack.Item == null)
                return ItemStack.EMPTY;

            var leftOver = LinkedInventoryManager.AddItemStack(itemStack);
            if (leftOver.GetCount() > 0)
                return itemStack;
            else {
                Planet planet = closestGrabbable.GetPlanet();
                Chunk chunk = planet != null ? planet.FindChunkAtPosition(transform.position) : null;
                if (chunk != null)
                    chunk.RemoveEntity(closestGrabbable);
                Destroy(closestGrabbable.gameObject);
            }
            return itemStack;
        }

        public Grabbable GetPickableInRange(Ray rayOrigin, float pickUpRadius, float pickUpDistance) {
            RaycastHit hitInfo;
            Grabbable pickable = null;
            if (Physics.SphereCast(rayOrigin, pickUpRadius, out hitInfo, pickUpDistance, itemMask)) {
                Debug.DrawRay(rayOrigin.origin, rayOrigin.direction * pickUpDistance, Color.yellow, 3f);
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
}