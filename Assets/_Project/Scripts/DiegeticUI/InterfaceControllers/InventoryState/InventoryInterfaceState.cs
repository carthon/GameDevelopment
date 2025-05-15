using System;
using System.Collections.Generic;
using _Project.Libraries.QuickOutline.Scripts;
using _Project.Scripts.Components;
using _Project.Scripts.Handlers;
using _Project.Scripts.Network.Client;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState {
    public class InventoryInterfaceState : InterfaceAbstractBaseState {
        
        private Grabbable _outlinedGrabbable;
        private Vector3 _lookAtPosition;
        private float headRotationSpeed = 0.5f;
        private Player _player;
        private Grid _grid;
        public List<Collider> GrabbedItemsCollider { get; set; }
        public List<Vector3> LastGrabbedItemsLocalPosition { get; set; }
        public RaycastHit RayCastHit;
        public Vector3 ProjectedPositionOverPlane;

        public InventoryInterfaceState(InterfaceStateFactory factory, ContainerRenderer context) : base(factory, context) {
            _isRootState = true;
            LastGrabbedItemsLocalPosition = new List<Vector3>();
            GrabbedItemsCollider = new List<Collider>();
            InitializeSubState();
        }
        protected override void UpdateState() {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            Camera cam = CameraHandler.Singleton.MainCamera;
            Ray ray = cam.ScreenPointToRay(mousePos);
            
            CameraHandler.Singleton.staticLookAtTransform.position = 
                Vector3.Lerp(CameraHandler.Singleton.staticLookAtTransform.position, _lookAtPosition, Time.deltaTime * headRotationSpeed);
            if(_player) {
                Quaternion lookRotation = Quaternion.LookRotation(_lookAtPosition - _player.HeadPivot.position);
                _player.HeadPivot.rotation = Quaternion.Lerp(_player.HeadPivot.rotation, lookRotation, Time.deltaTime * headRotationSpeed);
            }
            
            Plane gridPlane = new Plane(_player.Model.up, UIHandler.Instance.currentContainer.inventorySpawn.position);
            if (gridPlane.Raycast(ray, out float enter) && enter < 4f) {
                Vector3 hitPoint = ray.GetPoint(enter);
                ProjectedPositionOverPlane = hitPoint;
                Vector3Int gridPosition = _grid.WorldToCell(ProjectedPositionOverPlane);
                Vector3 targetPosition = _grid.CellToWorld(gridPosition);
                _lookAtPosition = hitPoint;
                UIHandler.Instance.inventoryCellIndicator.transform.rotation = _grid.transform.rotation;
                UIHandler.Instance.inventoryCellIndicator.transform.position = targetPosition;
                UIHandler.Instance.UpdateWatchedVariables("SelectedCell", $"GridPosition: {gridPosition}");
                if (Physics.Raycast(ray, out RayCastHit, Single.PositiveInfinity, LayerMask.GetMask("Inventory"), QueryTriggerInteraction.Collide)) {
                    if (RayCastHit.collider.CompareTag(global::Constants.TAG_UISLOT)) {
                        if (RayCastHit.collider.TryGetComponent(out Outline outline)) {
                            if (!(HitMouseOutline is null) && outline is not null && !outline.transform.Equals(HitMouseOutline.transform)) {
                                HitMouseOutline.enabled = false;
                            }
                            HitMouseOutline = outline;
                            if (CurrentSubState.GetType() != typeof(InventoryGrabItemInterfaceState)) HitMouseOutline.enabled = true;
                    
                        } else {
                            ResetMouseSelection();
                        }
                    } else {
                        ResetMouseSelection();
                    }
                }
                else {
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
                SwitchState(UIHandler.Instance.LastState);
            }
        }
        public sealed override void InitializeSubState() {
            SetSubState(Factory.InventorySelectItemState());
        }
        
        protected sealed override void EnterState() {
            _player = ClientHandler.Singleton.Player;
            _grid   = Context.inventoryGrid;
            Vector3 cellSize = _grid.cellSize;

            // 1) Datos del grid
            float totalWidth  = Context.Inventory.Width  * cellSize.x;
            float totalHeight = Context.Inventory.Height * cellSize.y;
            // offset desde esquina sup-izda → centro, en local del grid:
            // (X positivo = derecha, Z positivo = abajo)
            Vector3 centerOffset = new Vector3(totalWidth/2f, 0f, -totalHeight/2f);

            // 2) Toma el transform del controller
            Transform inventoryTransform = _player.inventoryControllerTransform;

            // 3) Usa EL forward y up del controller para rotar el Context
            Vector3 forward = inventoryTransform.forward;
            Vector3 up = inventoryTransform.up;  // si quieres alinear “hacia arriba” con el jugador
            Quaternion worldRot = Quaternion.LookRotation(forward, up);

            // 4) Posición del “centro” delante del controller
            Vector3 worldCenter = inventoryTransform.position + forward * (cellSize.x * _player.inventoryDistance);

            // 5) Calcula el offset rotado y la posición final
            Vector3 offsetWorld = worldRot * centerOffset;
            Vector3 finalPos = worldCenter - offsetWorld;

            // 6) Asigna directo en world para evitar lios de jerarquías
            Context.transform.position = finalPos;
            Context.transform.rotation = worldRot;
            
            _lookAtPosition = _grid.GetCellCenterWorld(new Vector3Int(Context.Inventory.Width/2, 0 , -Context.Inventory.Height/2));
            CameraHandler.Singleton.staticLookAtTransform.position = _lookAtPosition;
            Quaternion lookRotation = Quaternion.LookRotation(_lookAtPosition - _player.HeadPivot.position);
            _player.HeadPivot.rotation = lookRotation;
            
            UIHandler.Instance.inventoryCellIndicator.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            GrabbedItemsCollider = new List<Collider>();
        }
        
        protected override void ExitState() {
            ResetMouseSelection();
        }
        public override string StateName() => "InventoryInterfaceState->" + CurrentSubState?.StateName();
    }
}