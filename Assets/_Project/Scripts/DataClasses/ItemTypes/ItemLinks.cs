using System.Collections.Generic;
using System.Linq;

public class ItemLinks {
    public static readonly ItemLinks BLANK = new ItemLinks();

    public ItemLinks() {
        LinkedStacks = new List<ItemStack>();
        ItemTypes = new List<Item>();
    }

    public List<Item> ItemTypes { get; }

    public List<ItemStack> LinkedStacks { get; private set; }

    public void AddItemToLinks(ItemStack itemStack) {
        var deletedLinks = LinkedStacks.RemoveAll(itemStack => itemStack.Item == null || itemStack.IsEmpty());
        var updateItemStack = LinkedStacks.FindIndex(itemStack => itemStack.Item != null && !itemStack.IsFull());
        if (deletedLinks == 0)
            if (updateItemStack == -1)
                LinkedStacks.Add(itemStack);
    }
    public void SetItemLinks(List<ItemStack> itemLinks, Item type) {
        LinkedStacks = itemLinks;
        ItemTypes.Add(type);
    }
    public int GetItemsCount() {
        return LinkedStacks.Sum(stacks => stacks.GetCount());
    }
    public bool IsEmpty() {
        return LinkedStacks.Count <= 0;
    }
}