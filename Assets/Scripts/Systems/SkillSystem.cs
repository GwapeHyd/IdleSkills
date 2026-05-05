using UnityEngine;

public static class SkillSystem
{
    public static void AddXp(SkillState skill, int amount, SkillXpTable table)
    {
        if (amount <= 0 || table == null) return;

        // cap xp à l'xp du max level
        int maxXp = table.GetMaxXp();
        skill.xp = Mathf.Min(skill.xp + amount, maxXp);

        skill.level = XpToLevel(skill.xp, table);
    }

    public static int XpToLevel(int xp, SkillXpTable table)
    {
        if (table == null) return 1;

        int maxLevel = Mathf.Max(1, table.maxLevel);

        // recherche simple (max 100 donc OK)
        int best = 1;
        for (int lvl = 1; lvl <= maxLevel; lvl++)
        {
            int need = table.GetXpForLevel(lvl);
            if (xp >= need) best = lvl;
            else break;
        }
        return best;
    }

    public static float GetProgress01(int xp, int level, SkillXpTable table)
    {
        if (table == null) return 0f;

        int maxLevel = Mathf.Max(1, table.maxLevel);
        level = Mathf.Clamp(level, 1, maxLevel);

        if (level >= maxLevel) return 1f;

        int start = table.GetXpForLevel(level);
        int next = table.GetXpForLevel(level + 1);

        if (next <= start) return 0f;
        return Mathf.InverseLerp(start, next, xp);
    }
}