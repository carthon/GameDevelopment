using UnityEngine;
using UnityEngine.InputSystem;

public class DragItemHandlerUI : MonoBehaviour {
    public ItemSlotUI itemDragging;
    [SerializeField]
    private Canvas canvas;
    private bool _active;

    private Transform _startParent;
    private Vector3 _startPosition;
    private Camera mainCamera;

    public void Awake() {
        itemDragging = GetComponentInChildren<ItemSlotUI>();
        canvas = transform.GetComponentInParent<Canvas>();
        mainCamera = Camera.main;
    }
    public void Start() {
        UIHandler.Instance.dragItemHandlerUI = this;
        gameObject.SetActive(false);
    }

    private void Update() {
        PositionOnMouse();
    }

    public void Toggle(bool val) {
        PositionOnMouse();
        gameObject.SetActive(val);
        _active = val;
    }

    public void PositionOnMouse() {
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform) canvas.transform,
            Mouse.current.position.ReadValue(), null, out var position);
        transform.position = canvas.transform.TransformPoint(position);
    }

    public void SetData(ItemStack itemStack) {
        itemDragging.SetItemStack(itemStack);
    }

    public void ResetData() {
        itemDragging.ClearSlot();
    }

    public bool IsActive() { return _active; }
}