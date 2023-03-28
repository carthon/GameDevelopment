using _Project.Scripts.DataClasses.ItemTypes;
using _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState;
using _Project.Scripts.Handlers;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers {
    public class InterfaceStateFactory {
        private readonly UIHandler _context;
        public InterfaceStateFactory(UIHandler currentContext) {
            _context = currentContext;
        }
        public InterfaceAbstractBaseState DefaultState() => new DefaultInterfaceState(this, _context);
        public InterfaceAbstractBaseState InventoryState() => new InventoryInterfaceState(this, _context);
        public InventoryGrabItemInterfaceState InventoryGrabItemState(ItemStack grabbedItem) => new InventoryGrabItemInterfaceState(grabbedItem, this, _context);
        public InventorySelectItemInterfaceState InventorySelectItemState() => new InventorySelectItemInterfaceState(this, _context);
    }
}