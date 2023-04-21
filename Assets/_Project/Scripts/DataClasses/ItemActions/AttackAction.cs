using System;
using UnityEngine;

namespace _Project.Scripts.DataClasses.ItemActions {
    [Serializable]
    [CreateAssetMenu(menuName = "Data/Actions/AttackAction", fileName = "DefaultAttackAction")]
    public class AttackAction : ScriptableObject, IAction {

        public Transform ActionHandler { get; set; }
        public bool TryDoAction() {
            throw new System.NotImplementedException();
        }
        public string AnimationName() {
            throw new NotImplementedException();
        }
    }
}