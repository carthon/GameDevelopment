using UnityEngine;

namespace Scripts.DataClasses {
    [CreateAssetMenu(menuName = "Data/CameraData", fileName = "Camera Data")]
    public class CameraData : ScriptableObject {
        public float playerLookInputLerpSpeed = 0.35f;
        public float rotationMultiplier;
        public float pitchLimitTopLimit = 90f;
        public float pitchLimitBottomLimit = 90f;
        public float sensitivityX;
        public float sensitivityY;
    }
}