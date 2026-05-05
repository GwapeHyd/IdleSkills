using System;
using UnityEngine;

public class MiningSystem : MonoBehaviour
{
    public GameDatabase db;

    [Header("Offline")]
    public int offlineCapSeconds = 8 * 60 * 60;

    public PlayerState State { get; private set; } = new PlayerState();
    public event Action OnStateChanged;

    private bool _dirty;

    private void Awake()
    {
        if (db == null) { Debug.LogError("[MiningSystem] db is NULL", this); return; }
        db.BuildIndexes();

        State = SaveSystem.LoadOrCreate();

        EnsureMiningNodesInitialized();
        ApplyOfflineProgress();

        SaveSystem.Save(State);

        MarkDirty();
        FlushDirty();
    }

    private void Update()
    {
        if (db == null || State == null) return;

        // Regen online (peut rendre dirty)
        TickRegen(Time.deltaTime);

        // Action online (peut rendre dirty)
        TickMiningAction(Time.deltaTime);

        FlushDirty();
    }

    private void MarkDirty() => _dirty = true;

    private void FlushDirty()
    {
        if (!_dirty) return;
        _dirty = false;
        OnStateChanged?.Invoke();
    }

    private void TickMiningAction(float dt)
    {
        if (string.IsNullOrEmpty(State.activeMiningNodeId))
            return;

        var def = db.GetMiningNode(State.activeMiningNodeId);
        if (def == null) { StopMining(); return; }

        var node = GetNodeState(def.id);
        if (node == null) { StopMining(); return; }

        // Node vide => action reste "en cours" mais n'avance pas
        if (node.currentOre <= 0)
        {
            // IMPORTANT: ne pas accumuler du temps, sinon "passe" des actions dès que ça regen.
            if (State.activeMiningProgress != 0f)
            {
                State.activeMiningProgress = 0f;
                MarkDirty();
            }
            return;
        }

        State.activeMiningProgress += dt;

        // Pas dirty tant qu'on n'a pas complété un cycle
        while (State.activeMiningProgress >= def.actionDuration)
        {
            State.activeMiningProgress -= def.actionDuration;
            MineOne(def, node);   // <-- loot + XP ici
            MarkDirty();

            if (node.currentOre <= 0)
            {
                State.activeMiningProgress = 0f;
                break;
            }
        }
    }

    private void TickRegen(float dt)
    {
        foreach (var node in State.miningNodes)
        {
            var def = db.GetMiningNode(node.nodeId);
            if (def == null) continue;

            if (node.currentOre >= node.maxOre)
            {
                node.regenTimer = 0f;
                continue;
            }

            node.regenTimer += dt;
            float interval = Mathf.Max(0.01f, def.regenIntervalSec);

            bool changed = false;
            while (node.regenTimer >= interval && node.currentOre < node.maxOre)
            {
                node.regenTimer -= interval;
                node.currentOre += 1;
                changed = true;
            }

            if (changed)
                MarkDirty();
        }
    }

    private void MineOne(MiningNodeDefinition def, MiningNodeState node)
    {
        if (node.currentOre <= 0) return;

        node.currentOre -= 1;

        if (def.oreItem != null)
            State.inventory.Add(def.oreItem.id, def.oreAmount);

        // XP joueur mining (temporaire si tu n'as pas encore SkillXpTable)
        State.mining.xp += def.playerXpPerAction;
        State.mining.level = Mathf.Max(1, 1 + State.mining.xp / 100);

        // XP node + level up => +1 maxOre
        MiningNodeProgression.AddNodeXp(node, 1);
    }

    public void StartMining(string nodeId)
    {
        var def = db.GetMiningNode(nodeId);
        if (def == null) return;

        if (State.mining.level < def.requiredMiningLevel)
            return;

        State.activeMiningNodeId = nodeId;
        State.activeMiningProgress = 0f;

        MarkDirty();
        FlushDirty();
        SaveSystem.Save(State);
    }

    public void StopMining()
    {
        if (string.IsNullOrEmpty(State.activeMiningNodeId) && State.activeMiningProgress == 0f)
            return;

        State.activeMiningNodeId = "";
        State.activeMiningProgress = 0f;

        MarkDirty();
        FlushDirty();
        SaveSystem.Save(State);
    }

    private void EnsureMiningNodesInitialized()
    {
        if (State.miningNodes == null)
            State.miningNodes = new System.Collections.Generic.List<MiningNodeState>();

        foreach (var def in db.miningNodes)
        {
            if (def == null || string.IsNullOrWhiteSpace(def.id))
                continue;

            var existing = State.miningNodes.Find(n => n.nodeId == def.id);
            if (existing != null)
                continue;

            var ns = new MiningNodeState
            {
                nodeId = def.id,
                level = 1,
                xp = 0,
                maxOre = Mathf.Max(1, def.baseMaxOre),
                currentOre = Mathf.Max(0, def.baseMaxOre),
                regenTimer = 0f
            };

            State.miningNodes.Add(ns);
        }
    }

    private MiningNodeState GetNodeState(string nodeId) => State.miningNodes.Find(n => n.nodeId == nodeId);

    private void ApplyOfflineProgress()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long last = State.lastSeenUnixSeconds;
        if (last <= 0) { State.lastSeenUnixSeconds = now; return; }

        int delta = Mathf.Min((int)Mathf.Max(0, now - last), offlineCapSeconds);

        // OFFLINE REGEN
        foreach (var node in State.miningNodes)
        {
            var def = db.GetMiningNode(node.nodeId);
            if (def == null) continue;

            if (node.currentOre >= node.maxOre)
                continue;

            float interval = Mathf.Max(0.01f, def.regenIntervalSec);

            // total time available for regen includes leftover timer
            float total = node.regenTimer + delta;
            int steps = Mathf.FloorToInt(total / interval);

            node.regenTimer = total - steps * interval;

            if (steps > 0)
                node.currentOre = Mathf.Min(node.maxOre, node.currentOre + steps);
        }

        // OFFLINE MINING (consume ore that existed after regen)
        if (!string.IsNullOrEmpty(State.activeMiningNodeId))
        {
            var def = db.GetMiningNode(State.activeMiningNodeId);
            var node = def != null ? GetNodeState(def.id) : null;

            if (def != null && node != null && def.actionDuration > 0.01f)
            {
                float total = State.activeMiningProgress + delta;
                int cycles = Mathf.FloorToInt(total / def.actionDuration);
                float remainder = total - cycles * def.actionDuration;

                int possible = Mathf.Min(cycles, node.currentOre);
                for (int i = 0; i < possible; i++)
                    MineOne(def, node);

                // If we ran out of ore offline, keep it "active" but no progress
                State.activeMiningProgress = (node.currentOre > 0) ? remainder : 0f;
            }
        }

        State.lastSeenUnixSeconds = now;
    }

    private void OnApplicationQuit() => SaveSystem.Save(State);

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveSystem.Save(State);
    }
}
