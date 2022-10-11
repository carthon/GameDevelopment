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
		inventory.OnSlotSwap += SwapSlot;
	}
	public void Remove(Inventory inventory) {
		_inventories.Remove(inventory);
	}
	private void InventoryOnSlotChange(int slot, ItemStack itemStack) {
		if (NetworkManager.Singleton.IsServer) {
			//Message message = Message.Create(MessageSendMode.reliable, NetworkManager.ServerToClientId.inventory);
		}
	}
	private void SwapSlot(int inventory, int otherInventory, int slot, int otherSlot) {
		if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
			SendSlotSwapToServer(inventory, otherInventory, slot, otherSlot);
	}
	private void SendSlotSwapToServer(int inventory, int otherInventory, int slot, int otherSlot) {
		Message message = Message.Create(MessageSendMode.reliable, (ushort)NetworkManager.ClientToServerId.itemSwap);
		message.AddUShort(_player.Id);
		int[] data = new[] {
			inventory,
			otherInventory,
			slot,
			otherSlot
		};
		message.AddInts(data);
		NetworkManager.Singleton.Client.Send(message);
	}
}