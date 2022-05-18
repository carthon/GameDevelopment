using UnityEngine;

namespace _Project.Scripts.Components.Items {
    [CreateAssetMenu(menuName = "Items/Weapon Item")]
    public class WeaponItem : Item {
        public bool isUnarmed;
        public string OH_Light_Attack_1;
        public string OH_Heavy_Attack_1;
    }
}