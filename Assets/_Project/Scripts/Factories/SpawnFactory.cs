using _Project.Scripts.Components;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using UnityEngine;

namespace _Project.Scripts.Factories {
    public static class SpawnFactory {
        public static Player CreatePlayerInstance(GameObject prefab, ushort id, string username, Vector3 position) {
            GameObject gameObject = Object.Instantiate(prefab, position, Quaternion.identity);
            Player player = gameObject.GetComponent<Player>();
            player.name = $"Player {id} {(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            player.Id = id;
            player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
            player.OnSpawn();
            return player;
        }
        public static Grabbable CreateGrabbableInstance(ushort id, ItemStack itemStack, Vector3 position, Quaternion rotation) {
            GameObject gameObject = Object.Instantiate(itemStack.Item.itemPrefab, position, rotation);
            gameObject.layer = LayerMask.NameToLayer("Item");
            Grabbable grabbable = gameObject.GetComponent<Grabbable>();
            var rb = gameObject.GetComponent<Rigidbody>();
            if (rb is null) rb = gameObject.AddComponent<Rigidbody>();
            if (grabbable is null) grabbable = gameObject.AddComponent<Grabbable>();
            if (grabbable && rb) {
                grabbable.SetItemStack(itemStack.GetCopy());
                grabbable.Initialize(id, rb, itemStack.Item);
                id++;
                Grabbable.nextId = id;
            }
            return grabbable;
        }
        public static Grabbable CreateGrabbableInstance(ItemStack itemStack, Vector3 position, Quaternion rotation) => 
            CreateGrabbableInstance(Grabbable.nextId, itemStack, position, rotation); 
    }
}