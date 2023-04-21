using System;
using UnityEngine;

namespace _Project.Scripts.DataClasses.ItemActions {
    [Serializable]
    [CreateAssetMenu(menuName = "Data/Actions/MineAction", fileName = "DefaultMineAction")]
    public class ToolPart : ScriptableObject, IAction {

        public float toolDamage;

        public Transform ActionHandler { get; set; }
        public bool TryDoAction() {
            throw new System.NotImplementedException();
        }
        public string AnimationName() {
            throw new NotImplementedException();
        }
    }
}