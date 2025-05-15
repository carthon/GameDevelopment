using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Utils {
    public class GameObjectUtils {
        public static void SetLayerRecursively(GameObject obj, int layer, string tag = null) {
            if (obj is null)
                return;
            obj.layer = layer;
            if (tag is not null)
                obj.tag = tag;
            Transform objTransform = obj.transform;
            for (int i = 0; i < obj.transform.childCount; i++) {
                Transform child = objTransform.GetChild(i);
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}