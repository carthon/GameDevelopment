using UnityEngine;

namespace _Project.Scripts.Components {
    public class Container : MonoBehaviour {
        private Inventory _inventory;
        public int MaxContainerSlots() => _inventory.Width * _inventory.Height;
        public bool IsStatic { get; private set; }
        public Inventory Inventory { get =>_inventory; }
    }
}