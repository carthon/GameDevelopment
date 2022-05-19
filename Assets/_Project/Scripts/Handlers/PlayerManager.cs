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
            inventory = new Inventory("Player Inventory", 2);
            UIHandler.instance.AddInventory(inventory);
            moveDirection = Vector3.zero;
            inventory.AddItem(sword, 0);
            inventory.AddItem(dagger, 1);
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
                UIHandler.instance.hotbarPanel.OnInventoryEquipItem += HandleEquipment;
            }
        }
        private void HandleEquipment(UIItemSlot item) {
            if (item.GetItemStack().Item.GetType() == typeof(WeaponItem)) {
                WeaponItem weaponItem = (WeaponItem) item.GetItemStack().Item;
                equimentSlotHandlers[1].LoadItemModel(item);
            }
        }

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
        }
    }
}