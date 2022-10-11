using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : PanelBaseUI {
    public GameObject uiSlot;
    public Transform itemsParent;

    private Inventory _inventory;
    private int _inventoryId;
    private ItemSlotUI[] slots;
    public InventoryManager _inventoryManager;
    public bool IsConfigured { get; private set; }

    public void SetUpInventory(Inventory inventory, InventoryManager inventoryManager) {
        _inventoryId = inventory.Id;
        _inventory = inventory;
        _inventoryManager = inventoryManager;
        _inventory.OnSlotChange += UpdateSlot;
        _inventory.OnSlotSwap += UIHandler.Instance.SwapSlots;

        slots = GetComponentsInChildren<ItemSlotUI>();
        for (var i = 0; i < slots.Length; i++) {
            slots[i].SetItemStack(inventory.GetItemStack(i));
            slots[i].ClearSlot();
            SubscribeEvents(slots[i]);
        }
        IsConfigured = true;
        transform.SetAsFirstSibling();
    }

    public void UpdateSlot(int slot, ItemStack itemStack) {
        var item = _inventory.GetItemStack(slot);
        if (!item.IsEmpty())
            slots[slot].SetItemStack(item);
        else
            slots[slot].ClearSlot();
    }
    public IEnumerable<ItemSlotUI> GetUiSlots() {
        return slots;
    }
}