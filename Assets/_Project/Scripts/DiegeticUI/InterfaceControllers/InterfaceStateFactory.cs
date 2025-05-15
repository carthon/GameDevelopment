using _Project.Scripts.DataClasses;
using _Project.Scripts.DiegeticUI.InterfaceControllers.InventoryState;

namespace _Project.Scripts.DiegeticUI.InterfaceControllers {
    public class InterfaceStateFactory {
        private readonly ContainerRenderer _context;
        public InterfaceStateFactory(ContainerRenderer currentContext) {
            _context = currentContext;
        }
        public InterfaceAbstractBaseState DefaultState() => new DefaultInterfaceState(this, _context);
        public InterfaceAbstractBaseState InventoryState() => new InventoryInterfaceState(this, _context);
        public InventoryGrabItemInterfaceState InventoryGrabItemState(InventorySlot grabbedItem) => new InventoryGrabItemInterfaceState(grabbedItem, this, _context);
        public InventorySelectItemInterfaceState InventorySelectItemState() => new InventorySelectItemInterfaceState(this, _context);
    }
}