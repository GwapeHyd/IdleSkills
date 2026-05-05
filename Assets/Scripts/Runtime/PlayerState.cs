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
    public List<MiningNodeState> miningNodes = new List<MiningNodeState>();

    // action en cours
    public string activeActionId = "";      // ex: "chop_tree" ou ""
    public float activeActionProgress = 0f; // 0..duration
    public string activeMiningNodeId = "";
    public float activeMiningProgress = 0f;

    // offline
    public long lastSeenUnixSeconds = 0;
}