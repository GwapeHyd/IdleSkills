using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiningPanelUGUI : MonoBehaviour
{
    [Header("Systems")]
    public MiningSystem mining;

    [Tooltip("Node par défaut sélectionné (ex: copper)")]
    public string defaultNodeId = "copper";

    [Header("UI (Card)")]
    public Button actionButton;        // bouton unique (la card)
    public TextMeshProUGUI actionText; // "Mine"
    public TextMeshProUGUI nameText;   // "Copper 6/6"
    public TextMeshProUGUI gainText;   // "10 Skill XP / 2 seconds"
    public Image iconImage;            // icône du node (ou ore)
    public Slider progressSlider;      // barre de progression action

    [Header("Optional")]
    public TextMeshProUGUI nodeLevelText; // optionnel: "Node Lv 12 / 100"
    public Slider nodeXpSlider;           // optionnel: progress node xp (vers next level)
    public TextMeshProUGUI playerLevelText; // optionnel: "Mining Lv 5"
    public Slider playerXpSlider;           // optionnel: progress xp joueur (vers next level)

    private string _selectedNodeId;
    private ActionCoordinator _coordinator;

    private void Awake()
    {
        if (string.IsNullOrEmpty(_selectedNodeId))
            _selectedNodeId = defaultNodeId;
        _coordinator = FindAnyObjectByType<ActionCoordinator>();
    }

    private void OnEnable()
    {
        if (mining != null)
            mining.OnStateChanged += Refresh;

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionClicked);
        }

        Refresh();
    }

    private void Start()
    {
        // Double sécurité (order d’init)
        Refresh();
    }

    private void Update()
    {
        if (progressSlider == null || mining == null || mining.State == null || mining.db == null)
            return;

        var def = mining.db.GetMiningNode(_selectedNodeId);
        if (def == null) return;

        // Slider uniquement si ce node est l'action active
        if (mining.State.activeMiningNodeId != def.id)
            return;

        var node = mining.State.miningNodes.Find(n => n.nodeId == def.id);
        if (node == null || node.currentOre <= 0)
        {
            progressSlider.value = 0f;
            return;
        }

        // Normalisé 0..1 (plus robuste)
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        progressSlider.value = (def.actionDuration <= 0.01f)
            ? 0f
            : Mathf.Clamp01(mining.State.activeMiningProgress / def.actionDuration);
    }

    private void OnDisable()
    {
        if (mining != null)
            mining.OnStateChanged -= Refresh;

        if (actionButton != null)
            actionButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Appelle depuis tes boutons "Copper/Iron/..." : SelectNode("copper")
    /// </summary>
    public void SelectNode(string nodeId)
    {
        _selectedNodeId = nodeId;
        Refresh();
    }

    private void OnActionClicked()
    {
        if (mining == null || mining.db == null || mining.State == null) return;
        if (string.IsNullOrEmpty(_selectedNodeId)) return;

        var def = mining.db.GetMiningNode(_selectedNodeId);
        if (def == null) return;

        bool isRunningSelected = mining.State.activeMiningNodeId == def.id;

        if (isRunningSelected)
            _coordinator?.StopAll();
        else
            _coordinator?.StartMining(def.id);

        Refresh();
    }

    private void Refresh()
    {
        if (mining == null || mining.db == null || mining.State == null)
            return;

        var def = mining.db.GetMiningNode(_selectedNodeId);
        if (def == null)
            return;

        var node = mining.State.miningNodes.Find(n => n.nodeId == def.id);
        if (node == null)
            return;

        bool isRunningSelected = mining.State.activeMiningNodeId == def.id;

        // --- Texts ---
        if (actionText != null) actionText.text = "Mine";

        if (nameText != null)
            nameText.text = $"{def.displayName} {node.currentOre}/{node.maxOre}";

        if (gainText != null)
        {
            string sec = def.actionDuration.ToString("0.##");
            gainText.text = $"{def.playerXpPerAction} Skill XP / {sec} seconds";
        }

        // --- Icon ---
        if (iconImage != null)
        {
            Sprite sprite = def.icon != null ? def.icon : (def.oreItem != null ? def.oreItem.icon : null);
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        // --- Action slider ---
        // Progress is time-based; should be 0..duration. If you want a 0..1 slider, set maxValue=1 and assign normalized.
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = Mathf.Max(0.01f, def.actionDuration);

            if (!isRunningSelected)
                progressSlider.value = 0f;
            else
                progressSlider.value = (node.currentOre > 0) ? mining.State.activeMiningProgress : 0f;
        }

        // --- Optional: node level/xp UI ---
        if (nodeLevelText != null)
            nodeLevelText.text = $"Node Lv {node.level} / {MiningNodeProgression.MaxLevel}";

        if (nodeXpSlider != null)
        {
            nodeXpSlider.minValue = 0f;
            nodeXpSlider.maxValue = 1f;
            nodeXpSlider.value = MiningNodeProgression.GetNodeProgress01(node);
        }

        // --- Optional: player skill level/xp UI ---
        if (playerLevelText != null && mining.State.mining != null)
            playerLevelText.text = $"Mining Lv {mining.State.mining.level}";

        if (playerXpSlider != null && mining.State.mining != null && mining.miningXpTable != null)
        {
            playerXpSlider.minValue = 0f;
            playerXpSlider.maxValue = 1f;
            playerXpSlider.value = SkillSystem.GetProgress01(
                mining.State.mining.xp,
                mining.State.mining.level,
                mining.miningXpTable);
        }
    }
}
