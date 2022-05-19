// GENERATED AUTOMATICALLY FROM 'Assets/_Project/Scripts/InputSystem/PlayerControlls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerControlls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControlls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControlls"",
    ""maps"": [
        {
            ""name"": ""PlayerSpaceMovement"",
            ""id"": ""fd5035ff-7c80-4c30-ac1b-a4d3dcf06545"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""PassThrough"",
                    ""id"": ""1bf06d92-fa6e-4bd1-ba32-42ac9dd39b4c"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Camera"",
                    ""type"": ""PassThrough"",
                    ""id"": ""e01be9a7-43da-41d8-a438-ebbaa72f180b"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""cdab9190-d6f5-4e0f-91a4-68e42e596390"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""5ad25c17-92ea-473b-97b6-69b30f13c73e"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""6b83d2e6-2608-432c-9040-5364e139143a"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""359151fa-35de-4b9e-b915-f345f55a69f8"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""184848fe-97d1-44fc-b8db-9387df2a9052"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""904ffb5f-9785-4dba-9a32-51e628c62e61"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": ""AxisDeadzone"",
                    ""groups"": """",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""642fde7a-c78f-44d7-9a4a-0c59546eb3a3"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": ""Normalize"",
                    ""groups"": """",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""PlayerActions"",
            ""id"": ""310acb34-0f9c-47db-824e-9ee013813a9a"",
            ""actions"": [
                {
                    ""name"": ""Roll"",
                    ""type"": ""Button"",
                    ""id"": ""9fbf0acb-7c41-4717-9a37-1569303e5689"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RB"",
                    ""type"": ""Button"",
                    ""id"": ""43317496-5c0c-4fb6-b377-04df473b1239"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RT"",
                    ""type"": ""Button"",
                    ""id"": ""12efddb3-fc81-471f-b625-f526e6780aa5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""91e4997a-1309-4779-8059-cae20060cfe7"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Roll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2c7eeb8a-d84e-4a98-9349-7da7317b5d39"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Roll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0d55fd61-0036-4582-b94f-21e8e2b54abb"",
                    ""path"": ""<Keyboard>/t"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RB"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""936326a1-fc9e-4520-affa-2ddeb71a9c55"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RT"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UIActions"",
            ""id"": ""15ce1bd9-6148-4b17-a123-00ec85a2b2a0"",
            ""actions"": [
                {
                    ""name"": ""PlayerOverview"",
                    ""type"": ""Button"",
                    ""id"": ""d2864df3-9bc6-4baf-9831-feb1aa6560e7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""HotbarInput"",
                    ""type"": ""Value"",
                    ""id"": ""2977c2ec-6acd-422c-8f9f-daab8847678e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""EquipLeftHand"",
                    ""type"": ""PassThrough"",
                    ""id"": ""65087058-4e50-4458-b3b5-34b33ad9a911"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""3d57bc1b-065b-429b-92f7-f917c2fc41ca"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PlayerOverview"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""18f28a16-ff2e-4be4-93fd-0b399f373e27"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=0)"",
                    ""groups"": """",
                    ""action"": ""HotbarInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""66f0d446-bbd3-45b6-a078-3505e626d8c2"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": ""Scale"",
                    ""groups"": """",
                    ""action"": ""HotbarInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7c781b78-f303-4097-936b-8494dedaf5e8"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=2)"",
                    ""groups"": """",
                    ""action"": ""HotbarInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7d1f8160-eaec-45bf-a575-1a748ac82d0d"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=3)"",
                    ""groups"": """",
                    ""action"": ""HotbarInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0f1a5f8b-68c5-4c22-b8c0-034eb715723a"",
                    ""path"": ""<Keyboard>/5"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=4)"",
                    ""groups"": """",
                    ""action"": ""HotbarInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0da877fb-bdb4-43dc-a787-cf1af7311c67"",
                    ""path"": ""<Keyboard>/6"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=5)"",
                    ""groups"": """",
                    ""action"": ""HotbarInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bfa15fc1-4fdf-45f1-9a9b-13cc383e9409"",
                    ""path"": ""<Keyboard>/7"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=6)"",
                    ""groups"": """",
                    ""action"": ""HotbarInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4a85d38f-8a5f-4989-8aa7-4e2bde533c64"",
                    ""path"": ""<Keyboard>/8"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=7)"",
                    ""groups"": """",
                    ""action"": ""HotbarInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""929b2ddc-c70a-47dc-bd3f-dc9b421a2610"",
                    ""path"": ""<Keyboard>/9"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=8)"",
                    ""groups"": """",
                    ""action"": ""HotbarInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c0e04e5c-818d-4669-80f6-1e803f414c9c"",
                    ""path"": ""<Keyboard>/0"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=9)"",
                    ""groups"": """",
                    ""action"": ""HotbarInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a4e092ca-c9ce-4014-8eb3-397f76904945"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""EquipLeftHand"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // PlayerSpaceMovement
        m_PlayerSpaceMovement = asset.FindActionMap("PlayerSpaceMovement", throwIfNotFound: true);
        m_PlayerSpaceMovement_Movement = m_PlayerSpaceMovement.FindAction("Movement", throwIfNotFound: true);
        m_PlayerSpaceMovement_Camera = m_PlayerSpaceMovement.FindAction("Camera", throwIfNotFound: true);
        // PlayerActions
        m_PlayerActions = asset.FindActionMap("PlayerActions", throwIfNotFound: true);
        m_PlayerActions_Roll = m_PlayerActions.FindAction("Roll", throwIfNotFound: true);
        m_PlayerActions_RB = m_PlayerActions.FindAction("RB", throwIfNotFound: true);
        m_PlayerActions_RT = m_PlayerActions.FindAction("RT", throwIfNotFound: true);
        // UIActions
        m_UIActions = asset.FindActionMap("UIActions", throwIfNotFound: true);
        m_UIActions_PlayerOverview = m_UIActions.FindAction("PlayerOverview", throwIfNotFound: true);
        m_UIActions_HotbarInput = m_UIActions.FindAction("HotbarInput", throwIfNotFound: true);
        m_UIActions_EquipLeftHand = m_UIActions.FindAction("EquipLeftHand", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // PlayerSpaceMovement
    private readonly InputActionMap m_PlayerSpaceMovement;
    private IPlayerSpaceMovementActions m_PlayerSpaceMovementActionsCallbackInterface;
    private readonly InputAction m_PlayerSpaceMovement_Movement;
    private readonly InputAction m_PlayerSpaceMovement_Camera;
    public struct PlayerSpaceMovementActions
    {
        private @PlayerControlls m_Wrapper;
        public PlayerSpaceMovementActions(@PlayerControlls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement => m_Wrapper.m_PlayerSpaceMovement_Movement;
        public InputAction @Camera => m_Wrapper.m_PlayerSpaceMovement_Camera;
        public InputActionMap Get() { return m_Wrapper.m_PlayerSpaceMovement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerSpaceMovementActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerSpaceMovementActions instance)
        {
            if (m_Wrapper.m_PlayerSpaceMovementActionsCallbackInterface != null)
            {
                @Movement.started -= m_Wrapper.m_PlayerSpaceMovementActionsCallbackInterface.OnMovement;
                @Movement.performed -= m_Wrapper.m_PlayerSpaceMovementActionsCallbackInterface.OnMovement;
                @Movement.canceled -= m_Wrapper.m_PlayerSpaceMovementActionsCallbackInterface.OnMovement;
                @Camera.started -= m_Wrapper.m_PlayerSpaceMovementActionsCallbackInterface.OnCamera;
                @Camera.performed -= m_Wrapper.m_PlayerSpaceMovementActionsCallbackInterface.OnCamera;
                @Camera.canceled -= m_Wrapper.m_PlayerSpaceMovementActionsCallbackInterface.OnCamera;
            }
            m_Wrapper.m_PlayerSpaceMovementActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Movement.started += instance.OnMovement;
                @Movement.performed += instance.OnMovement;
                @Movement.canceled += instance.OnMovement;
                @Camera.started += instance.OnCamera;
                @Camera.performed += instance.OnCamera;
                @Camera.canceled += instance.OnCamera;
            }
        }
    }
    public PlayerSpaceMovementActions @PlayerSpaceMovement => new PlayerSpaceMovementActions(this);

    // PlayerActions
    private readonly InputActionMap m_PlayerActions;
    private IPlayerActionsActions m_PlayerActionsActionsCallbackInterface;
    private readonly InputAction m_PlayerActions_Roll;
    private readonly InputAction m_PlayerActions_RB;
    private readonly InputAction m_PlayerActions_RT;
    public struct PlayerActionsActions
    {
        private @PlayerControlls m_Wrapper;
        public PlayerActionsActions(@PlayerControlls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Roll => m_Wrapper.m_PlayerActions_Roll;
        public InputAction @RB => m_Wrapper.m_PlayerActions_RB;
        public InputAction @RT => m_Wrapper.m_PlayerActions_RT;
        public InputActionMap Get() { return m_Wrapper.m_PlayerActions; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActionsActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActionsActions instance)
        {
            if (m_Wrapper.m_PlayerActionsActionsCallbackInterface != null)
            {
                @Roll.started -= m_Wrapper.m_PlayerActionsActionsCallbackInterface.OnRoll;
                @Roll.performed -= m_Wrapper.m_PlayerActionsActionsCallbackInterface.OnRoll;
                @Roll.canceled -= m_Wrapper.m_PlayerActionsActionsCallbackInterface.OnRoll;
                @RB.started -= m_Wrapper.m_PlayerActionsActionsCallbackInterface.OnRB;
                @RB.performed -= m_Wrapper.m_PlayerActionsActionsCallbackInterface.OnRB;
                @RB.canceled -= m_Wrapper.m_PlayerActionsActionsCallbackInterface.OnRB;
                @RT.started -= m_Wrapper.m_PlayerActionsActionsCallbackInterface.OnRT;
                @RT.performed -= m_Wrapper.m_PlayerActionsActionsCallbackInterface.OnRT;
                @RT.canceled -= m_Wrapper.m_PlayerActionsActionsCallbackInterface.OnRT;
            }
            m_Wrapper.m_PlayerActionsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Roll.started += instance.OnRoll;
                @Roll.performed += instance.OnRoll;
                @Roll.canceled += instance.OnRoll;
                @RB.started += instance.OnRB;
                @RB.performed += instance.OnRB;
                @RB.canceled += instance.OnRB;
                @RT.started += instance.OnRT;
                @RT.performed += instance.OnRT;
                @RT.canceled += instance.OnRT;
            }
        }
    }
    public PlayerActionsActions @PlayerActions => new PlayerActionsActions(this);

    // UIActions
    private readonly InputActionMap m_UIActions;
    private IUIActionsActions m_UIActionsActionsCallbackInterface;
    private readonly InputAction m_UIActions_PlayerOverview;
    private readonly InputAction m_UIActions_HotbarInput;
    private readonly InputAction m_UIActions_EquipLeftHand;
    public struct UIActionsActions
    {
        private @PlayerControlls m_Wrapper;
        public UIActionsActions(@PlayerControlls wrapper) { m_Wrapper = wrapper; }
        public InputAction @PlayerOverview => m_Wrapper.m_UIActions_PlayerOverview;
        public InputAction @HotbarInput => m_Wrapper.m_UIActions_HotbarInput;
        public InputAction @EquipLeftHand => m_Wrapper.m_UIActions_EquipLeftHand;
        public InputActionMap Get() { return m_Wrapper.m_UIActions; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIActionsActions set) { return set.Get(); }
        public void SetCallbacks(IUIActionsActions instance)
        {
            if (m_Wrapper.m_UIActionsActionsCallbackInterface != null)
            {
                @PlayerOverview.started -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnPlayerOverview;
                @PlayerOverview.performed -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnPlayerOverview;
                @PlayerOverview.canceled -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnPlayerOverview;
                @HotbarInput.started -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnHotbarInput;
                @HotbarInput.performed -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnHotbarInput;
                @HotbarInput.canceled -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnHotbarInput;
                @EquipLeftHand.started -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnEquipLeftHand;
                @EquipLeftHand.performed -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnEquipLeftHand;
                @EquipLeftHand.canceled -= m_Wrapper.m_UIActionsActionsCallbackInterface.OnEquipLeftHand;
            }
            m_Wrapper.m_UIActionsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @PlayerOverview.started += instance.OnPlayerOverview;
                @PlayerOverview.performed += instance.OnPlayerOverview;
                @PlayerOverview.canceled += instance.OnPlayerOverview;
                @HotbarInput.started += instance.OnHotbarInput;
                @HotbarInput.performed += instance.OnHotbarInput;
                @HotbarInput.canceled += instance.OnHotbarInput;
                @EquipLeftHand.started += instance.OnEquipLeftHand;
                @EquipLeftHand.performed += instance.OnEquipLeftHand;
                @EquipLeftHand.canceled += instance.OnEquipLeftHand;
            }
        }
    }
    public UIActionsActions @UIActions => new UIActionsActions(this);
    public interface IPlayerSpaceMovementActions
    {
        void OnMovement(InputAction.CallbackContext context);
        void OnCamera(InputAction.CallbackContext context);
    }
    public interface IPlayerActionsActions
    {
        void OnRoll(InputAction.CallbackContext context);
        void OnRB(InputAction.CallbackContext context);
        void OnRT(InputAction.CallbackContext context);
    }
    public interface IUIActionsActions
    {
        void OnPlayerOverview(InputAction.CallbackContext context);
        void OnHotbarInput(InputAction.CallbackContext context);
        void OnEquipLeftHand(InputAction.CallbackContext context);
    }
}
