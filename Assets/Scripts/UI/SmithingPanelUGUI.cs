using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmithingPanelUGUI : MonoBehaviour
{
    [Header("Systems")]
    public SmithingActionRunner runner;

    public string defaultRecipeId = "smelt_copper_bar";

    [Header("UI (Card)")]
    public Button actionButton;
    public TextMeshProUGUI actionText; // "Craft"
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI gainText;
    public Image iconImage;
    public Slider progressSlider;

    [Header("UI (Locked)")]
    public GameObject lockedOverlayRoot;
    public TextMeshProUGUI lockedOverlayText;

    [Header("UI (Crafting Setup)")]
    public GameObject craftingSetupRoot;

    private string _selectedRecipeId;
    private ActionCoordinator _coordinator;

    private void Awake()
    {
        if (string.IsNullOrEmpty(_selectedRecipeId))
            _selectedRecipeId = defaultRecipeId;

        _coordinator = FindAnyObjectByType<ActionCoordinator>();
    }

    private void OnEnable()
    {
        if (runner != null)
            runner.OnStateChanged += Refresh;

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionChosen);
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (runner != null)
            runner.OnStateChanged -= Refresh;

        if (actionButton != null)
            actionButton.onClick.RemoveAllListeners();
    }

    public void SelectRecipe(string recipeId)
    {
        _selectedRecipeId = recipeId;
        Refresh();
    }

    private void OnActionClicked()
    {
        if (runner == null || runner.State == null || runner.db == null) return;
        if (string.IsNullOrEmpty(_selectedRecipeId)) return;

        var recipe = runner.db.GetSmithingRecipe(_selectedRecipeId);
        if (recipe == null) return;

        bool isRunningSelected = runner.State.activeSmithingRecipeId == recipe.id;

        if (isRunningSelected)
            _coordinator?.StopAll();
        else
            _coordinator?.StartSmithing(recipe.id);

        Refresh();
    }

    private void OnActionChosen()
    {
        //setup le panel de craft ou il y aura le actionClicked
        if (craftingSetupRoot != null)
            craftingSetupRoot.SetActive(true);
    }

    private void Refresh()
    {
        if (runner == null || runner.db == null || runner.State == null)
            return;

        var recipe = runner.db.GetSmithingRecipe(_selectedRecipeId);
        if (recipe == null)
            return;

        // Locked
        int required = Mathf.Max(1, recipe.requiredSmithingLevel);
        int current = runner.State.smithing != null ? runner.State.smithing.level : 1;
        bool unlocked = current >= required;

        if (lockedOverlayRoot != null)
            lockedOverlayRoot.SetActive(!unlocked);

        if (lockedOverlayText != null)
            lockedOverlayText.text = $"LOCKED - Lv {required}";

        // Texts
        if (actionText != null) actionText.text = "Craft";
        if (nameText != null) nameText.text = recipe.displayName;

        if (gainText != null)
        {
            string sec = recipe.craftDuration.ToString("0.##");
            gainText.text = $"{recipe.xpPerCraft} Skill XP / {sec} seconds";
        }

        // Icon
        if (iconImage != null)
        {
            iconImage.sprite = recipe.icon != null ? recipe.icon : (recipe.outputItem != null ? recipe.outputItem.icon : null);
            iconImage.enabled = iconImage.sprite != null;
        }

        // Button interactable: doit être unlocked + ingrédients OK
        bool canStart = unlocked && runner.CanStart(recipe);
        if (actionButton != null)
            actionButton.interactable = canStart || (runner.State.activeSmithingRecipeId == recipe.id);

        // Progress
        bool isRunningSelected = runner.State.activeSmithingRecipeId == recipe.id;
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = Mathf.Max(0.01f, recipe.craftDuration);
            progressSlider.value = isRunningSelected ? runner.State.activeSmithingProgress : 0f;
        }
    }
}