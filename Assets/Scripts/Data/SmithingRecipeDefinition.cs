using UnityEngine;
using System;

[Serializable]
public struct ItemAmount
{
    public string itemId;
    public int amount;
}

[CreateAssetMenu(menuName = "IdleSkills/Smithing/Recipe", fileName = "SmithingRecipe_")]
public class SmithingRecipeDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;                // ex: "smelt_copper_bar"
    public string displayName;       // ex: "Copper Bar"
    public Sprite icon;

    [Header("Requirements")]
    public int requiredSmithingLevel = 1;

    [Header("Timing")]
    public float craftDuration = 2f;

    [Header("Costs")]
    public ItemAmount[] requires;

    [Header("Output")]
    public ItemDefinition outputItem; // ton type existant
    public int outputAmount = 1;

    [Header("Rewards")]
    public int xpPerCraft = 5;
}
