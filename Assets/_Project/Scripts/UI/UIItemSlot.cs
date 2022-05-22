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
        public event Action<UIItemSlot> OnItemClicked, OnItemDroppedOn, OnItemBeginDrag, 
            OnItemEndDrag, OnRightMouseBtnClick, OnPullItemStack;
        private bool empty = true;
        public Inventory Parent { get; set; }

        private void Awake() {
            ResetData();
            Deselect();
        }

        public void ResetData() {
            itemImage.gameObject.SetActive(false);
            _itemStack = null;
            empty = true;
        }
        public void Deselect() {
            borderImage.enabled = false;
        }

        public void SetData(ItemStack item) {
            if (item != null) {
                itemImage.gameObject.SetActive(true);
                _itemStack = item;
                itemImage.sprite = item.Item.itemIcon;
                quantityTxt.text = _itemStack.Count + "";
                empty = false;
            }
        }
        public void SetData(UIItemSlot item) {
            if (item != null) {
                itemImage.gameObject.SetActive(true);
                _itemStack = item.GetItemStack();
                itemImage.sprite = _itemStack.Item.itemIcon;
                quantityTxt.text = _itemStack.Count + "";
                Parent = item.Parent;
                empty = false;
            }
        }

        public void Select() {
            borderImage.enabled = true;
        }

        public void OnBeginDrag() {
            if (empty)
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
        public ItemStack PullItemStack() {
            ItemStack itemStack = _itemStack;
            Parent.PullItem(_itemStack);
            ResetData();
            OnPullItemStack?.Invoke(this);
            return itemStack;
        }
        public ItemStack GetItemStack() => _itemStack;

        public bool IsEmpty() => empty;
    }
}