using System;
using _Project.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour {
    [SerializeField]
    protected Image icon;
    [SerializeField]
    protected Image borderImage;
    [SerializeField]
    protected TextMeshProUGUI _amount;
    [SerializeField]
    private Button removeButton;
    protected EventTrigger _eventTrigger;

    [SerializeField]
    protected ItemStack _itemStack;
    public ItemStack ItemStack => _itemStack;

    protected virtual void Start() {
        _eventTrigger = GetComponent<EventTrigger>();
        if (_eventTrigger == null)
            _eventTrigger = gameObject.AddComponent<EventTrigger>();
        RegisterEntries();
    }

    public event Action<ItemSlotUI> OnItemClicked,
        OnItemDroppedOn,
        OnItemBeginDrag,
        OnItemEndDrag,
        OnItemRbClick;

    private void RegisterEntries() {
        var entry = new EventTrigger.Entry {
            eventID = EventTriggerType.BeginDrag
        };
        entry.callback.AddListener(data => OnBeginDrag());
        _eventTrigger.triggers.Add(entry);

        entry = new EventTrigger.Entry {
            eventID = EventTriggerType.EndDrag
        };
        entry.callback.AddListener(data => OnEndDrag());
        _eventTrigger.triggers.Add(entry);

        entry = new EventTrigger.Entry {
            eventID = EventTriggerType.Drop
        };
        entry.callback.AddListener(data => OnDrop());
        _eventTrigger.triggers.Add(entry);

        entry = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerClick
        };
        entry.callback.AddListener(OnPointerClick);
        _eventTrigger.triggers.Add(entry);
    }
    /// <summary>
    ///     Sets the itemStack to a linked itemStack
    /// </summary>
    public void SetItemStack(ItemStack itemStack) {
        _itemStack = itemStack;
        _amount.SetText(itemStack.GetCount().ToString());
        if (itemStack.GetCount() > 1)
            _amount.gameObject.SetActive(true);
        if (!itemStack.IsEmpty()) {
            icon.sprite = itemStack.Item.itemIcon;
            icon.enabled = true;
            if (removeButton != null)
                removeButton.interactable = true;
        }
    }

    public void SetItemStackCount(int count) {
        _amount.SetText(count.ToString());
        if (count > 1)
            _amount.gameObject.SetActive(true);
        else
            _amount.gameObject.SetActive(false);
    }

    public void ClearSlot() {
        icon.enabled = false;
        _itemStack.Item = null;
        _itemStack.SetCount(0);
        _amount.gameObject.SetActive(false);
        icon.sprite = null;
        if (removeButton != null)
            removeButton.interactable = false;
    }

    public void OnRemoveButton() {
        var itemInv = _itemStack.GetInventory();
        UIHandler.Instance.UpdateVisuals = true;
        itemInv.DropItemInSlot(_itemStack.GetSlotID(), GodEntity.Instance.GetPlayer().transform.position + Vector3.up * 5f);
    }
    public virtual void OnBeginDrag() {
        InvokeEvent(OnItemBeginDrag, this);
    }
    public virtual void OnEndDrag() {
        InvokeEvent(OnItemEndDrag, this);
    }

    public virtual void OnDrop() {
        InvokeEvent(OnItemDroppedOn, this);
    }
    public virtual void OnPointerClick(BaseEventData data) {
        var pointerData = (PointerEventData) data;
        if (pointerData.button == PointerEventData.InputButton.Right)
            InvokeEvent(OnItemRbClick, this);
        else
            InvokeEvent(OnItemClicked, this);
    }
    private void InvokeEvent(Action<ItemSlotUI> action, ItemSlotUI slot) {
        if (slot.GetType() == typeof(HotbarSlotUI))
            action?.Invoke((HotbarSlotUI) slot);
        else
            action?.Invoke(slot);
    }
}