using System;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.UI {
    public class MouseFollower : MonoBehaviour {
        [SerializeField] private Canvas canvas;
        [SerializeField] private UIInventoryItem uiItem;

        private void Awake() {
            canvas = transform.parent.GetComponent<Canvas>();
            uiItem = GetComponentInChildren<UIInventoryItem>();
        }

        public void SetData(Item item) {
            uiItem.SetData(item);
        }

        private void Update() {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform) canvas.transform,
                Mouse.current.position.ReadValue(), null, out var position);
            transform.position = canvas.transform.TransformPoint(position);
        }

        public void Toggle(bool val) {
            gameObject.SetActive(val);
        }
    }
}