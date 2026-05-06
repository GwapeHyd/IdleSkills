using System;
using UnityEngine;

public class SmithingActionRunner : MonoBehaviour
{
    [Header("References")]
    public GameDatabase db;

    [Header("Offline")]
    public int offlineCapSeconds = 8 * 60 * 60;

    public PlayerState State { get; private set; }

    public SkillXpTable smithingXpTable;

    public event Action OnStateChanged;

    private bool _pendingSave;
    private float _saveTimer;
    private const float SaveInterval = 1f;

    private void Awake()
    {
        if (db == null)
        {
            Debug.LogError("[SmithingActionRunner] db is NULL", this);
            return;
        }

        db.BuildIndexes();

        State = GameManager.Instance.State;

        ApplyOfflineProgress();
        OnStateChanged?.Invoke();
    }

    private void Update()
    {
        if (State == null || db == null) return;

        if (string.IsNullOrEmpty(State.activeSmithingRecipeId))
            return;

        var recipe = db.GetSmithingRecipe(State.activeSmithingRecipeId);
        if (recipe == null)
        {
            StopAction();
            return;
        }

        // Si on n'a plus les ingrédients en cours de route => stop
        if (!CanPay(recipe))
        {
            StopAction();
            return;
        }

        State.activeSmithingProgress += Time.deltaTime;

        while (State.activeSmithingProgress >= recipe.craftDuration)
        {
            State.activeSmithingProgress -= recipe.craftDuration;

            // Re-check ingrédients à chaque craft (important)
            if (!CanPay(recipe))
            {
                StopAction();
                break;
            }

            CompleteOneCraft(recipe);
        }

        TickSave();
        OnStateChanged?.Invoke();
    }

    private void TickSave()
    {
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
    }

    public bool CanStart(SmithingRecipeDefinition recipe)
    {
        if (recipe == null) return false;
        if (State == null || State.smithing == null) return false;
        if (State.smithing.level < recipe.requiredSmithingLevel) return false;
        return CanPay(recipe);
    }

    public void StartAction(string recipeId)
    {
        var recipe = db.GetSmithingRecipe(recipeId);
        if (recipe == null) return;

        if (!CanStart(recipe))
        {
            Debug.Log($"Need Smithing level {recipe.requiredSmithingLevel} and ingredients for {recipe.displayName}");
            return;
        }

        State.activeSmithingRecipeId = recipe.id;
        State.activeSmithingProgress = 0f;

        OnStateChanged?.Invoke();
        SaveSystem.Save(State);
    }

    public void StopAction()
    {
        State.activeSmithingRecipeId = "";
        State.activeSmithingProgress = 0f;

        OnStateChanged?.Invoke();
        SaveSystem.Save(State);
    }

    private bool CanPay(SmithingRecipeDefinition recipe)
    {
        if (recipe.requires == null) return true;
        foreach (var req in recipe.requires)
        {
            if (req.amount <= 0) continue;
            if (State.inventory.GetAmount(req.itemId) < req.amount)
                return false;
        }
        return true;
    }

    private void Pay(SmithingRecipeDefinition recipe)
    {
        if (recipe.requires == null) return;
        foreach (var req in recipe.requires)
        {
            if (req.amount <= 0) continue;
            State.inventory.Remove(req.itemId, req.amount);
        }
    }

    private void CompleteOneCraft(SmithingRecipeDefinition recipe)
    {
        Pay(recipe);

        if (recipe.outputItem != null)
            State.inventory.Add(recipe.outputItem.id, recipe.outputAmount);

        SkillSystem.AddXp(State.smithing, recipe.xpPerCraft, smithingXpTable);

        _pendingSave = true;
        _saveTimer = 0f;
    }

    private void ApplyOfflineProgress()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var last = State.lastSeenUnixSeconds;
        if (last <= 0) { State.lastSeenUnixSeconds = now; return; }

        var delta = (int)Mathf.Max(0, now - last);
        delta = Mathf.Min(delta, offlineCapSeconds);

        if (!string.IsNullOrEmpty(State.activeSmithingRecipeId))
        {
            var recipe = db.GetSmithingRecipe(State.activeSmithingRecipeId);
            if (recipe != null && recipe.craftDuration > 0.01f)
            {
                float total = State.activeSmithingProgress + delta;
                int cycles = Mathf.FloorToInt(total / recipe.craftDuration);
                float remainder = total - cycles * recipe.craftDuration;

                // Craft offline tant qu'on a les ingrédients
                int crafted = 0;
                for (int i = 0; i < cycles; i++)
                {
                    if (!CanPay(recipe)) break;
                    CompleteOneCraft(recipe);
                    crafted++;
                }

                State.activeSmithingProgress = (!string.IsNullOrEmpty(State.activeSmithingRecipeId) && crafted > 0)
                    ? remainder
                    : remainder;

                // Si plus d'ingrédients, stop l'action
                if (!CanPay(recipe))
                {
                    State.activeSmithingRecipeId = "";
                    State.activeSmithingProgress = 0f;
                }
            }
        }

        State.lastSeenUnixSeconds = now;
        SaveSystem.Save(State);
        OnStateChanged?.Invoke();
    }
}