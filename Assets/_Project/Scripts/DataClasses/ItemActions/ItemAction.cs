using System;
using _Project.Scripts.Components;
using UnityEngine;

namespace _Project.Scripts.DataClasses.ItemActions {
    public class ItemAction : ScriptableObject{
        public virtual bool TryDoAction() => false;
        public virtual string AnimationName() => "Empty";
    }
}