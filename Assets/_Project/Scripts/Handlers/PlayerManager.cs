using System.Collections.Generic;
using System.Linq;
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
        
        [SerializeField] private Dictionary<BodyPart,EquipmentSlotHandler> equipmentSlotHandlers;

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
            hotbarPanel = (hotbarPanel == null) ? GetComponent<UIHotbarPanel>() : hotbarPanel;
            inventory = new Inventory("Player Inventory", 2);
            UIHandler.instance.AddInventory(inventory);
            equipmentSlotHandlers = new Dictionary<BodyPart, EquipmentSlotHandler>();
            foreach (EquipmentSlotHandler equipmentSlotHandler in GetComponentsInChildren<EquipmentSlotHandler>()) {
                equipmentSlotHandlers.Add(equipmentSlotHandler.GetEquipmentBodyPart(), equipmentSlotHandler);
            }
            moveDirection = Vector3.zero;
            inventory.AddItem(sword);
            inventory.AddItem(dagger);
            EventSubscriber();
        }
        private void EventSubscriber() {
            inputHandler.OnHotbarEquip += HandleEquipment;
            inputHandler.OnLeftHandEquip += HandleEquipment;
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
            if (inputHandler.enableUI) {
                bool isDisplaying = UIHandler.instance.isDisplaying;
                UIHandler.instance.DisplayAllInventories(isDisplaying);
                UIHandler.instance.isDisplaying = !isDisplaying;
                lockCamera = isDisplaying;
            }
            if (inputHandler.equipInput)
                HandleEquipment(inputHandler.hotbarItems);
        }

        #region HandleEquipment
        public void HandleEquipment(int hotbarSlot) {
            UIItemSlot uiSlot = hotbarSlot != -1 ? hotbarPanel.GetItemInSlot(hotbarSlot) :  hotbarPanel.GetItemInSlot(hotbarPanel.activeSlot);
            BodyPart bodypart = hotbarSlot != -1 ? BodyPart.RIGHT_HAND : BodyPart.LEFT_HAND;
            EquipmentSlotHandler equipmentSlotHandler = equipmentSlotHandlers[bodypart];
            ItemStack item = uiSlot.GetItemStack();
            int activeSlot = hotbarPanel.activeSlot;
            #region SelectionMechanic
            if (activeSlot != hotbarPanel.GetSlotFromItem(uiSlot)) {
                hotbarPanel.GetItemInSlot(activeSlot).Deselect();
            }
            if (item == null) {
                uiSlot.Select();
                hotbarPanel.activeSlot = hotbarPanel.GetSlotFromItem(uiSlot);
                equipmentSlotHandler.UnloadItemAndDestroy();
                return;
            }
            uiSlot.Select();
            #endregion
            #region EquipItem
            if (item.Item.GetType() == typeof(Wereable)) {
                Wereable wereable = (Wereable) item.Item;
                equipmentSlotHandler = equipmentSlotHandlers[wereable.GetBodyPart()];
            }
            if (equipmentSlotHandler != null)
                EquipOnSlot(equipmentSlotHandler, uiSlot);
            if (item.Item != null && item.Item.GetType() == typeof(Wereable))
                Debug.Log("Se puede equipar!");
            #endregion
            hotbarPanel.activeSlot = hotbarPanel.GetSlotFromItem(uiSlot);
            UIHandler.instance.SyncAllInventoryPanels();
        }

        private void EquipOnSlot(EquipmentSlotHandler equipmentSlotHandler, UIItemSlot uiSlot) {
            UIItemSlot tmpSlot = uiSlot;
            if (equipmentSlotHandler.currentItemOnSlot != null) {
                uiSlot.SetData(equipmentSlotHandler.currentItemOnSlot);
                uiSlot.Parent = equipmentSlotHandler.lastInventory;
                equipmentSlotHandler.UnloadItemAndDestroy();
            }
            equipmentSlotHandler.LoadItemModel(tmpSlot);
            uiSlot.SetData(equipmentSlotHandler.currentItemOnSlot);
            uiSlot.Parent = equipmentSlotHandler.lastInventory;
            hotbarPanel.UpdateEquippedItem(tmpSlot, equipmentSlotHandler);
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
            inputHandler.enableUI = false;
            inputHandler.equipInput = false;
        }
    }
}