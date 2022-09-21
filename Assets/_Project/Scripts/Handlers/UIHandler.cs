using System.Collections.Generic;
using _Project.Scripts.UI;
using UnityEngine;

public class UIHandler : MonoBehaviour {
    public static UIHandler Instance;
    public GameObject inventoryUI;
    public Transform inventorySpawner;
    public ItemPickerUI ItemPickerUI;
    [SerializeField] private List<InventoryUI> _inventories;
    public DragItemHandlerUI dragItemHandlerUI;
    public HotbarUI _hotbarUi;

    public bool UpdateVisuals { get; set; }

    public void Awake() {
        _inventories = new List<InventoryUI>();
        _hotbarUi = GetComponentInChildren<HotbarUI>();
        Instance = this;
    }

    public void Tick(float delta) {
        _hotbarUi.Tick(delta);
    }
    public void AddInventory(Inventory inventory) {
        var index = _inventories.FindIndex(inventoryUi => inventoryUi.IsConfigured);
        InventoryUI inventoryUi = null;
        if (index == -1 || _inventories.Count < 1) {
            inventoryUi = Instantiate(inventoryUI, inventorySpawner).GetComponent<InventoryUI>();
            _inventories.Add(inventoryUi);
            index = _inventories.Count - 1;
        }
        _inventories[index].SetUpInventory(inventory);
    }
}