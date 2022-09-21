using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : PanelBaseUI {
    public GameObject uiSlot;
    public Transform itemsParent;

    private Inventory _inventory;
    private ItemSlotUI[] slots;
    public bool IsConfigured { get; private set; }

    public void SetUpInventory(Inventory inventory) {
        _inventory = inventory;
        inventory.OnInventoryChange += UpdateUI;

        slots = GetComponentsInChildren<ItemSlotUI>();
        for (var i = 0; i < slots.Length; i++) {
            slots[i].SetItemStack(_inventory.GetItemStack(i));
            slots[i].ClearSlot();
            SubscribeEvents(slots[i]);
        }
        IsConfigured = true;
        transform.SetAsFirstSibling();
    }

    public void UpdateUI(int slot, ItemStack itemStack) {
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