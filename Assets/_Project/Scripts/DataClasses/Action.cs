namespace _Project.Scripts.DataClasses {
    public struct Action {
        public readonly bool actionValue;
        public readonly bool isImportant;
        
        public Action(bool actionValue, bool isImportant) {
            this.actionValue = actionValue;
            this.isImportant = isImportant;
        }
    }
}