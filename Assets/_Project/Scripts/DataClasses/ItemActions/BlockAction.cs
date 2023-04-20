using System;

namespace _Project.Scripts.DataClasses.ItemActions {
    [Serializable]
    public class BlockAction : ItemAction {

        public override bool TryDoAction() {
            throw new System.NotImplementedException();
        }
        public override string AnimationName() => "Block";
    }
}