using System;
using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.Client;
using QuickOutline.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState {
    public class InventoryInterfaceState : InterfaceAbstractBaseState {
        
        private Grabbable _outlinedGrabbable;
        private Player _player;
        public List<Transform> GrabbedItems { get; set; }
        public List<Vector3> LastGrabbedItemsLocalPosition { get; set; }
        public RaycastHit HitPoint;

        public InventoryInterfaceState(InterfaceStateFactory factory, UIHandler context) : base(factory, context) {
            _isRootState = true;
            InitializeSubState();
        }
        protected override void UpdateState() {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            Camera cam = CameraHandler.Singleton.MainCamera;
            Ray ray = cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, Single.PositiveInfinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide)) {
                HitPoint = hit;
                Outline outlineParent = hit.collider.GetComponentInParent<Outline>();
                if (hit.collider.TryGetComponent(out Outline outline) || outlineParent != null) {
                    outline = outlineParent;
                    if (HitMouseOutline != null && !outline.transform.Equals(HitMouseOutline.transform)) {
                        HitMouseOutline.enabled = false;
                    }
                    HitMouseOutline = outline;
                    HitMouseOutline.enabled = true;
                } else {
                    ResetMouseSelection();
                }
            }
            else {
                ResetMouseSelection();
            }
            CheckSwitchStates();
        }
        public override void CheckSwitchStates() {
            if (!InputHandler.Singleton.IsInInventory) {
                SwitchState(Context.LastState);
            }
        }
        public sealed override void InitializeSubState() {
            SetSubState(Factory.InventorySelectItemState());
        }
        protected sealed override void EnterState() {
            _player = Client.Singleton.Player;
            Cursor.lockState = CursorLockMode.None;
            ContainerRenderer.Singleton.ToggleRender(true);
            Quaternion lookRotation = Quaternion.LookRotation(_player.inventorySpawnTransform.position - _player.HeadPivot.position);
            _player.HeadPivot.rotation = lookRotation;
            GrabbedItems = new List<Transform>();
        }
        protected override void ExitState() {
            ContainerRenderer.Singleton.ToggleRender(false);
            ResetMouseSelection();
        }
        public override string StateName() => "InventoryInterfaceState->" + CurrentSubState?.StateName();
    }
}