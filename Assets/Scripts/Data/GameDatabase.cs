using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Idle/Data/Game Database")]
public class GameDatabase : ScriptableObject
{
    public List<ItemDefinition> items = new();
    public List<WoodcuttingActionDefinition> woodcuttingActions = new();
    public List<MiningNodeDefinition> miningNodes = new();
    public SmithingRecipeDefinition[] smithingRecipes;

    private Dictionary<string, ItemDefinition> _itemById;
    private Dictionary<string, WoodcuttingActionDefinition> _wcById;
    private Dictionary<string, MiningNodeDefinition> _mnById;
    private Dictionary<string, SmithingRecipeDefinition> _smithingById;

    public void BuildIndexes()
    {
        _itemById = new Dictionary<string, ItemDefinition>();
        foreach (var it in items)
            if (it && !string.IsNullOrWhiteSpace(it.id))
                _itemById[it.id] = it;

        _wcById = new Dictionary<string, WoodcuttingActionDefinition>();
        foreach (var a in woodcuttingActions)
            if (a && !string.IsNullOrWhiteSpace(a.id))
                _wcById[a.id] = a;

        _mnById = new Dictionary<string, MiningNodeDefinition>();
        foreach (var n in miningNodes)
            if (n && !string.IsNullOrWhiteSpace(n.id))
                _mnById[n.id] = n;

        _smithingById = new Dictionary<string, SmithingRecipeDefinition>();
        foreach (var r in smithingRecipes)
            if (r && !string.IsNullOrWhiteSpace(r.id))
                _smithingById[r.id] = r;
    }

    public ItemDefinition GetItem(string id)
        => (id != null && _itemById != null && _itemById.TryGetValue(id, out var it)) ? it : null;

    public WoodcuttingActionDefinition GetWoodcuttingAction(string id)
        => (id != null && _wcById != null && _wcById.TryGetValue(id, out var a)) ? a : null;

    public MiningNodeDefinition GetMiningNode(string id)
        => (id != null && _mnById != null && _mnById.TryGetValue(id, out var n)) ? n : null;

    public SmithingRecipeDefinition GetSmithingRecipe(string id)
        => (id != null && _smithingById != null && _smithingById.TryGetValue(id, out var r)) ? r : null;
}