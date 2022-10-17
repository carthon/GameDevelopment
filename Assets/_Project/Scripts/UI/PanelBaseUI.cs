using UnityEngine;

public abstract class PanelBaseUI : MonoBehaviour {
    public static bool wasDropped;
    protected void SubscribeEvents(ItemSlotUI itemSlotUI) {
        itemSlotUI.OnItemClicked += HandleItemSelection;
        itemSlotUI.OnItemBeginDrag += HandleBeginDrag;
        itemSlotUI.OnItemDroppedOn += HandleDrop;
        itemSlotUI.OnItemEndDrag += HandleEndDrag;
        itemSlotUI.OnItemRbClick += HandleRbClick;
    }

    private void UnSubscribeEvents(ItemSlotUI itemSlotUI) {
        itemSlotUI.OnItemClicked -= HandleItemSelection;
        itemSlotUI.OnItemBeginDrag -= HandleBeginDrag;
        itemSlotUI.OnItemDroppedOn -= HandleDrop;
        itemSlotUI.OnItemRbClick -= HandleRbClick;
    }

    protected virtual void HandleItemSelection(ItemSlotUI itemSlot) {
        var mouseFollower = UIHandler.Instance.dragItemHandlerUI;
        if (mouseFollower.IsActive()) {
            mouseFollower.Toggle(false);
            mouseFollower.ResetData();
        }
    }
    protected virtual void HandleDrop(ItemSlotUI itemSlot) {
        var dragItemHandlerUI = UIHandler.Instance.dragItemHandlerUI;
        HandleItemSwap(itemSlot.ItemStack, dragItemHandlerUI.itemDragging.ItemStack);
        wasDropped = true;
    }
    private void HandleItemSwap(ItemStack itemSlot1, ItemStack itemSlot2) {
        var inventory1 = itemSlot1.GetInventory();
        var inventory2 = itemSlot2.GetInventory();
        var itemStack1Copy = inventory1.GetItemStack(itemSlot1.GetSlotID());
        inventory1.SwapItemsInInventory(inventory2, itemSlot1.GetSlotID(), itemSlot2.GetSlotID());
        //var itemStack1Copy = inventory1.TakeStackFromSlot(itemSlot1.GetSlotID());
        //var itemStack2Copy = inventory2.TakeStackFromSlot(itemSlot2.GetSlotID());
        //inventory1.AddItemStackToSlot(itemStack2Copy, itemStack1Copy.GetSlotID());
        //inventory2.AddItemStackToSlot(itemStack1Copy, itemStack2Copy.GetSlotID());
        
        UIHandler.Instance.UpdateVisuals = true;
    }
    protected virtual void HandleEndDrag(ItemSlotUI itemSlot) {
        var dragItemHandlerUI = UIHandler.Instance.dragItemHandlerUI;
        dragItemHandlerUI.Toggle(false);
        Debug.Log("Handling end drag");
        if (!wasDropped) {
            itemSlot.OnRemoveButton();
            Debug.Log("Handling drop");
            UIHandler.Instance.UpdateVisuals = true;
        }
        wasDropped = false;
    }
    protected virtual void HandleRbClick(ItemSlotUI itemSlot) {
    }
    protected virtual void HandleBeginDrag(ItemSlotUI itemSlot) {
        var dragItemHandlerUI = UIHandler.Instance.dragItemHandlerUI;
        dragItemHandlerUI.SetData(itemSlot.ItemStack);
        dragItemHandlerUI.Toggle(true);
    }
}