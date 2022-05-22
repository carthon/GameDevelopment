using UnityEngine;

namespace _Project.Scripts.Components {
    [CreateAssetMenu(menuName = "Items/Wereable Item")]
    public class Wereable : Item {
        private BodyPart bodyPart;
        public BodyPart GetBodyPart() => bodyPart;
    }
}