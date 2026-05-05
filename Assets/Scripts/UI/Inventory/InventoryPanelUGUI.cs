using System.Collections.Generic;
using UnityEngine;

public class InventoryPanelUGUI : MonoBehaviour
{
    public WoodcuttingActionRunner runner;

    [Header("UI")]
    public Transform contentRoot;              // Content du ScrollView
    public InventoryItemRowUGUI rowPrefab;     // prefab de ligne

    // Cache pour éviter de ré-instancier tout le temps
    private readonly Dictionary<string, InventoryItemRowUGUI> _rowsByItemId = new();

    private void OnEnable()
    {
        if (runner != null)
            runner.OnStateChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (runner != null)
            runner.OnStateChanged -= Refresh;
    }

    public void Refresh()
    {
        if (runner == null || runner.db == null || runner.State == null) return;
        if (contentRoot == null || rowPrefab == null) return;

        // Crée/maj rows existantes
        foreach (var entry in runner.State.inventory.entries)
        {
            if (string.IsNullOrWhiteSpace(entry.itemId)) continue;

            // Option: ne pas afficher les quantités 0
            if (entry.amount <= 0) continue;

            if (!_rowsByItemId.TryGetValue(entry.itemId, out var row) || row == null)
            {
                row = Instantiate(rowPrefab, contentRoot);
                _rowsByItemId[entry.itemId] = row;
            }

            var def = runner.db.GetItem(entry.itemId);
            var displayName = def != null ? def.displayName : entry.itemId;
            var icon = def != null ? def.icon : null;

            row.Bind(icon, displayName, entry.amount);
            row.gameObject.SetActive(true);
        }

        // Masque les rows d’items plus présents (ou 0)
        foreach (var kvp in _rowsByItemId)
        {
            var id = kvp.Key;
            var row = kvp.Value;
            if (row == null) continue;

            int amount = runner.State.inventory.GetAmount(id);
            if (amount <= 0)
                row.gameObject.SetActive(false);
        }
    }
}