using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public class NetworkMessage {
        private readonly MessageSendMode _messageSendMode;
        private Message _message;
        private readonly ushort _messageId;
        private readonly IGenericMessageStruct _messageData;

        public NetworkMessage(MessageSendMode messageSendMode, ushort messageId, IGenericMessageStruct messageData) {
            _messageSendMode = messageSendMode;
            _messageId = messageId;
            _messageData = messageData;
        }
        public void Send(bool isClient, ushort clientId = 0) {
            AddData();
            if (isClient)
                NetworkManager.Singleton.Client.Send(_message);
            else if (clientId > 0)
                NetworkManager.Singleton.Server.Send(_message, clientId);
            else
                NetworkManager.Singleton.Server.SendToAll(_message);
        }
        private void AddData(IGenericMessageStruct messageData) {
            FieldInfo[] fields = messageData.GetType().GetFields();
            foreach (var field in fields) {
                var data = field.GetValue(messageData);
                switch (data) {
                    case int integer:
                        _message.AddInt(integer);
                        break;
                    case int[] integers:
                        _message.AddInts(integers);
                        break;
                    case bool boolean:
                        _message.AddBool(boolean);
                        break;
                    case bool[] booleans:
                        _message.AddBools(booleans);
                        break;
                    case ushort number:
                        _message.AddUShort(number);
                        break;
                    case Quaternion quaternion:
                        _message.AddQuaternion(quaternion);
                        break;
                    case Vector3 vector3:
                        _message.AddVector3(vector3);
                        break;
                    case ItemStack itemStack:
                        _message.AddItemStack(itemStack);
                        break;
                    case string stringed:
                        _message.AddString(stringed);
                        break;
                    case IList list:
                        foreach (var t in list)
                            AddData((IGenericMessageStruct) t);
                        break;
                }
            }
        }
        private void AddData() {
            _message = Message.Create(_messageSendMode, _messageId);
            AddData(_messageData);
        }
    }
}