using _Project.Scripts.Handlers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Utils {
    public class GUIUtils {
        public static GameObject createDebugTextWithWorldCanvas(GameObject gameObject, Vector2 widthHeight,float textDepthPosition) {
            GameObject child = new GameObject("DebugCanvas");
            child.transform.parent = gameObject.transform;
            TextMeshProUGUI text = child.AddComponent<TextMeshProUGUI>();
            text.fontSize = 0.5f;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            child.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            rectTransform.sizeDelta = widthHeight;
            rectTransform.transform.localPosition = new Vector3(0, 0, textDepthPosition);
            return child;
        }
        public static void DrawWorldText(string text, Vector3 worldPos, Color? colour = null) {
#if UNITY_EDITOR
            UnityEditor.Handles.color = colour ?? Color.white;
            UnityEditor.Handles.Label(worldPos, text);
#endif
        }
    }
}