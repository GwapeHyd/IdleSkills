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

        // Texts
        if (actionText != null) actionText.text = "Cut";
        if (nameText != null) nameText.text = action.displayName;

        if (gainText != null)
        {
            string sec = action.actionDuration.ToString("0.##");
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
            progressSlider.maxValue = Mathf.Max(0.01f, action.actionDuration);
            progressSlider.value = isRunningSelected ? runner.State.activeActionProgress : 0f;
        }

        // Option: feedback visuel du bouton (désactivé si running autre action)
        // Ici: tu peux autoriser le changement d'action en cours (en cliquant une autre card)
        // Si tu veux l'interdire, décommente:
        // bool runningOther = !string.IsNullOrEmpty(runner.State.activeActionId) && !isRunningSelected;
        // if (actionButton != null) actionButton.interactable = !runningOther;
    }
}