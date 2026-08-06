using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class WoodcuttingPanelUGUI : MonoBehaviour
{
    [Header("Systems")]
    public WoodcuttingActionRunner runner;

    [Tooltip("Action par défaut sélectionnée quand on ouvre le panel (ex: chop_normal)")]
    public string defaultActionId = "chop_normal";

    [Header("UI (Card)")]
    public Button actionButton;        // bouton unique (la card)
    public TextMeshProUGUI actionText; // "Cut"
    public TextMeshProUGUI nameText;   // "Normal Tree"
    public TextMeshProUGUI gainText;   // "10 Skill XP / 2 seconds"
    public Image iconImage;            // icône (log ou arbre)
    public Slider progressSlider;      // barre de progression

    [Header("UI (Tree XP)")]
    public TextMeshProUGUI treeLevelText; // "Tree Lv 3"
    public Slider treeXpSlider;           // slider xp arbre
    public TextMeshProUGUI treeXpText;    // "23 / 55 XP"

    [Header("UI (Locked)")]
    public GameObject lockedOverlayRoot;      // panel sombre
    public TextMeshProUGUI lockedOverlayText; // "LOCKED - Lv 5"

    private string _selectedActionId;
    private ActionCoordinator _coordinator;

    private void Awake()
    {
        // Sélection par défaut (une seule fois)
        if (string.IsNullOrEmpty(_selectedActionId))
            _selectedActionId = defaultActionId;
        _coordinator = FindAnyObjectByType<ActionCoordinator>();
    }

    private void OnEnable()
    {
        if (runner != null)
            runner.OnStateChanged += Refresh;

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionClicked);
        }

        // Refresh immédiat à l’ouverture (démarrage compris)
        Refresh();
    }

    private void Start()
    {
        // Double sécurité: si l'ordre d'activation fait que l'état arrive après OnEnable
        Refresh();
    }

    private void OnDisable()
    {
        if (runner != null)
            runner.OnStateChanged -= Refresh;

        if (actionButton != null)
            actionButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Appelle depuis tes boutons "Normal/Oak/Willow" : SelectAction("chop_oak")
    /// </summary>
    public void SelectAction(string actionId)
    {
        _selectedActionId = actionId;
        Refresh();
    }

    private void OnActionClicked()
    {
        if (runner == null || runner.State == null || runner.db == null) return;
        if (string.IsNullOrEmpty(_selectedActionId)) return;

        var selected = runner.db.GetWoodcuttingAction(_selectedActionId);
        if (selected == null) return;

        if (!runner.CanStart(selected))
            return;

        bool isRunningSelected = runner.State.activeActionId == selected.id;

        // Toggle: si déjà en cours => stop, sinon => start cette action
        if (isRunningSelected)
            _coordinator?.StopAll();
        else
            _coordinator?.StartWoodcutting(selected.id);

        // Pas obligatoire (runner déclenche OnStateChanged), mais rend la UI instantanée
        Refresh();
    }

    private void Refresh()
    {
        if (runner == null || runner.db == null || runner.State == null)
            return;

        var action = runner.db.GetWoodcuttingAction(_selectedActionId);
        if (action == null)
            return;
        
        int required = Mathf.Max(1, action.requiredLevel);
        int current = runner.State.woodcutting != null ? runner.State.woodcutting.level : 1;
        bool unlocked = current >= required;

        if (lockedOverlayRoot != null)
            lockedOverlayRoot.SetActive(!unlocked);

        if (lockedOverlayText != null)
            lockedOverlayText.text = $"LOCKED - Lv {required}";

        if (actionButton != null)
            actionButton.interactable = unlocked; // bloque le clic si pas le niveau

        // Texts
        if (actionText != null) actionText.text = "Cut";
        if (nameText != null) nameText.text = action.displayName;

        // Calcul de la durée d'action modifiée par le niveau de l'arbre
        float actionDuration = action.actionDuration;
        if (runner.State.treeStates != null && !string.IsNullOrEmpty(action.id))
        {
            var tree = runner.State.treeStates.Find(t => t.treeId == action.id);
            int treeLevel = tree != null ? tree.level : 0;
            actionDuration = Mathf.Max(0.1f, action.actionDuration - 0.01f * treeLevel);
        }
        if (gainText != null)
        {
            string sec = actionDuration.ToString("0.##");
            gainText.text = $"{action.xpPerAction} Skill XP / {sec} seconds";
        }

        // Icon (log) - si tu préfères une icône "arbre", remplace par action.actionIcon
        if (iconImage != null)
        {
            var sprite = action.outputItem != null ? action.actionIcon : null;
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        // Progress
        bool isRunningSelected = runner.State.activeActionId == action.id;

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = Mathf.Max(0.01f, actionDuration);
            progressSlider.value = isRunningSelected ? runner.State.activeActionProgress : 0f;
        }

        // --- Affichage niveau/XP arbre ---
        if (runner.State.treeStates != null && !string.IsNullOrEmpty(action.id))
        {
            var tree = runner.State.treeStates.Find(t => t.treeId == action.id);
            int treeLevel = tree != null ? tree.level : 0;
            int treeXp = tree != null ? tree.xp : 0;
            int xpToNext = TreeProgression.XpToNext(treeLevel);
            float progress = (xpToNext > 0) ? Mathf.Clamp01((float)treeXp / xpToNext) : 0f;

            if (treeLevelText != null)
                treeLevelText.text = $"Tree Lv {treeLevel}";

            if (treeXpSlider != null)
            {
                treeXpSlider.minValue = 0f;
                treeXpSlider.maxValue = 1f;
                treeXpSlider.value = progress;
            }

            if (treeXpText != null)
                treeXpText.text = $"{treeXp} / {xpToNext}";
        }
    }
}