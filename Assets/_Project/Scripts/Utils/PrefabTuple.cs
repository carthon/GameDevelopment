using System;
using UnityEngine;

namespace _Project.Scripts.Utils {
    [Serializable]
    public struct PrefabTuple {
        public ushort id;
        public GameObject model;
        public Item item;
    }
}