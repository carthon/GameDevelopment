using System;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Scripts.UI {
    public class UIItemSlot : MonoBehaviour {
        private ItemStack _itemStack;
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text quantityTxt;
        [SerializeField] private Image borderImage;
        public int SlotID { get; set; }

        public event Action<UIItemSlot> OnItemClicked, OnItemDroppedOn, OnItemBeginDrag, 
            OnItemEndDrag, OnRightMouseBtnClick, OnPullItemStack;

        private void Awake() {
            ResetData();
            Deselect();
        }

        public void ResetData() {
            itemImage.gameObject.SetActive(false);
            _itemStack = new ItemStack();
        }
        public void Deselect() {
            borderImage.enabled = false;
        }

        public void SetData(ItemStack item) {
            if (!item.IsEmpty()) {
                itemImage.gameObject.SetActive(true);
                _itemStack = item;
                _itemStack.SetSlot(item.GetSlotID());
                itemImage.sprite = item.Item.itemIcon;
                quantityTxt.text = _itemStack.GetCount() + "";
            }
        }
        public void SetData(UIItemSlot item) {
            if (!item.IsEmpty()) {
                itemImage.gameObject.SetActive(true);
                _itemStack = item.GetItemStack();
                _itemStack.SetSlot(item.GetItemStack().GetSlotID());
                itemImage.sprite = _itemStack.Item.itemIcon;
                quantityTxt.text = _itemStack.GetCount() + "";
            }
        }

        public void Select() {
            borderImage.enabled = true;
        }

        public void OnBeginDrag() {
            if (_itemStack.IsEmpty())
                return;
            OnItemBeginDrag?.Invoke(this);
        }
        public void OnEndDrag() {
            OnItemEndDrag?.Invoke(this);
        }

        public void OnDrop() {
            OnItemDroppedOn?.Invoke(this);
        }
        public void OnPointerClick(BaseEventData data) {
            PointerEventData pointerData = (PointerEventData) data;
            if(pointerData.button == PointerEventData.InputButton.Right)
                OnRightMouseBtnClick?.Invoke(this);
            else 
                OnItemClicked?.Invoke(this);
        }
        public ItemStack GrabItemStack() {
            ItemStack itemStack;
            if (!IsEmpty())
                itemStack = _itemStack.GetInventory().TakeStack(_itemStack);
            else
                itemStack = _itemStack.GetCopy();
            OnPullItemStack?.Invoke(this);
            return itemStack;
        }
        public ItemStack GetItemStack() => _itemStack;
        public ItemStack GetItemStackCopy() => new ItemStack(_itemStack);

        public bool IsEmpty() => this._itemStack.IsEmpty();
    }
}