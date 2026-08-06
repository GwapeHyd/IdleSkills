using System.Collections.Generic;
using UnityEngine;

public class InventoryPanelUGUI : MonoBehaviour
{
    [Header("Data")]
    public GameDatabase db;

    [Header("Event sources (optional)")]
    public WoodcuttingActionRunner woodcutting;
    public MiningSystem mining;
    public SmithingActionRunner smithing;

    [Header("UI")]
    public Transform contentRoot;              // Content du ScrollView
    public InventoryItemRowUGUI rowPrefab;     // prefab de ligne

    // Cache pour éviter de ré-instancier tout le temps
    private readonly Dictionary<string, InventoryItemRowUGUI> _rowsByItemId = new();

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (woodcutting != null) woodcutting.OnStateChanged += Refresh;
        if (mining != null) mining.OnStateChanged += Refresh;
        if (smithing != null) smithing.OnStateChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (woodcutting != null) woodcutting.OnStateChanged -= Refresh;
        if (mining != null) mining.OnStateChanged -= Refresh;
        if (smithing != null) smithing.OnStateChanged -= Refresh;
    }

    public void Refresh()
    {
        if (contentRoot == null || rowPrefab == null) return;
        if (db == null) return;

        var gm = GameManager.Instance;
        if (gm == null || gm.State == null) return;

        var inventory = gm.State.inventory;
        if (inventory == null) return;

        foreach (var entry in inventory.entries)
        {
            if (string.IsNullOrWhiteSpace(entry.itemId)) continue;

            if (entry.amount <= 0) continue;

            if (!_rowsByItemId.TryGetValue(entry.itemId, out var row) || row == null)
            {
                row = Instantiate(rowPrefab, contentRoot);
                _rowsByItemId[entry.itemId] = row;
            }

            var def = db.GetItem(entry.itemId);
            var displayName = def != null ? def.displayName : entry.itemId;
            var icon = def != null ? def.icon : null;

            row.Bind(icon, displayName, entry.amount);
            row.gameObject.SetActive(true);
        }

        foreach (var kvp in _rowsByItemId)
        {
            var id = kvp.Key;
            var row = kvp.Value;
            if (row == null) continue;

            int amount = inventory.GetAmount(id);
            row.gameObject.SetActive(amount > 0);
        }
    }
}