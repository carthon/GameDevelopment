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
            }
            else {
                mouseFollower.Toggle(true);
                mouseFollower.SetData(itemSlot.GetItemStack());
                UIHandler.instance.draggedItem = itemSlot;
            }
        }
        protected virtual void HandleShowItemActions(UIItemSlot obj) {
        }
        protected virtual void HandleSwap(UIItemSlot obj) {
            draggedItem = UIHandler.instance.draggedItem;
            Debug.Log("Handling Swap (Base) with" + obj);
            // Item que va a ser sustituido
            int newSlotIndex = uiSlots.IndexOf(obj);
            ItemStack newItemStack = draggedItem.GetItemStack();
            // Item agarrado
            int lastSlotIndex = uiSlots.IndexOf(draggedItem);
            ItemStack oldItemStack = uiSlots[newSlotIndex].GetItemStack();
            if (newSlotIndex == -1 && lastSlotIndex == -1) return;

            obj.Parent.SwapSlotItems(obj, draggedItem);
            uiSlots[lastSlotIndex].SetData(oldItemStack);
            uiSlots[newSlotIndex].SetData(newItemStack);
            UIHandler.instance.draggedItem = null;
        }
        protected virtual void HandleEndDrag(UIItemSlot obj) {
        }
        protected virtual void HandleBeginDrag(UIItemSlot obj) {
        }
    }
}