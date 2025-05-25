using System.Collections.Generic;
using _Project.Scripts.Components;
using UnityEngine;

namespace _Project.Scripts.Utils {
    public class GrabbableProxy {
        private static readonly Dictionary<Collider, Grabbable> _map = new Dictionary<Collider, Grabbable>();

        public static void Attach(Collider col, Grabbable grab) => _map.Add(col, grab);
        public static void Detach(Collider col) => _map.Remove(col);
        public static void TryGet(Collider col, out Grabbable grab) => _map.TryGetValue(col, out grab);
    }
}