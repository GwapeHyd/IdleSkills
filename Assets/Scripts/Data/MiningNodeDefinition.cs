using UnityEngine;

[CreateAssetMenu(menuName = "Idle/Data/Mining Node")]
public class MiningNodeDefinition : ScriptableObject
{
    public string id;                  // "copper"
    public string displayName;         // "Copper"
    public Sprite icon;

    [Header("Gameplay")]
    public int requiredMiningLevel = 1;

    [Tooltip("Durée d'une action (en secondes) pour miner 1 minerai.")]
    public float actionDuration = 2f;

    [Tooltip("XP Mining joueur gagnée par action.")]
    public int playerXpPerAction = 10;

    [Header("Ore Reserve")]
    public int baseMaxOre = 6;         // ex: 6
    public float regenIntervalSec = 10f; // ex: +1 / 10 sec

    [Header("Loot")]
    public ItemDefinition oreItem;     // ex: item "ore_copper"
    public int oreAmount = 1;          // ex: +1 ore
}