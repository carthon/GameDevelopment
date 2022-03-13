using UnityEngine;

namespace _Project.Scripts.Utils {
    public static class Utilities {
        public static void RotateAround(Transform target, Vector3 pivotPoint, float angle) {
            target.position = angle * (target.position - pivotPoint) + pivotPoint;
        }
    }
}
