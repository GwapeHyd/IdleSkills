using System;

[Serializable]
public class MiningNodeState
{
    public string nodeId;

    // réserve
    public int currentOre;
    public int maxOre;

    // progression du node
    public int level = 1;
    public int xp = 0;

    // regen
    public float regenTimer = 0f;
}