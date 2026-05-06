using System;
using System.Collections.Generic;

[Serializable]
public class SkillState
{
    public int level = 1;
    public int xp = 0;
}

[Serializable]
public class PlayerState
{
    public InventoryState inventory = new();
    public SkillState woodcutting = new();
    public SkillState mining = new();
    public SkillState smithing = new();
    public List<MiningNodeState> miningNodes = new List<MiningNodeState>();

    public string activeActionId = "";      
    public float activeActionProgress = 0f; 
    public string activeMiningNodeId = "";
    public float activeMiningProgress = 0f;
    public string activeSmithingRecipeId = "";
    public float activeSmithingProgress = 0f;

    // offline
    public long lastSeenUnixSeconds = 0;
}