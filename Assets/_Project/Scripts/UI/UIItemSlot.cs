using System;
using _Project.Scripts.Components.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Scripts.UI {
    public class UIItemSlot : MonoBehaviour {
        protected ItemStack itemStack;
        [SerializeField] protected Image itemImage;
        [SerializeField] protected TMP_Text quantityTxt;
        [SerializeField] protected Image borderImage;
        private UIInventoryPanel parent;
        public event Action<UIItemSlot> OnItemClicked, OnItemDroppedOn, OnItemBeginDrag, 
            OnItemEndDrag, OnRightMouseBtnClick, OnPullItemStack;
        public int SlotID { get; set; }

        private void Awake() {
            ResetData();
            Deselect();
        }

        public virtual void OnBeginDrag() {
            InvokeEvent(OnItemBeginDrag, this);
        }
        public virtual void OnEndDrag() {
            InvokeEvent(OnItemEndDrag, this);
        }

        public virtual void OnDrop() {
            InvokeEvent(OnItemDroppedOn, this);
        }
        public virtual void OnPointerClick(BaseEventData data) {
            PointerEventData pointerData = (PointerEventData) data;
            if (pointerData.button == PointerEventData.InputButton.Right)
                InvokeEvent(OnRightMouseBtnClick, this);
            else
                InvokeEvent(OnItemClicked, this);
        }
        private void InvokeEvent(Action<UIItemSlot> action, UIItemSlot slot) {
            UIItemSlot itemSlot = slot;
            if (slot.GetType() == typeof(UIHotbarSlot))
                action?.Invoke((UIHotbarSlot) slot);
            else if (slot.GetType() == typeof(UIEquipmentSlot))
                action?.Invoke((UIEquipmentSlot) slot);
            else
                action?.Invoke((UIItemSlot) slot);
        
        }

        public void ResetData() {
            itemImage.gameObject.SetActive(false);
            itemStack = new ItemStack();
        }
        public void Deselect() {
            borderImage.enabled = false;
        }

        public void SetData(ItemStack item) {
            if (!item.IsEmpty()) {
                itemImage.gameObject.SetActive(true);
                itemStack = item;
                itemStack.SetSlot(item.GetSlotID());
                itemImage.sprite = item.Item.itemIcon;
                quantityTxt.text = itemStack.GetCount() + "";
            }
            else
                ResetData();
        }
        public void SetData(UIItemSlot item) {
            if (!item.IsEmpty()) {
                itemImage.gameObject.SetActive(true);
                itemStack = item.GetItemStack();
                //itemStack.SetSlot(item.GetItemStack().GetSlotID());
                itemImage.sprite = itemStack.Item.itemIcon;
                quantityTxt.text = itemStack.GetCount() + "";
            }
        }

        public void UpdateData() {
            if (!itemStack.IsEmpty()) {
                itemImage.gameObject.SetActive(true);
                itemImage.sprite = itemStack.Item.itemIcon;
                quantityTxt.text = itemStack.GetCount() + "";
            }
            else {
                itemImage.gameObject.SetActive(false);
            }
        }
        public void Select() {
            borderImage.enabled = true;
        }
        public ItemStack GetItemStack() => itemStack;
        public ItemStack GetItemStackCopy() => new ItemStack(itemStack);

        public virtual UIPanelsBase GetParent() => parent;
        public void SetParent(UIInventoryPanel panel) => parent = panel;
        
        public bool IsEmpty() => this.itemStack.IsEmpty();
    }
}