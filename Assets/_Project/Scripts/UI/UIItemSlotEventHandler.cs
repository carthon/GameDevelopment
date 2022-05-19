using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.Handlers;
using UnityEngine;

namespace _Project.Scripts.UI {
    public class UIItemSlotEventHandler : MonoBehaviour{
        protected List<UIItemSlot> uiSlots;
        protected UIItemSlot draggedItem;
        
        protected virtual void HandleItemSelection(UIItemSlot itemSlot) {
            Debug.Log(itemSlot.name);
        }
        protected virtual void HandleShowItemActions(UIItemSlot obj) {
        }
        protected virtual void HandleSwap(UIItemSlot obj) {
            draggedItem = UIHandler.instance.draggedItem;
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
        }
        protected virtual void HandleEndDrag(UIItemSlot obj) {
            MouseFollower mouseFollower = UIHandler.instance.mouseFollower;
            mouseFollower.Toggle(false);
        }
        protected virtual void HandleBeginDrag(UIItemSlot obj) {
            MouseFollower mouseFollower = UIHandler.instance.mouseFollower;
            mouseFollower.Toggle(true);
            if (obj.GetItemStack() != null) {
                mouseFollower.SetData(obj.GetItemStack());
                UIHandler.instance.draggedItem = obj;
            }
            else
                Debug.Log("Couldnt grab an empty object!");
        }
    }
}