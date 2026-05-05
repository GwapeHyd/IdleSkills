using System;
using UnityEngine;

public class WoodcuttingActionRunner : MonoBehaviour
{
    [Header("References")]
    public GameDatabase db;

    [Header("Offline")]
    public int offlineCapSeconds = 8 * 60 * 60; // 8h

    public PlayerState State { get; private set; } = new PlayerState();

    public SkillXpTable woodcuttingXpTable;

    public event Action OnStateChanged;

    private bool _pendingSave;
    private float _saveTimer;
    private const float SaveInterval = 1f;

    private void Awake()
    {
        if (db == null)
        {
            Debug.LogError("[WoodcuttingActionRunner] db is NULL", this);
            return;
        }

        db.BuildIndexes();

        State = SaveSystem.LoadOrCreate();
        ApplyOfflineProgress();

        OnStateChanged?.Invoke();
    }

    private void Update()
    {
        // si aucune action
        if (string.IsNullOrEmpty(State.activeActionId))
            return;

        var action = db.GetWoodcuttingAction(State.activeActionId);
        if (action == null)
        {
            StopAction();
            return;
        }

        State.activeActionProgress += Time.deltaTime;

        while (State.activeActionProgress >= action.actionDuration)
        {
            State.activeActionProgress -= action.actionDuration;
            CompleteOneAction(action);
        }

        if (_pendingSave)
        {
            _saveTimer += Time.deltaTime;
            if (_saveTimer >= SaveInterval)
            {
                _saveTimer = 0f;
                _pendingSave = false;
                SaveSystem.Save(State);
            }
        }

        OnStateChanged?.Invoke();
    }

    public bool CanStart(WoodcuttingActionDefinition action)
    {
        if (action == null) return false;
        return State != null && State.woodcutting != null && State.woodcutting.level >= action.requiredLevel;
    }

    public void StartAction(string actionId)
    {
        var action = db.GetWoodcuttingAction(actionId);
        if (action == null) return;

        if (!CanStart(action))
        {
            Debug.Log($"Need Woodcutting level {action.requiredLevel} for {action.displayName}");
            return;
        }

        State.activeActionId = actionId;
        State.activeActionProgress = 0f;

        OnStateChanged?.Invoke();
        SaveSystem.Save(State);
    }

    public void StopAction()
    {
        State.activeActionId = "";
        State.activeActionProgress = 0f;

        OnStateChanged?.Invoke();
        SaveSystem.Save(State);
    }

    private void CompleteOneAction(WoodcuttingActionDefinition action)
    {
        // loot
        State.inventory.Add(action.outputItem.id, action.outputAmount);
        // xp
        SkillSystem.AddXp(State.woodcutting, action.xpPerAction, woodcuttingXpTable);

        if (!_pendingSave)
        {
            _pendingSave = true;
            _saveTimer = 0f;
        }
    }

    private void ApplyOfflineProgress()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var last = State.lastSeenUnixSeconds;
        if (last <= 0) { State.lastSeenUnixSeconds = now; return; }

        var delta = (int)Mathf.Max(0, now - last);
        delta = Mathf.Min(delta, offlineCapSeconds);

        if (!string.IsNullOrEmpty(State.activeActionId))
        {
            var action = db.GetWoodcuttingAction(State.activeActionId);
            if (action != null && action.actionDuration > 0.01f)
            {
                // On prend en compte la progress en cours
                var totalSeconds = State.activeActionProgress + delta;
                var cycles = Mathf.FloorToInt(totalSeconds / action.actionDuration);
                var remainder = totalSeconds - cycles * action.actionDuration;

                for (int i = 0; i < cycles; i++)
                    CompleteOneAction(action);

                State.activeActionProgress = remainder;
            }
        }

        State.lastSeenUnixSeconds = now;
        SaveSystem.Save(State);
        OnStateChanged?.Invoke();
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Save(State);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveSystem.Save(State);
    }
}