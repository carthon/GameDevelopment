using System;
using UnityEngine;

[Serializable]
public class LootTableData {
    [SerializeField]
    private Item item;
    [SerializeField]
    private int count;
    public LootTableData(Item item, int count) {
        Item = item;
        Count = count;
    }
    public Item Item
    {
        get => item;
        set => item = value;
    }
    public int Count
    {
        get => count;
        set => count = value;
    }
}