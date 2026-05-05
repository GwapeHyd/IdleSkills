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
        Debug.Log($"[MiningPanelUGUI] Refresh mining={(mining!=null)} db={(mining!=null && mining.db!=null)} state={(mining!=null && mining.State!=null)} selected={_selectedNodeId}");

        if (mining == null || mining.db == null || mining.State == null)
            return;

        var def = mining.db.GetMiningNode(_selectedNodeId);
        if (def == null)
            return;
        
        Debug.Log($"[MiningPanelUGUI] refs actionText={(actionText!=null)} nameText={(nameText!=null)} gainText={(gainText!=null)} icon={(iconImage!=null)} slider={(progressSlider!=null)} button={(actionButton!=null)}");



        var node = mining.State.miningNodes.Find(n => n.nodeId == def.id);
        Debug.Log($"[MiningPanelUGUI] nodesCount={mining.State.miningNodes?.Count ?? -1} node={(node!=null ? node.nodeId : "NULL")} current/max={(node!=null ? $"{node.currentOre}/{node.maxOre}" : "-")}");
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
            // Priorité: icône du node; sinon ore item
            Sprite sprite = def.icon != null ? def.icon : (def.oreItem != null ? def.oreItem.icon : null);
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        // --- Action slider ---
        // Si node vide, l'action reste "en cours" mais la barre ne bouge pas.
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

        // Optionnel: si tu veux empêcher de cliquer un node trop bas level joueur:
        // if (actionButton != null) actionButton.interactable = mining.State.mining.level >= def.requiredMiningLevel;
    }
}