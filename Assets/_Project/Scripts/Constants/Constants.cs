using UnityEngine;

namespace _Project.Scripts.Constants {
    public static class Constants {
        //Tags
        public static readonly string TAG_UISLOT = "UISlot";
        public static readonly string TAG_UNTAGGED = "Untagged";
    
        public static readonly string DEFAULT_ITEM = "default";

        public static readonly int MAX_SERVER_INPUTS = 50;
    
        public static readonly int LAYER_LOCALPLAYER = LayerMask.NameToLayer("LocalPlayer");
        public static readonly int LAYER_REMOTEPLAYER = LayerMask.NameToLayer("RemotePlayer");
    }
}