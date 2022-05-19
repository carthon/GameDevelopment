using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using _Project.Scripts.Components;
using _Project.Scripts.Components.Items;
using _Project.Scripts.UI;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class PlayerManager : MonoBehaviour {
        private InputHandler inputHandler;
        private Locomotion locomotion;
        private Animator animator;
        private Inventory inventory;
        private AgressionHandler agressionHandler;
        private List<EquipmentSlotHandler> equimentSlotHandlers;

        [SerializeField] private UIHotbarPanel hotbarPanel;
        
        private CameraHandler cameraHandler;
        public bool lockCamera;
        public Item sword;
        public Item dagger;

        public bool isInteracting;
        private Vector3 moveDirection;
        

        private static readonly int IsInteracting = Animator.StringToHash("isInteracting");

        private void Start() {
            cameraHandler = CameraHandler.Singleton;
            inputHandler = GetComponent<InputHandler>();
            animator = GetComponentInChildren<Animator>();
            locomotion = GetComponent<Locomotion>();
            agressionHandler = GetComponent<AgressionHandler>();
            equimentSlotHandlers = GetComponentsInChildren<EquipmentSlotHandler>().ToList();
            hotbarPanel = (hotbarPanel == null) ? GetComponent<UIHotbarPanel>() : hotbarPanel;
            inventory = new Inventory("Player Inventory", 2);
            UIHandler.instance.AddInventory(inventory);
            moveDirection = Vector3.zero;
            inventory.AddItem(sword, 0);
            inventory.AddItem(dagger, 1);
            EventSubscriber();
        }
        private void EventSubscriber() {
            hotbarPanel.OnInventoryEquipItem += HandleEquipment;
        }

        private void FixedUpdate() {
            float delta = Time.fixedDeltaTime;
            if (cameraHandler != null) {
                TickCamera(delta);
            }
        }
        
        private void TickCamera(float delta){
            cameraHandler.FollowTarget(delta);
            if (!lockCamera) {
                cameraHandler.HandleCameraRotation(delta, inputHandler.mouseX, inputHandler.mouseY);
            }
        }

        private void Update() {
            float delta = Time.deltaTime;
            
            isInteracting = animator.GetBool(IsInteracting);

            inputHandler.TickInput(delta);

            HandleUI(delta);
            
            HandleLocomotion(delta);
            
            //if (inputHandler.rb_Input)
                //agressionHandler.HandleLightAttack((WeaponItem) hotbarHandler.GetActiveItem());
            //if (inputHandler.rt_Input)
            //agressionHandler.HandleHeavyAttack((WeaponItem) hotbarHandler.GetActiveItem());
        }

        private void HandleUI(float delta) {
            if (inputHandler.playerOverview) {
                bool isDisplaying = UIHandler.instance.isDisplaying;
                UIHandler.instance.DisplayAllInventories(isDisplaying);
                UIHandler.instance.isDisplaying = !isDisplaying;
                lockCamera = isDisplaying;
            }
            HandleEquipment(inputHandler.hotbarSlot, inputHandler.leftHandEquip);
        }

        #region HandleEquipment

        public void HandleEquipment(UIItemSlot item) => HandleEquipment(item, false);
        public void HandleEquipment(UIItemSlot item, bool isLeft = false) {
            int activeSlot = hotbarPanel.activeSlot;
            int hand = isLeft ? 0 : 1;
            int otherHand = isLeft ? 1 : 0;
            equimentSlotHandlers[hand].LoadItemModel(item);
            if (isLeft) {
                if (equimentSlotHandlers[otherHand] == null)
                    equimentSlotHandlers[otherHand].UnloadItemAndDestroy();
                else
                    SwapEquipment(equimentSlotHandlers[hand], equimentSlotHandlers[otherHand]);
            }
            if (activeSlot != hotbarPanel.GetSlotFromItem(item)) {
                hotbarPanel.GetItemInSlot(activeSlot).Deselect();
            }
            item.Select();
            hotbarPanel.activeSlot = hotbarPanel.GetSlotFromItem(item);
        }
        public void HandleEquipment(int slot, bool isLeft = false) {
            if ((hotbarPanel.activeSlot != slot && !isLeft) 
                || isLeft) {
                UIItemSlot item = hotbarPanel.GetItemInSlot(slot);
                if (item != null)
                    HandleEquipment(item, isLeft);
            }
        }
        private void SwapEquipment(EquipmentSlotHandler equipmentSlotHandler1, EquipmentSlotHandler equipmentSlotHandler2) {
            Item tmp = equipmentSlotHandler2.currentItemOnSlot;
            equipmentSlotHandler2.LoadItemModel(equipmentSlotHandler1.currentItemOnSlot);
            equipmentSlotHandler1.LoadItemModel(tmp);
        }
        #endregion

        private void HandleLocomotion(float delta) {
            moveDirection = cameraHandler.cameraTransform.forward * inputHandler.vertical;
            moveDirection += cameraHandler.cameraTransform.right * inputHandler.horizontal;
            moveDirection.y = 0;
            locomotion.isRolling = inputHandler.rollFlag;
            locomotion.isSprinting = inputHandler.sprintFlag;
            locomotion.HandleMovement(delta, moveDirection);
            locomotion.HandleRollingAndSprinting(delta, moveDirection);
            locomotion.HandleFalling(delta, moveDirection);
        }

        private void LateUpdate() {
            inputHandler.rollFlag = false;
            inputHandler.sprintFlag = false;
            inputHandler.rb_Input = false;
            inputHandler.rt_Input = false;
            inputHandler.playerOverview = false;
            inputHandler.leftHandEquip = false;
        }
    }
}