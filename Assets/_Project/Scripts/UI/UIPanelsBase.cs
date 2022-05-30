using System;
using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using UnityEngine;

namespace _Project.Scripts.UI {
    public abstract class UIPanelsBase : MonoBehaviour {
        [SerializeField] protected Dictionary<BodyPart,EquipmentSlotHandler> equipmentSlotHandlers;
        protected PlayerManager player;

        protected virtual void Start() {
            player = UIHandler.instance.GetPlayer();
            equipmentSlotHandlers = new Dictionary<BodyPart, EquipmentSlotHandler>();
            foreach (EquipmentSlotHandler equipmentSlotHandler in player.GetComponentsInChildren<EquipmentSlotHandler>()) {
                equipmentSlotHandlers.Add(equipmentSlotHandler.GetEquipmentBodyPart(), equipmentSlotHandler);
            }
        }

        protected virtual void HandleItemSelection(UIItemSlot itemSlot) {
            Debug.Log(itemSlot.GetType());
            MouseFollower mouseFollower = UIHandler.instance.mouseFollower;
            if (mouseFollower.Active) {
                mouseFollower.Toggle(false);
                itemSlot.GetParent().HandleSwap(itemSlot);
                mouseFollower.ResetData();
            }
            else if (!itemSlot.IsEmpty()) {
                mouseFollower.Toggle(true);
                mouseFollower.SetData(itemSlot);
            }
        }
        public virtual void HandleSwap(UIItemSlot obj) {
            Debug.Log("Base implementation");
        }
        protected virtual void HandleShowItemActions(UIItemSlot obj) {
        }
        protected virtual void HandleEndDrag(UIItemSlot obj) {
        }
        protected virtual void HandleBeginDrag(UIItemSlot obj) {
        }
    }
}