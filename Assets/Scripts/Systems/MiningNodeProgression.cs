using UnityEngine;

public static class MiningNodeProgression
{
    public const int MaxLevel = 100;

    // XP requise pour passer du level L au L+1
    public static int XpToNext(int level)
    {
        level = Mathf.Clamp(level, 1, MaxLevel);
        // courbe simple qui augmente
        return 50 + level * 25; // ex: L1->2 = 75, L10->11 = 300 ...
    }

    public static void AddNodeXp(MiningNodeState node, int amount)
    {
        if (node == null || amount <= 0) return;
        if (node.level >= MaxLevel) return;

        node.xp += amount;

        // multi level-up si gros gain
        while (node.level < MaxLevel)
        {
            int need = XpToNext(node.level);
            if (node.xp < need) break;

            node.xp -= need;
            node.level += 1;

            // règle demandée: +1 maxOre par niveau
            node.maxOre += 1;

            // Option: si tu veux aussi remplir 1 ore lors du level up:
            // node.currentOre = Mathf.Min(node.currentOre + 1, node.maxOre);
        }
    }

    public static float GetNodeProgress01(MiningNodeState node)
    {
        if (node == null) return 0f;
        if (node.level >= MaxLevel) return 1f;
        int need = XpToNext(node.level);
        return need <= 0 ? 0f : Mathf.Clamp01((float)node.xp / need);
    }
}