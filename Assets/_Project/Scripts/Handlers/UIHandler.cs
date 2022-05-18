using System;
using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.UI;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneTemplate;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class UIHandler : MonoBehaviour {
        public static UIHandler instance;

        [SerializeField] private Transform layoutUI;

        public bool isDisplaying = false;

        private List<UIInventoryPanel> modalInventories;
        [SerializeField] public MouseFollower mouseFollower;
        public GameObject modalInventoryPrefab;
        public GameObject hotbarUIPrefab;
        public Vector2 mousePosition;
        // Start is called before the first frame update
        private void Awake() {
            instance = this;
            modalInventories = new List<UIInventoryPanel>();
            if (mouseFollower == null) mouseFollower = GetComponentInChildren<MouseFollower>();
            mouseFollower.Toggle(false);
        }

        public void AddInventory(Inventory inventory) {
            GameObject inventoryUI = Instantiate(modalInventoryPrefab, layoutUI.transform);
            UIInventoryPanel inventoryPanel = inventoryUI.GetComponent<UIInventoryPanel>();
            inventoryPanel.SetInventoryPanel(inventory);
            inventoryPanel.CreateInventoryUI();
            if (!modalInventories.Find(inv => inv.gameObject.name == name))
                modalInventories.Add(inventoryPanel);
            else
                Debug.LogWarning("An inventory with that name already exists");
        }

        public void LoadHotbarUI(HotbarHandler hotbarHandler) {
            UIHotbarPanel uiHotbarPanel = GetComponentInChildren<UIHotbarPanel>();
            uiHotbarPanel.SetHotbarHandler(hotbarHandler);
        }


        public void DisplayAllInventories(bool display) {
            foreach (UIInventoryPanel inventory in modalInventories) {
                inventory.Display(display);
            }
        }
    }
}
