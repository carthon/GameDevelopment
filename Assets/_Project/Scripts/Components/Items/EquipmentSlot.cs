using System;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts {
    [CreateAssetMenu(menuName = "Equipment Slot")]
    public class EquipmentSlot : ScriptableObject {
        private Inventory inventory;
        [SerializeField] private Transform overrideTransform;
        [SerializeField] private BodyPart bodyPart;

        private void Awake() {
            inventory = new Inventory(1);
        }

        public Inventory GetInventory() => inventory;
        
        public EquipmentSlot(Transform overrideTransform, EquipmentSlotHandler equipmentSlot) {
            this.overrideTransform = overrideTransform;
            this.bodyPart = BodyPart.RIGHT_HAND;
        }
        
        public EquipmentSlot(Transform overrideTransform, EquipmentSlotHandler equipmentSlot, BodyPart bodyPart) {
            this.overrideTransform = overrideTransform;
            this.bodyPart = bodyPart;
        }
        public BodyPart GetBodyPart() => bodyPart;
    }

    public enum BodyPart {
        LEFT_HAND = 0,
        RIGHT_HAND = 1,
        HEAD = 2,
    }
}