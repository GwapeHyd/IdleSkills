using UnityEngine;

public enum GlobalActionKind { None, Woodcutting, Mining }

public class ActionCoordinator : MonoBehaviour
{
    public WoodcuttingActionRunner woodcutting;
    public MiningSystem mining;

    public GlobalActionKind ActiveKind
    {
        get
        {
            if (woodcutting != null && woodcutting.State != null && !string.IsNullOrEmpty(woodcutting.State.activeActionId))
                return GlobalActionKind.Woodcutting;
            if (mining != null && mining.State != null && !string.IsNullOrEmpty(mining.State.activeMiningNodeId))
                return GlobalActionKind.Mining;
            return GlobalActionKind.None;
        }
    }

    public string ActiveId
    {
        get
        {
            if (ActiveKind == GlobalActionKind.Woodcutting) return woodcutting.State.activeActionId;
            if (ActiveKind == GlobalActionKind.Mining) return mining.State.activeMiningNodeId;
            return "";
        }
    }

    public void StopAll()
    {
        if (woodcutting != null) woodcutting.StopAction();
        if (mining != null) mining.StopMining();
    }

    public void StartWoodcutting(string actionId)
    {
        StopAll();
        if (woodcutting != null) woodcutting.StartAction(actionId);
    }

    public void StartMining(string nodeId)
    {
        StopAll();
        if (mining != null) mining.StartMining(nodeId);
    }
}