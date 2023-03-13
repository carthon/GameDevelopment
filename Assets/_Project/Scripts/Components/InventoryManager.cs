using System;
using System.Collections.Generic;
using _Project.Scripts.Network;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Client = _Project.Scripts.Network.Client.Client;
using Server = _Project.Scripts.Network.Server.Server;

namespace _Project.Scripts.Components {
	public class InventoryManager : MonoBehaviour {
		private List<Inventory> _inventories = new List<Inventory>();
		public List<Inventory> Inventories => _inventories;
		public Outline Outline { get; private set; }
		private Player _player;
		public Player Player { set => _player = value; }
		public ItemStack AddItemStack(ItemStack itemStack) {
			foreach (Inventory inventory in _inventories) {
				ItemStack leftovers = inventory.AddItemStack(itemStack);
				if (!leftovers.Equals(ItemStack.EMPTY)) {
					return leftovers;
				}
			}
			return itemStack;
		}
		public void Awake() {
			Outline = GetComponent<Outline>();
		}
		public void DropItemStack(int inventoryId, int slotId) {
			ItemStack droppedItemStack = Inventories[inventoryId].GetItemStack(slotId);
			droppedItemStack.SetCount(0);
			Transform playerTransform = _player.transform;
			Inventories[inventoryId].DropItemInSlot(slotId, playerTransform.position, playerTransform.rotation);
			InventoryOnSlotChange(slotId, droppedItemStack);
		}
		public void SetItemStackInInventory(ItemStack itemStack, int inventoryId) {
			if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
				_inventories[inventoryId].GetInventorySlots()[itemStack.GetSlotID()].SetStack(itemStack);
			}
		}
		public void Add(Inventory inventory) {
			inventory.Id = _inventories.Count;
			_inventories.Add(inventory);
			inventory.OnSlotChange += InventoryOnSlotChange;
			inventory.OnSlotSwap += SendSlotSwapToServer;
		}
		public void Remove(Inventory inventory) {
			inventory.OnSlotChange -= InventoryOnSlotChange;
			inventory.OnSlotSwap -= SendSlotSwapToServer;
			_inventories.Remove(inventory);
		}
		private void InventoryOnSlotChange(int slot, ItemStack itemStack) {
			if (NetworkManager.Singleton.IsServer && !_player.IsLocal) {
				Message message = Message.Create(MessageSendMode.reliable, Server.PacketHandler.clientInventoryChange);
				message.AddUShort(_player.Id);
				message.AddInt(itemStack.GetInventory().Id);
				message.AddItemStack(itemStack);
				NetworkManager.Singleton.Server.Send(message, _player.Id);
			}
		}
		private void SendSlotSwapToServer(int inventory, int otherInventory, int slot, int otherSlot) {
			if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer){
				int[] data = new[] {
					inventory,
					otherInventory,
					slot,
					otherSlot
				};
				Message message = Message.Create(MessageSendMode.reliable, (ushort)Client.PacketHandler.serverItemSwap);
				message.AddInts(data);
				NetworkManager.Singleton.Client.Send(message);
			}
		}
	}
}