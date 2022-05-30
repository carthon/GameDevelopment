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
            UIHandler.instance.SetPlayer(this);
            moveDirection = Vector3.zero;
            inventory.AddItem(sword);
            inventory.AddItem(dagger);
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
                bool isLeft = inputHandler.hotbarItems == -1;
                hotbarPanel.UseItem(inputHandler.hotbarItems, isLeft);
                inputHandler.equipInput = false;
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
            inputHandler.enableUI = false;
        }
    }
}