using UnityEngine;

namespace _Project.Scripts.DataClasses.ItemActions {
    public interface IAction {
        public Transform ActionHandler { get; set; }
        public bool TryDoAction();
        string AnimationName();
    }
}