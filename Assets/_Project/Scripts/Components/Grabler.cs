using System.Collections.Generic;
using _Project.Libraries.Marching_Cubes.Scripts;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Utils;
using UnityEngine;

namespace _Project.Scripts.Components {
    public class Grabler : MonoBehaviour {
        [SerializeField] private LayerMask itemMask;
        public InventoryManager LinkedInventoryManager { get; set; }
        private Collider _lastCollider;
        private Grabbable _lastGrabbable;
        public bool CanPickUp { get; set; }

        public ItemStack TryPickItems(Grabbable closestGrabbable) {
            if (!CanPickUp)
                return ItemStack.EMPTY;

            var itemStack = closestGrabbable.GetItemStack();
            if (itemStack.Item == null)
                return ItemStack.EMPTY;

            var leftOver = LinkedInventoryManager.AddItemStack(itemStack);
            return leftOver;
        }

        public Grabbable GetPickableInRange(Ray rayOrigin, float pickUpRadius, float pickUpDistance) {
            if (Physics.SphereCast(rayOrigin, pickUpRadius, out RaycastHit hitInfo, pickUpDistance, itemMask)) {
                if (hitInfo.collider == _lastCollider && _lastGrabbable != null)
                    return _lastGrabbable;
                Debug.DrawRay(rayOrigin.origin, rayOrigin.direction * pickUpDistance, Color.yellow, 3f);
                GrabbableProxy.TryGet(hitInfo.collider, out Grabbable pickable);
                _lastGrabbable = pickable;
                _lastCollider = hitInfo.collider;
                return pickable;
            }
            _lastCollider = null;
            _lastGrabbable = null;
            return null;
        }
    }
}