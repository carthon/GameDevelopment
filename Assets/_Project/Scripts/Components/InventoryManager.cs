using System;
using System.Collections.Generic;
using _Project.Libraries.QuickOutline.Scripts;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Entities;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;
using static _Project.Scripts.Network.PacketType;

namespace _Project.Scripts.Components {
	public class InventoryManager : MonoBehaviour {
		private List<Inventory> _inventories = new List<Inventory>();
		public List<Inventory> Inventories => _inventories;
		public Outline Outline { get; private set; }
		private IEntity _owner;
		public IEntity Owner { set => _owner = value; }
		public Action<int, InventorySlot> OnSlotChange;
		public Action<ItemStack, Vector3, Quaternion> OnItemDropped;
		public ItemStack AddItemStack(ItemStack itemStack) {
			ItemStack leftOvers = itemStack;
			foreach (Inventory inventory in _inventories) {
				leftOvers = inventory.AddItemStack(itemStack);
			}
			return leftOvers;
		}
		public void Awake() {
			Outline = GetComponent<Outline>();
		}
		public void DropItemStack(int inventoryId, Vector2Int slot, Vector3 position, Quaternion rotation) {
			ItemStack itemStack = Inventories[inventoryId].TakeItemStack(slot);
			if (itemStack.IsEmpty())
				return;
			OnItemDropped?.Invoke(itemStack, position, rotation);
		}
		public void Add(Inventory inventory) {
			inventory.Id = _inventories.Count;
			_inventories.Add(inventory);
			inventory.OnSlotChange += InventoryOnSlotChange;
		}
		public void Remove(Inventory inventory) {
			inventory.OnSlotChange -= InventoryOnSlotChange;
			_inventories.Remove(inventory);
		}
		public void SetInventorySlot(InventorySlot inventorySlot, int inventoryId) {
			_inventories[inventoryId].SetInventorySlot(inventorySlot);
			_inventories[inventoryId].UpdateItemDict();
		}
		private void InventoryOnSlotChange(InventorySlot inventorySlot) {
			OnSlotChange?.Invoke(inventorySlot.ItemStack.GetInventory().Id, inventorySlot);
		}
	}
}