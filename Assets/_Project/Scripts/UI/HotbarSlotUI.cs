public class HotbarSlotUI : ItemSlotUI {

    public bool IsListening { get; set; } = false;
    public ItemLinks LinkedItems { get; private set; }

    public void AssignGroup(ItemLinks itemLinks) {
        LinkedItems = itemLinks;
        icon.sprite = LinkedItems.LinkedStacks[0].Item.itemIcon;
        icon.enabled = true;
    }

    public void ClearLink() {
        icon.enabled = false;
        LinkedItems = new ItemLinks();
        icon.sprite = null;
        _amount.gameObject.SetActive(false);
    }

    public void UpdateItemCount(int slot, ItemStack itemStack) {
        var isItemLinked = LinkedItems.ItemTypes.Contains(itemStack.Item);
        if (isItemLinked) {
            LinkedItems.AddItemToLinks(itemStack);
            SetItemStackCount(LinkedItems.GetItemsCount());
        }
    }
    public void SetBorder(bool value) {
        borderImage.enabled = value;
    }
}