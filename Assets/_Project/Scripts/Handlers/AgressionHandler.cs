using System;
using _Project.Scripts.Components.Items;
using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class AgressionHandler : MonoBehaviour {
        private AnimatorHandler animatorHandler;

        public void Awake() {
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
        }

        public void HandleHeavyAttack(WeaponItem weapon) {
            animatorHandler.PlayTargetAnimation(weapon.OH_Heavy_Attack_1, true);
        }
        public void HandleLightAttack(WeaponItem weapon) {
            animatorHandler.PlayTargetAnimation(weapon.OH_Light_Attack_1, true);
        }
    }
}