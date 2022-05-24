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
            if (inputHandler.equipInput) {
                HandleEquipment(inputHandler.hotbarItems);
                inputHandler.equipInput = false;
            }
        }

        #region HandleEquipment
        public void HandleEquipment(int hotbarSlot) {
            UIItemSlot uiSlot = hotbarSlot != -1 ? hotbarPanel.GetItemHolderInSlot(hotbarSlot) :  hotbarPanel.GetItemHolderInSlot(hotbarPanel.activeSlot);
            BodyPart bodypart = hotbarSlot != -1 ? BodyPart.RIGHT_HAND : BodyPart.LEFT_HAND;
            EquipmentSlotHandler equipmentSlotHandler = equipmentSlotHandlers[bodypart];
            int activeSlot = hotbarPanel.activeSlot;
            int nextActiveSlot = hotbarPanel.GetSlotFromItemHolder(uiSlot);
            ItemStack item = uiSlot.GetItemStack();
            
            if (activeSlot != nextActiveSlot) {
                hotbarPanel.GetItemHolderInSlot(activeSlot).Deselect();
            }
            // @TODO: Revisar el cuando equipa o desequipa un objeto, ahora los equipmentSlots tienen un inventario de un slot
            if (item.Equals(equipmentSlotHandler.currentInventory.GetItem(0))) {
                UnEquipItem(uiSlot, equipmentSlotHandler);
            }else
                EquipItem(uiSlot, equipmentSlotHandler);
            if (!item.IsEmpty() && item.Item.GetType() == typeof(Wereable))
                Debug.Log("Se puede equipar!");
            hotbarPanel.activeSlot = nextActiveSlot;
        }
        #endregion
        private void EquipItem(UIItemSlot uiItemSlot, EquipmentSlotHandler equipmentSlotHandler)
        {
            uiItemSlot.Select();
            ItemStack itemStack = uiItemSlot.GrabItemStack();
            EquipmentSlotHandler sameItemEquiped = equipmentSlotHandlers.Values.ToList()
                .Find(otherItem => otherItem.currentInventory.Equals(itemStack));
            if (equipmentSlotHandler.IsEmpty() && sameItemEquiped == null)
                equipmentSlotHandler.LoadItemModel(itemStack);
            else if(!itemStack.IsEmpty() || sameItemEquiped != null) {
                ItemStack realItemStack = sameItemEquiped != null ? sameItemEquiped.currentInventory.TakeStackFromSlot(0) : itemStack;
                ItemStack.SwapItemsStack(realItemStack, equipmentSlotHandler.currentInventory.TakeStackFromSlot(0));
                if (sameItemEquiped == null) 
                    itemStack.GetInventory().AddItem(realItemStack);
                else if (sameItemEquiped.IsEmpty()){
                    hotbarPanel.GetEquipmentSlots()[(int) sameItemEquiped.GetEquipmentBodyPart()].ResetData();
                    sameItemEquiped.UnloadItemAndDestroy();
                }
                else {
                    hotbarPanel.GetEquipmentSlots()[(int) sameItemEquiped.GetEquipmentBodyPart()].SetData(sameItemEquiped.currentInventory.GetItem(0));
                    sameItemEquiped.LoadItemModel(sameItemEquiped.currentInventory.GetItem(0));
                }
                    
                equipmentSlotHandler.LoadItemModel(equipmentSlotHandler.currentInventory.GetItem(0));
            }
            hotbarPanel.GetEquipmentSlots()[(int)equipmentSlotHandler.GetEquipmentBodyPart()].SetData(equipmentSlotHandler.currentInventory.GetItem(0));
            //UIHandler.instance.SyncAllInventoryPanels();
        }

        private void UnEquipItem(UIItemSlot uiItemSlot, EquipmentSlotHandler equipmentSlotHandler)
        {
            uiItemSlot.Select();
            ItemStack itemStack = equipmentSlotHandler.currentInventory.TakeStackFromSlot(0);
            itemStack.GetInventory().AddItem(itemStack);
            equipmentSlotHandler.UnloadItemAndDestroy();
            hotbarPanel.GetEquipmentSlots()[(int)equipmentSlotHandler.GetEquipmentBodyPart()].ResetData();
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
            inputHandler.enableUI = false;
        }
    }
}