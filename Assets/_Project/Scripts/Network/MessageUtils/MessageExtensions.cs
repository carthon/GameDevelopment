using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Constants;
using _Project.Scripts.DataClasses;
using _Project.Scripts.DataClasses.ItemTypes;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageUtils {
    public static class MessageExtensions
    {
        #region Vector2 && Vector2Int
        /// <inheritdoc cref="AddVector2(Message, Vector2)"/>
        /// <remarks>This method is simply an alternative way of calling <see cref="AddVector2(Message, Vector2)"/>.</remarks>
        public static Message Add(this Message message, Vector2 value) => AddVector2(message, value);

        /// <summary>Adds a <see cref="Vector2"/> to the message.</summary>
        /// <param name="value">The <see cref="Vector2"/> to add.</param>
        /// <returns>The message that the <see cref="Vector2"/> was added to.</returns>
        public static Message AddVector2(this Message message, Vector2 value)
        {
            return message.AddFloat(value.x).AddFloat(value.y);
        }

        /// <summary>Retrieves a <see cref="Vector2"/> from the message.</summary>
        /// <returns>The <see cref="Vector2"/> that was retrieved.</returns>
        public static Vector2 GetVector2(this Message message)
        {
            return new Vector2(message.GetFloat(), message.GetFloat());
        }
        
        /// <inheritdoc cref="AddVector2(Message, Vector2)"/>
        /// <remarks>This method is simply an alternative way of calling <see cref="AddVector2(Message, Vector2)"/>.</remarks>
        public static Message Add(this Message message, Vector2Int value) => AddVector2Int(message, value);

        /// <summary>Adds a <see cref="Vector2"/> to the message.</summary>
        /// <param name="value">The <see cref="Vector2"/> to add.</param>
        /// <returns>The message that the <see cref="Vector2"/> was added to.</returns>
        public static Message AddVector2Int(this Message message, Vector2Int value)
        {
            return message.AddInt(value.x).AddInt(value.y);
        }

        /// <summary>Retrieves a <see cref="Vector2"/> from the message.</summary>
        /// <returns>The <see cref="Vector2"/> that was retrieved.</returns>
        public static Vector2Int GetVector2Int(this Message message)
        {
            return new Vector2Int(message.GetInt(), message.GetInt());
        }
        
        #endregion

        #region Vector3
        /// <inheritdoc cref="AddVector3(Message, Vector3)"/>
        /// <remarks>This method is simply an alternative way of calling <see cref="AddVector3(Message, Vector3)"/>.</remarks>
        public static Message Add(this Message message, Vector3 value) => AddVector3(message, value);
        
        /// <summary>Adds a <see cref="Vector3"/> to the message.</summary>
        /// <param name="value">The <see cref="Vector3"/> to add.</param>
        /// <returns>The message that the <see cref="Vector3"/> was added to.</returns>
        public static Message AddVector3(this Message message, Vector3 value)
        {
            return message.AddFloat(value.x).AddFloat(value.y).AddFloat(value.z);
        }

        /// <summary>Retrieves a <see cref="Vector3"/> from the message.</summary>
        /// <returns>The <see cref="Vector3"/> that was retrieved.</returns>
        public static Vector3 GetVector3(this Message message)
        {
            return new Vector3(message.GetFloat(), message.GetFloat(), message.GetFloat());
        }
        #endregion

        #region Quaternion
        /// <inheritdoc cref="AddQuaternion(Message, Quaternion)"/>
        /// <remarks>This method is simply an alternative way of calling <see cref="AddQuaternion(Message, Quaternion)"/>.</remarks>
        public static Message Add(this Message message, Quaternion value) => AddQuaternion(message, value);

        /// <summary>Adds a <see cref="Quaternion"/> to the message.</summary>
        /// <param name="value">The <see cref="Quaternion"/> to add.</param>
        /// <returns>The message that the <see cref="Quaternion"/> was added to.</returns>
        public static Message AddQuaternion(this Message message, Quaternion value)
        {
            return message.AddFloat(value.x).AddFloat(value.y).AddFloat(value.z).AddFloat(value.w);
        }

        /// <summary>Retrieves a <see cref="Quaternion"/> from the message.</summary>
        /// <returns>The <see cref="Quaternion"/> that was retrieved.</returns>
        public static Quaternion GetQuaternion(this Message message)
        {
            return new Quaternion(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
        }
        #endregion
        
        #region InventorySlot
        /// <inheritdoc cref="AddInventorySlot(Message, ItemStack)"/>
        /// <remarks>This method is simply an alternative way of calling <see cref="AddVector3(Message, Vector3)"/>.</remarks>
        public static Message Add(this Message message, InventorySlot value) => AddInventorySlot(message, value);

        /// <summary>Adds a <see cref="InventorySlot"/> to the message.</summary>
        /// <param name="value">The <see cref="InventorySlot"/> to add.</param>
        /// <returns>The message that the <see cref="InventorySlot"/> was added to.</returns>
        public static Message AddInventorySlot(this Message message, InventorySlot value)
        {
            return message.AddItemStack(value.ItemStack ?? ItemStack.EMPTY).AddBool(value.IsFlipped).AddBool(value.IsOrigin);
        }

        /// <summary>Retrieves a <see cref="ItemStack"/> from the message.</summary>
        /// <returns>The <see cref="ItemStack"/> that was retrieved.</returns>
        public static InventorySlot GetInventorySlot(this Message message) {
            ItemStack itemStack = message.GetItemStack();
            bool isFlipped = message.GetBool();
            bool isOrigin = message.GetBool();
            return !itemStack.Equals(ItemStack.EMPTY) ? new InventorySlot(itemStack, isFlipped, isOrigin) : new InventorySlot();
        }
        #endregion
        
        #region ItemStack
        /// <inheritdoc cref="AddItemStack(Message, ItemStack)"/>
        /// <remarks>This method is simply an alternative way of calling <see cref="AddVector3(Message, Vector3)"/>.</remarks>
        public static Message Add(this Message message, ItemStack value) => AddItemStack(message, value);

        /// <summary>Adds a <see cref="ItemStack"/> to the message.</summary>
        /// <param name="value">The <see cref="ItemStack"/> to add.</param>
        /// <returns>The message that the <see cref="ItemStack"/> was added to.</returns>
        public static Message AddItemStack(this Message message, ItemStack value)
        {
            return message.AddString(value.Item != null ? value.Item.id : string.Empty).AddInt(value.GetCount()).AddVector2Int(value.OriginalSlot);
        }

        /// <summary>Retrieves a <see cref="ItemStack"/> from the message.</summary>
        /// <returns>The <see cref="ItemStack"/> that was retrieved.</returns>
        public static ItemStack GetItemStack(this Message message) {
            string prefabId = message.GetString();
            if (prefabId != string.Empty)
                return new ItemStack(NetworkManager.Singleton.itemsDictionary[prefabId], 
                message.GetInt(), message.GetVector2Int());
            return new ItemStack(null, message.GetInt(), message.GetVector2Int());
        }
        #endregion
        
        #region Actions
        /// <inheritdoc cref="AddAction(Message, ItemStack)"/>
        /// <remarks>This method is simply an alternative way of calling <see cref="AddVector3(Message, Vector3)"/>.</remarks>
        public static Message Add(this Message message, Dictionary<ActionsEnum, Action> value) => AddActions(message, value);

        /// <summary>Adds a <see cref="Dictionary<ActionsEnum, Action>"/> to the message.</summary>
        /// <param name="value">The <see cref="Dictionary<ActionsEnum, Action>"/> to add.</param>
        /// <returns>The message that the <see cref="Dictionary<ActionsEnum, Action>"/> was added to.</returns>
        public static Message AddActions(this Message message, Dictionary<ActionsEnum, Action> value) {
            message.AddInt(value.Count);
            foreach (ActionsEnum key in value.Keys) {
                message.AddInt((int) key);
                message.AddBools(new[] {value[key].actionValue, value[key].isImportant});
            }
            return message;
        }

        /// <summary>Retrieves a <see cref="Dictionary<ActionsEnum, Action>"/> from the message.</summary>
        /// <returns>The <see cref="Dictionary<ActionsEnum, Action>"/> that was retrieved.</returns>
        public static Dictionary<ActionsEnum, Action> GetActions(this Message message) {
            int count = message.GetInt();
            Dictionary<ActionsEnum, Action> actions = new Dictionary<ActionsEnum, Action>();
            for (int i = 0; i < count; i++) {
                ActionsEnum key = (ActionsEnum) message.GetInt();
                bool[] actionComponent = message.GetBools();
                Action value = new Action(actionComponent[0], actionComponent[1]);
                actions.Add(key, value);
            }
            return actions;
        }
        #endregion
    }
}