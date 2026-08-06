using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnnouncementToastUGUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI messageText;

    [Tooltip("Optionnel: root visuel (Image/Panel). Si null, on utilise le GameObject courant.")]
    public RectTransform visualRoot;

    [Header("Timing")]
    public float lifetimeSeconds = 3f;

    [Header("Animation")]
    public float popSeconds = 0.15f;
    public float startScale = 0.92f;
    public float endScale = 1.0f;
    public float fadeInSeconds = 0.12f;
    public float fadeOutSeconds = 0.20f;

    private CanvasGroup _cg;

    private void Awake()
    {
        if (visualRoot == null)
            visualRoot = transform as RectTransform;

        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void Bind(string message)
    {
        if (messageText != null)
            messageText.text = message;

        // Important: forcer un rebuild layout immédiatement pour que la taille s’adapte
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

        StopAllCoroutines();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        // état initial
        _cg.alpha = 0f;
        if (visualRoot != null)
            visualRoot.localScale = Vector3.one * startScale;

        // Fade in + pop
        float t = 0f;
        float fadeDur = Mathf.Max(0.01f, fadeInSeconds);
        float popDur = Mathf.Max(0.01f, popSeconds);

        while (t < Mathf.Max(fadeDur, popDur))
        {
            t += Time.unscaledDeltaTime;

            float a = Mathf.Clamp01(t / fadeDur);
            _cg.alpha = a;

            float s = Mathf.Clamp01(t / popDur);
            float scale = Mathf.Lerp(startScale, endScale, s);
            if (visualRoot != null) visualRoot.localScale = Vector3.one * scale;

            yield return null;
        }

        _cg.alpha = 1f;
        if (visualRoot != null) visualRoot.localScale = Vector3.one * endScale;

        // attente
        float remain = Mathf.Max(0f, lifetimeSeconds);
        yield return new WaitForSecondsRealtime(remain);

        // fade out
        float outT = 0f;
        float outDur = Mathf.Max(0.01f, fadeOutSeconds);
        while (outT < outDur)
        {
            outT += Time.unscaledDeltaTime;
            _cg.alpha = 1f - Mathf.Clamp01(outT / outDur);
            yield return null;
        }

        Destroy(gameObject);
    }
}