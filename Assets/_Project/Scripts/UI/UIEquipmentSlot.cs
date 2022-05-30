using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.Handlers;
using UnityEngine;

namespace _Project.Scripts.UI {
    public class UIEquipmentSlot : UIItemSlot {
        [SerializeField]private EquipmentSlot equipmentSlot;
        public EquipmentSlot GetEquipmentSlot() => equipmentSlot;
        
        public void EquipItem(ItemStack itemStackToEquip, EquipmentSlotHandler equipmentSlotHandler) {
            Inventory equipmentInventory = equipmentSlotHandler.currentInventory;
            ItemStack equippingItem = itemStackToEquip.GetInventory().TakeStack(itemStackToEquip);
            ItemStack otherItemEquipped = equipmentInventory.GetItem(0);
            if (!otherItemEquipped.IsEmpty()) {
                //bool canFitItemStack = equipmentInventory.SwapItemsInInventory(itemStackToEquip.GetInventory(), otherItemEquipped, itemStackToEquip);
                UnEquipItem(equipmentSlotHandler);
            }
            //equipmentSlotHandler.LoadItemModel(equippingItem);
            equipmentSlotHandler.currentInventory.AddItem(equippingItem);
            SetData(equippingItem);
        }
        public void UnEquipItem(EquipmentSlotHandler equipmentSlotHandler) {
            ItemStack takenItem = equipmentSlotHandler.currentInventory.TakeStackFromSlot(0);
            int itemStackInventory = takenItem.GetInventory().FindItemStackSlot(takenItem);
            if (itemStackInventory == -1) {
                UIHandler.instance.mouseFollower.SetData(this);
                UIHandler.instance.mouseFollower.Toggle(true);
            }else {
                takenItem.GetInventory().AddItem(takenItem);
            }
            equipmentSlotHandler.UnloadItemAndDestroy();
            ResetData();
        }
    }
}