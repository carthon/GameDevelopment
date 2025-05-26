using System;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Utils {
    public class GUIUtils {
        public static GameObject createDebugTextWithWorldCanvas(GameObject gameObject, Vector2 widthHeight, float textDepthPosition) {
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
            Handles.color = colour ?? Color.white;
            Handles.Label(worldPos, text);
#endif
        }
    }
    public class RuntimeEnumPopup<T> where T : Enum {
        private bool _showPopup = false;
        private Rect _buttonGuiRect;
        private Rect _windowRect;
        private readonly int _windowId = "EnumPopupWindow".GetHashCode();
        private readonly Action<T> _onSelect;
        private T _current;

        private const float EntryHeight = 20f;
        private const float WindowPadding = 8f; // margen interno vertical

        public RuntimeEnumPopup(T initialValue, Action<T> onSelect) {
            _current = initialValue;
            _onSelect = onSelect;
        }

        public void OnGUILayout(Rect areaRect) {
            // 1) Botón que abre la ventana
            if (GUILayout.Button(_current.ToString(), GUILayout.ExpandWidth(false))) {
                _showPopup = !_showPopup;

                // Captura dónde está el botón en coords GUI
                Rect localRect = GUILayoutUtility.GetLastRect();
                _buttonGuiRect = new Rect(
                    areaRect.x + localRect.x,
                    areaRect.y + localRect.y,
                    localRect.width,
                    localRect.height
                );

                // 2) Calcula altura dinámica de la ventana
                T[] values = (T[])Enum.GetValues(typeof(T));
                
                GUIStyle winStyle = GUI.skin.window;
                float windowVP = winStyle.padding.top + winStyle.padding.bottom;
                float contentH = values.Length * EntryHeight + WindowPadding * 2 + windowVP;
                float width = Math.Max(_buttonGuiRect.width, 100f);


                _windowRect = new Rect(
                    _buttonGuiRect.x,
                    _buttonGuiRect.y + _buttonGuiRect.height,
                    width,
                    contentH
                );
            }
            if (_showPopup)
                _windowRect = GUI.Window(_windowId, _windowRect, DrawWindow, GUIContent.none);
        }

        private void DrawWindow(int id) {
            GUILayout.BeginVertical();
            T[] values = (T[])Enum.GetValues(typeof(T));
            foreach (T val in values) {
                if (GUILayout.Button(val.ToString(), GUILayout.Height(EntryHeight))) {
                    _current = val;
                    _onSelect(val);
                    _showPopup = false;
                }
            }
            GUILayout.EndVertical();

            // evita que un click “atraviese” la ventana
            GUI.DragWindow(new Rect(0, 0, _windowRect.width, _windowRect.height));
        }
    }
}