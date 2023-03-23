using System;
using UnityEngine;

//Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: Sebastian Lague

namespace _Project.Scripts.WorldGen.Helper {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
        AttributeTargets.Class | AttributeTargets.Struct)]
    public class ConditionalHideAttribute : PropertyAttribute {
        public string conditionalSourceField;
        public int enumIndex;
        public bool showIfTrue;

        public ConditionalHideAttribute(string boolVariableName, bool showIfTrue) {
            conditionalSourceField = boolVariableName;
            this.showIfTrue = showIfTrue;
        }

        public ConditionalHideAttribute(string enumVariableName, int enumIndex) {
            conditionalSourceField = enumVariableName;
            this.enumIndex = enumIndex;
        }
    }
}