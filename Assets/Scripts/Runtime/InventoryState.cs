using System;
using System.Collections.Generic;

[Serializable]
public class InventoryEntry
{
    public string itemId;
    public int amount;
}

[Serializable]
public class InventoryState
{
    public List<InventoryEntry> entries = new();

    public int GetAmount(string itemId)
    {
        var e = entries.Find(x => x.itemId == itemId);
        return e != null ? e.amount : 0;
    }

    public void Add(string itemId, int amount)
    {
        if (amount <= 0) return;
        var e = entries.Find(x => x.itemId == itemId);
        if (e == null)
        {
            entries.Add(new InventoryEntry { itemId = itemId, amount = amount });
        }
        else
        {
            e.amount += amount;
        }
    }

    public bool Remove(string itemId, int amount)
    {
        if (amount <= 0) return true;
        var e = entries.Find(x => x.itemId == itemId);
        if (e == null || e.amount < amount) return false;
        e.amount -= amount;
        if (e.amount <= 0) entries.Remove(e);
        return true;
    }
}