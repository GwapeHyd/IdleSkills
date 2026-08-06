using UnityEngine;

public static class TreeProgression
{
    public const int MaxLevel = 100;

    // XP requise pour passer du level L au L+1
    public static int XpToNext(int level)
    {
        level = Mathf.Clamp(level, 0, MaxLevel);
        // courbe simple qui augmente
        return (level == 0) ? 5 : 10 + level * 5 + level / 10; // ex: L0->1 = 5, L1->2 = 15, L10->11 = 150 ...
    }

    public static void AddTreeXp(TreeState tree, int amount)
    {
        if (tree == null || amount <= 0) return;
        if (tree.level >= MaxLevel) return;

        tree.xp += amount;

        // multi level-up si gros gain
        while (tree.level < MaxLevel)
        {
            int need = XpToNext(tree.level);
            if (tree.xp < need) break;

            tree.xp -= need;
            tree.level += 1;
        }
    }

    public static float GetTreeProgress01(TreeState tree)
    {
        if (tree == null) return 0f;
        if (tree.level >= MaxLevel) return 1f;
        int need = XpToNext(tree.level);
        return need <= 0 ? 0f : Mathf.Clamp01((float)tree.xp / need);
    }
}
