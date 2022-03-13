using UnityEngine;
using UnityEngine.Internal;

namespace _Project.Scripts.Utils {
    public static class Logger {
        public static void DrawRay(Vector3 start, Vector3 dir, [DefaultValue("Color.white")] Color color,
            [DefaultValue("0.0f")] float duration, [DefaultValue("true")] bool depthTest) {
            Debug.DrawRay(start, dir, color, duration, depthTest);
        }
    }
}
