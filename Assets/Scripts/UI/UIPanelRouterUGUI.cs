using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelRouterUGUI : MonoBehaviour
{
    public enum HeaderMode { None, Skill }

    [Serializable]
    public class View
    {
        [Tooltip("Id unique: inventory, combat, woodcutting, mining...")]
        public string id;

        [Tooltip("Texte affiché dans le bandeau du haut (ex: Inventory)")]
        public string displayName;

        [Tooltip("Icon affichée dans le bandeau du haut")]
        public Sprite icon;

        [Tooltip("Panel racine à activer/désactiver")]
        public GameObject panel;

        [Header("Skill (optionnel)")]
        public HeaderMode headerMode = HeaderMode.None;

        [Tooltip("Si headerMode=Skill, quel skill afficher (ex: Woodcutting)")]
        public SkillKind skillKind = SkillKind.None;
    }

    public enum SkillKind { None, Woodcutting, Mining /* + Smithing, etc. */ }

    [Header("Data / Systems")]
    public WoodcuttingActionRunner runner; 
    public MiningSystem mining;

    [Header("Views")]
    public List<View> views = new();
    [Header("Startup")]
    public string defaultViewId = "inventory";

    [Header("Top Header (Icon + Name)")]
    public Image topIcon;
    public TextMeshProUGUI topName;

    [Header("Skill Header (Level / XP / Slider)")]
    public GameObject skillHeaderRoot;      // parent à activer/désactiver
    public TextMeshProUGUI skillLevelText;  // "Skill Level 9 / 100"
    public TextMeshProUGUI skillXpText;     // "Skill XP 1054"
    public Slider skillXpSlider;            // slider XP (progress vers level suivant)

    // internal
    private readonly Dictionary<string, View> _byId = new();
    private View _current;

    private void Awake()
    {
        _byId.Clear();
        foreach (var v in views)
        {
            if (v == null || string.IsNullOrWhiteSpace(v.id)) continue;
            _byId[v.id] = v;
        }
    }

    private void Start()
    {
        // Start arrive après tous les Awake()
        if (!string.IsNullOrEmpty(defaultViewId))
            Open(defaultViewId);

        // Double sécurité: si runner update après (offline progress etc.)
        RefreshHeaders();
    }

    private void OnEnable()
    {
        if (runner != null)
            runner.OnStateChanged += RefreshHeaders;
        
        if (mining != null)
            mining.OnStateChanged += RefreshHeaders;
    }

    private void OnDisable()
    {
        if (runner != null)
            runner.OnStateChanged -= RefreshHeaders;

        if (mining != null)
            mining.OnStateChanged -= RefreshHeaders;
    }

    public void Open(string viewId)
    {
        if (!_byId.TryGetValue(viewId, out var target) || target.panel == null)
        {
            Debug.LogWarning($"[UIPanelRouterUGUI] Unknown view id '{viewId}' or panel missing.", this);
            return;
        }

        // Fermer l'ancien
        if (_current != null && _current.panel != null)
            _current.panel.SetActive(false);

        // Ouvrir le nouveau
        _current = target;
        _current.panel.SetActive(true);

        RefreshHeaders();
    }

    private void RefreshHeaders()
    {
        if (_current == null)
            return;

        // Bandeau top : icon + name
        if (topIcon != null) topIcon.sprite = _current.icon;
        if (topIcon != null) topIcon.enabled = _current.icon != null;
        if (topName != null) topName.text = _current.displayName ?? _current.id;

        // Bandeau skill (si besoin)
        bool showSkill = _current.headerMode == HeaderMode.Skill && _current.skillKind != SkillKind.None;
        if (skillHeaderRoot != null) skillHeaderRoot.SetActive(showSkill);

        if (!showSkill)
            return;

        if (runner == null || runner.State == null)
            return;

        SkillState skill = null;
        SkillXpTable table = null;
        switch (_current.skillKind)
        {
            case SkillKind.Woodcutting:
                skill = runner.State.woodcutting;
                table = runner.woodcuttingXpTable;
                break;
            case SkillKind.Mining:
                skill = mining.State.mining;
                table = mining.miningXpTable;
                break;
        }

        if (skill == null || table == null)
            return;

        // Texte
        if (skillLevelText != null)
            skillLevelText.text = $"Skill Level {skill.level} / {table.maxLevel}";

        if (skillXpText != null)
            skillXpText.text = $"Skill XP {skill.xp:N0}";

        if (skillXpSlider != null)
        {
            skillXpSlider.minValue = 0f;
            skillXpSlider.maxValue = 1f;
            skillXpSlider.value = SkillSystem.GetProgress01(skill.xp, skill.level, table);
        }
    }
}