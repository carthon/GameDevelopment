using System;
using System.Collections.Generic;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.Client;
using QuickOutline.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState {
    public class InventoryInterfaceState : InterfaceAbstractBaseState {
        
        private Grabbable _outlinedGrabbable;
        private Vector3 _lookAtPosition;
        private float headRotationSpeed = 0.5f;
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
            
            CameraHandler.Singleton.lookAtTransform.position = (Vector3.Distance(CameraHandler.Singleton.lookAtTransform.position, _lookAtPosition) > 2f) ? _lookAtPosition :
                Vector3.Lerp(CameraHandler.Singleton.lookAtTransform.position, _lookAtPosition, Time.deltaTime * headRotationSpeed);
            Quaternion lookRotation = Quaternion.LookRotation(_lookAtPosition - _player.HeadPivot.position);
            _player.HeadPivot.rotation = Quaternion.Lerp(_player.HeadPivot.rotation, lookRotation, Time.deltaTime * headRotationSpeed);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Single.PositiveInfinity, (1 << LayerMask.NameToLayer("Default")), QueryTriggerInteraction.Collide)) {
                HitPoint = hit;
                Context.UpdateWatchedVariables("SelectedItem", $"SelectedItem:{hit.collider.name}");
                Outline outlineParent = hit.collider.GetComponentInParent<Outline>();
                if (hit.collider.TryGetComponent(out Outline outline) || outlineParent != null) {
                    _lookAtPosition = hit.collider.transform.position;
                    outline = outlineParent;
                    if (HitMouseOutline != null && !outline.transform.Equals(HitMouseOutline.transform)) {
                        HitMouseOutline.enabled = false;
                    }
                    HitMouseOutline = outline;
                    if (CurrentSubState.GetType() != typeof(InventoryGrabItemInterfaceState)) HitMouseOutline.enabled = true;
                    
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
            _lookAtPosition = ContainerRenderer.Singleton.SpawnPoint.position;
            Cursor.lockState = CursorLockMode.None;
            ContainerRenderer.Singleton.ToggleRender(true);
            GrabbedItems = new List<Transform>();
        }
        protected override void ExitState() {
            ContainerRenderer.Singleton.ToggleRender(false);
            ResetMouseSelection();
        }
        public override string StateName() => "InventoryInterfaceState->" + CurrentSubState?.StateName();
    }
}