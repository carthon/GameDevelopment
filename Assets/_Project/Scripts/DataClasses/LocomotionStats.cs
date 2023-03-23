using System;
using UnityEngine;

namespace _Project.Scripts.DataClasses {
    [CreateAssetMenu(fileName = "LocomotionStat", menuName = "Data/Locomotion Stats")]
    [Serializable]
    public class LocomotionStats : ScriptableObject {
        public float height;
        public float fallingSpeed;
        public float runSpeed;
        public float backwardsMultSpeed;
        public float strafeMultSpeed;
        public float sprintSpeed;
        public float inAirSpeed;
        public float jumpStrength;
        public LayerMask groundLayer;
    }
}