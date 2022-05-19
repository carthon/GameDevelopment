using System;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.Handlers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.UI {
    public class MouseFollower : MonoBehaviour {
        [SerializeField] private Canvas canvas;
        [SerializeField] private UIItemSlot uiItemSlot;
        public bool Active { get; private set; }

        private void Awake() {
            canvas = transform.parent.GetComponent<Canvas>();
            uiItemSlot = GetComponentInChildren<UIItemSlot>();
        }

        public void SetData(ItemStack item) {
            uiItemSlot.SetData(item);
        }

        private void Update() {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform) canvas.transform,
                Mouse.current.position.ReadValue(), null, out var position);
            transform.position = canvas.transform.TransformPoint(position);
        }

        public void Toggle(bool val) {
            gameObject.SetActive(val);
            Active = val;
        }
    }
}