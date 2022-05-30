using _Project.Scripts.Components.Items;
using _Project.Scripts.Handlers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.UI {
    public class UIHotbarSlot : UIItemSlot {
        private UIHotbarPanel parent;
        
        public override UIPanelsBase GetParent() => parent;
        public void SetParent(UIHotbarPanel panel) => parent = panel;
    }
}