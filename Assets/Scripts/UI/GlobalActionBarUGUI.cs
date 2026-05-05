using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalActionBarUGUI : MonoBehaviour
{
    [Header("Systems")]
    public ActionCoordinator coordinator;
    public UIPanelRouterUGUI router;

    [Header("UI")]
    public GameObject root;
    public Button clickAreaButton;     // bouton sur toute la barre
    public Image skillIconImage;       // icône du skill (hache/pickaxe)
    public TextMeshProUGUI actionText; // "Cut" / "Mine"
    public TextMeshProUGUI nameText;   // "Normal Tree" / "Copper"
    public Slider progress01Slider;    // 0..1

    [Header("Skill icons")]
    public Sprite woodcuttingIcon;
    public Sprite miningIcon;

    private GlobalActionKind _kind = GlobalActionKind.None;
    private string _id = "";

    private void OnEnable()
    {
        if (clickAreaButton != null)
        {
            clickAreaButton.onClick.RemoveAllListeners();
            clickAreaButton.onClick.AddListener(OnBarClicked);
        }

        Subscribe();
        ForceRebuild();
    }

    private void OnDisable()
    {
        Unsubscribe();
        if (clickAreaButton != null) clickAreaButton.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        UpdateProgressOnly();
    }

    private void Subscribe()
    {
        if (coordinator == null) return;

        // On écoute les state-changed des systèmes pour rebuild quand ça change
        if (coordinator.woodcutting != null) coordinator.woodcutting.OnStateChanged += ForceRebuild;
        if (coordinator.mining != null) coordinator.mining.OnStateChanged += ForceRebuild;
    }

    private void Unsubscribe()
    {
        if (coordinator == null) return;

        if (coordinator.woodcutting != null) coordinator.woodcutting.OnStateChanged -= ForceRebuild;
        if (coordinator.mining != null) coordinator.mining.OnStateChanged -= ForceRebuild;
    }

    private void OnBarClicked()
    {
        if (router == null || coordinator == null) return;

        // “envoie à celle en cours”
        if (coordinator.ActiveKind == GlobalActionKind.Woodcutting)
            router.Open("woodcutting");
        else if (coordinator.ActiveKind == GlobalActionKind.Mining)
            router.Open("mining");
    }

    private void ForceRebuild()
    {
        if (coordinator == null)
            return;

        var kind = coordinator.ActiveKind;
        var id = coordinator.ActiveId;

        if (kind == GlobalActionKind.None)
        {
            _kind = GlobalActionKind.None;
            _id = "";
            if (root != null) root.SetActive(false);
            return;
        }

        if (root != null) root.SetActive(true);

        if (kind != _kind || id != _id)
        {
            _kind = kind;
            _id = id;

            if (skillIconImage != null)
            {
                var sprite = (kind == GlobalActionKind.Woodcutting) ? woodcuttingIcon :
                             (kind == GlobalActionKind.Mining) ? miningIcon : null;
                skillIconImage.sprite = sprite;
                skillIconImage.enabled = sprite != null;
            }

            if (actionText != null)
                actionText.text = (kind == GlobalActionKind.Woodcutting) ? "Cut" : "Mine";

            if (nameText != null)
            {
                if (kind == GlobalActionKind.Woodcutting)
                {
                    var wc = coordinator.woodcutting;
                    var def = (wc != null) ? wc.db.GetWoodcuttingAction(id) : null;
                    nameText.text = def != null ? def.displayName : id;
                }
                else
                {
                    var ms = coordinator.mining;
                    var def = (ms != null) ? ms.db.GetMiningNode(id) : null;
                    nameText.text = def != null ? def.displayName : id;
                }
            }
        }

        UpdateProgressOnly();
    }

    private void UpdateProgressOnly()
    {
        if (progress01Slider == null || coordinator == null) return;

        progress01Slider.minValue = 0f;
        progress01Slider.maxValue = 1f;

        if (_kind == GlobalActionKind.Woodcutting)
        {
            var wc = coordinator.woodcutting;
            if (wc == null || wc.State == null) { progress01Slider.value = 0f; return; }

            var def = wc.db.GetWoodcuttingAction(_id);
            if (def == null || def.actionDuration <= 0.01f) { progress01Slider.value = 0f; return; }

            progress01Slider.value = Mathf.Clamp01(wc.State.activeActionProgress / def.actionDuration);
            return;
        }

        if (_kind == GlobalActionKind.Mining)
        {
            var ms = coordinator.mining;
            if (ms == null || ms.State == null) { progress01Slider.value = 0f; return; }

            var def = ms.db.GetMiningNode(_id);
            if (def == null || def.actionDuration <= 0.01f) { progress01Slider.value = 0f; return; }

            var node = ms.State.miningNodes.Find(n => n.nodeId == _id);
            bool hasOre = node != null && node.currentOre > 0;

            progress01Slider.value = hasOre
                ? Mathf.Clamp01(ms.State.activeMiningProgress / def.actionDuration)
                : 0f;

            return;
        }

        progress01Slider.value = 0f;
    }
}