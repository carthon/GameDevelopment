using System;
using UnityEngine;

namespace _Project.Scripts.DataClasses {
    [CreateAssetMenu(fileName = "LocomotionStat", menuName = "Data/Locomotion Stats")]
    [Serializable]
    public class LocomotionStats : ScriptableObject {
        public float height;
        public float runSpeed;
        public float crouchSpeed;
        public float backwardsMultSpeed;
        public float strafeMultSpeed;
        public float sprintSpeed;
        public float inAirSpeed;
        public float flySpeed;
        public float jumpStrength;
        public float groundedRayRadius;
        public bool canFly;
        public bool ignoreGround;
        public LayerMask groundLayer;
    }
}