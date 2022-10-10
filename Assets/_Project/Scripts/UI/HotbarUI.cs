using System.Collections.Generic;
using System.Linq;

namespace _Project.Scripts.UI {
    public class HotbarUI : PanelBaseUI {
        private int _activeSlot = 9;
        private List<HotbarSlotUI> _hotbarSlotUis;
        private Dictionary<Item, int> itemsCount;

        public int ActiveSlot
        {
            get => _activeSlot;
            set
            {
                if (value >= 0 && value <= 9) {
                    _hotbarSlotUis[ActiveSlot].SetBorder(false);
                    _activeSlot = value;
                    _hotbarSlotUis[value].SetBorder(true);
                }
            }
        }
        private void Start() {
            _hotbarSlotUis = GetComponentsInChildren<HotbarSlotUI>().ToList();
            for (var i = 0; i < _hotbarSlotUis.Count; i++) {
                _hotbarSlotUis[i].ClearLink();
                _hotbarSlotUis[i].SetBorder(false);
                SubscribeEvents(_hotbarSlotUis[i]);
            }
        }
        public void Tick(float delta) {
        }
        protected override void HandleRbClick(ItemSlotUI itemSlot) {
            var hotbarSlotUI = (HotbarSlotUI) itemSlot;
            if (hotbarSlotUI.IsListening) {
                hotbarSlotUI.ClearLink();
                hotbarSlotUI.ItemStack.GetInventory().OnSlotChange -= hotbarSlotUI.UpdateItemCount;
                hotbarSlotUI.IsListening = false;
            }
        }
        protected override void HandleDrop(ItemSlotUI itemSlot) {
            var hotbarSlotUI = (HotbarSlotUI) itemSlot;
            var itemStack = UIHandler.Instance.dragItemHandlerUI.itemDragging.ItemStack;
            if (!hotbarSlotUI.IsListening) {
                hotbarSlotUI.SetItemStack(itemStack);
                hotbarSlotUI.LinkedItems.SetItemLinks(itemStack.GetInventory().GetItemStacksByType(hotbarSlotUI.ItemStack.Item), itemSlot.ItemStack.Item);
                hotbarSlotUI.ItemStack.GetInventory().OnSlotChange += hotbarSlotUI.UpdateItemCount;
                hotbarSlotUI.SetItemStackCount(hotbarSlotUI.LinkedItems.GetItemsCount());
                hotbarSlotUI.IsListening = true;
                wasDropped = true;
            }
        }

        public List<HotbarSlotUI> GetHotbarSlots() {
            return _hotbarSlotUis;
        }
        public ItemLinks GetItemLinkInSlot(int slot) {
            return _hotbarSlotUis[slot].LinkedItems;
        }
    }
}