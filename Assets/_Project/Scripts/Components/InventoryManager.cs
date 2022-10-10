using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RiptideNetworking;
using UnityEngine;

public class InventoryManager : MonoBehaviour {
	private List<Inventory> _inventories = new List<Inventory>();
	public List<Inventory> Inventories { get => _inventories; }
	private PlayerNetworkManager _player;
	public ItemStack AddItemStack(ItemStack itemStack) {
		foreach (Inventory inventory in _inventories) {
			ItemStack leftovers = inventory.AddItemStack(itemStack);
			if (!leftovers.Equals(ItemStack.EMPTY)) {
				return leftovers;
			}
		}
		return itemStack;
	}
	public void Add(Inventory inventory) {
		inventory.Id = inventory.Size;
		_inventories.Add(inventory);
		inventory.OnSlotChange += InventoryOnSlotChange;
		inventory.OnSlotSwap += InventoryOnSlotSwap;
	}
	public void Remove(Inventory inventory) {
		_inventories.Remove(inventory);
	}
	private void InventoryOnSlotChange(int slot, ItemStack itemStack) {
		if (NetworkManager.Singleton.IsServer) {
			Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.inventory);
		}
	}
	private void InventoryOnSlotSwap(int inventory, int slot, int otherSlot) {
		if (_player.IsLocal && !NetworkManager.Singleton.IsServer) {
			SendSlotSwapToServer(inventory, slot, otherSlot);
		}
	}
	private void SendSlotSwapToServer(int inventory, int slot, int otherSlot) {
		Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ClientToServerId.inventory);
		message.AddUShort(_player.Id);
		message.AddInt(inventory);
		message.AddInt(slot);
		message.AddInt(otherSlot);
		NetworkManager.Singleton.Client.Send(message);
	}
}