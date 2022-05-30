using System;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts {
    [CreateAssetMenu(menuName = "Equipment Slot")]
    public class EquipmentSlot : ScriptableObject {
        [SerializeField] private Transform overrideTransform;
        [SerializeField] private BodyPart bodyPart;
        private EquipmentSlotHandler parent;
        public BodyPart GetBodyPart() => bodyPart;
        public void SetParent(EquipmentSlotHandler newParent) => parent = newParent;
        public EquipmentSlotHandler GetParent() => parent;
    }

    public enum BodyPart {
        LEFT_HAND = 0,
        RIGHT_HAND = 1,
        HEAD = 2,
    }
}