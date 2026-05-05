using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Idle/Data/Skill XP Table")]
public class SkillXpTable : ScriptableObject
{
    [Tooltip("Niveau maximum atteignable (ex: 99 ou 100).")]
    public int maxLevel = 99;

    [Tooltip("XP totale requise pour atteindre le niveau L. Index 0 ignoré. Index 1 = 0 XP.")]
    public List<int> xpForLevel = new List<int>() { 0, 0 };

    public int GetXpForLevel(int level)
    {
        level = Mathf.Clamp(level, 1, maxLevel);
        if (xpForLevel == null || xpForLevel.Count <= level)
            return (level - 1) * 100; // fallback

        return xpForLevel[level];
    }

    public int GetMaxXp()
    {
        return GetXpForLevel(maxLevel);
    }
}