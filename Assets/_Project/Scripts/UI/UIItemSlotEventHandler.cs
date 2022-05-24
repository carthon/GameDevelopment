using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.Handlers;
using UnityEngine;

namespace _Project.Scripts.UI {
    public class UIItemSlotEventHandler : MonoBehaviour{
        protected List<UIItemSlot> uiSlots;
        private UIItemSlot draggedItem;
        
        protected virtual void HandleItemSelection(UIItemSlot itemSlot) {
            Debug.Log(itemSlot.name);
            MouseFollower mouseFollower = UIHandler.instance.mouseFollower;
            if (mouseFollower.Active){
                mouseFollower.Toggle(false);
                HandleSwap(itemSlot);
                UIHandler.instance.draggedItem = null;
            }
            else if (!itemSlot.IsEmpty()){
                mouseFollower.Toggle(true);
                mouseFollower.SetData(itemSlot);
                UIHandler.instance.draggedItem = itemSlot;
            }
        }
        protected virtual void HandleShowItemActions(UIItemSlot obj) {
        }
        protected virtual void HandleSwap(UIItemSlot obj) {
            draggedItem = UIHandler.instance.draggedItem;
            // Item que va a ser sustituido
            int newSlotIndex = uiSlots.IndexOf(obj);
            // Item agarrado
            int lastSlotIndex = uiSlots.IndexOf(draggedItem);
            if (newSlotIndex == -1 && lastSlotIndex == -1) return;

            SwapSlotItems(obj, draggedItem);
            uiSlots[lastSlotIndex].SetData(uiSlots[newSlotIndex]);
            uiSlots[newSlotIndex].SetData(draggedItem);
        }
        private void SwapSlotItems(UIItemSlot draggedItem, UIItemSlot otherItem) {
            ItemStack.SwapItemsStack(draggedItem.GrabItemStack(), otherItem.GrabItemStack());
        }
        protected virtual void HandleEndDrag(UIItemSlot obj) {
        }
        protected virtual void HandleBeginDrag(UIItemSlot obj) {
        }
    }
}