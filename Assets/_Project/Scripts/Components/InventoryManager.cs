using System;
using System.Collections.Generic;
using _Project.Libraries.QuickOutline.Scripts;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.Network;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;
using Client = _Project.Scripts.Network.Client.Client;
using Logger = _Project.Scripts.Utils.Logger;
using Server = _Project.Scripts.Network.Server.Server;

namespace _Project.Scripts.Components {
	public class InventoryManager : MonoBehaviour {
		private List<Inventory> _inventories = new List<Inventory>();
		public List<Inventory> Inventories => _inventories;
		public Outline Outline { get; private set; }
		private Player _player;
		public Player Player { set => _player = value; }
		public Action<int, InventorySlot> OnInventoryManagerSlotChange;
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
		public void DropItemStack(int inventoryId, Vector2Int slot) {
			Transform player = _player.transform;
			Vector3 upDirection = player.up.normalized;
			DropItemStack(inventoryId, slot, player.position + upDirection * 2f, player.rotation);
		}
		public void DropItemStack(int inventoryId, Vector2Int slot, Vector3 position, Quaternion rotation) {
			Inventories[inventoryId].DropItemInSlot(slot, position, rotation);
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
		}
		private void InventoryOnSlotChange(InventorySlot inventorySlot) {
			OnInventoryManagerSlotChange?.Invoke(inventorySlot.ItemStack.GetInventory().Id, inventorySlot);
			if (!NetworkManager.Singleton.IsServer || _player.IsLocal)
				return;
			Message message = Message.Create(MessageSendMode.reliable, Server.PacketHandler.clientInventoryChange);
			message.AddUShort(_player.Id);
			message.AddInt(inventorySlot.ItemStack.GetInventory().Id);
			message.AddInventorySlot(inventorySlot);
			NetworkManager.Singleton.Server.Send(message, _player.Id);
		}
		//TODO: Posiblemente borrar porque no hace falta
		private void SendSlotSwapToServer(int inventory, int otherInventory, Vector2Int slot, Vector2Int otherSlot, bool wasFlipped) {
			if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer){
				int[] data = new[] {
					inventory,
					otherInventory,
				};
				Message message = Message.Create(MessageSendMode.reliable, (ushort)Client.PacketHandler.serverItemSwap);
				message.AddInts(data);
				message.AddVector2Int(slot);
				message.AddVector2Int(otherSlot);
				message.AddBool(wasFlipped);
				NetworkManager.Singleton.Client.Send(message);
			}
		}
	}
}