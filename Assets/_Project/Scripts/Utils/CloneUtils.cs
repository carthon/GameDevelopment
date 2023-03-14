using System;
using System.Collections.Generic;

namespace _Project.Scripts.Utils {
    public class CloneUtils {
        public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
            (Dictionary<TKey, TValue> original) where TValue : ICloneable
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count,
                original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue) entry.Value.Clone());
            }
            return ret;
        }
    }
}