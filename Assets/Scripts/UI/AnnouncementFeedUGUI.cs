using System.Collections.Generic;
using UnityEngine;

public class AnnouncementFeedUGUI : MonoBehaviour
{
    [Header("UI")]
    public RectTransform container;                 // un VerticalLayoutGroup (ancré en bas)
    public AnnouncementToastUGUI toastPrefab;

    [Header("Rules")]
    public int maxToasts = 5;

    private readonly List<AnnouncementToastUGUI> _alive = new();

    private void OnEnable()
    {
        AnnouncementEvents.OnAnnounce += HandleAnnounce;
    }

    private void OnDisable()
    {
        AnnouncementEvents.OnAnnounce -= HandleAnnounce;
    }

    private void HandleAnnounce(string message)
    {
        if (container == null || toastPrefab == null) return;

        // cleanup refs null
        _alive.RemoveAll(t => t == null);

        // si trop, on supprime le plus ancien (celui en haut)
        while (_alive.Count >= maxToasts)
        {
            if (_alive[0] != null)
                Destroy(_alive[0].gameObject);
            _alive.RemoveAt(0);
        }

        var toast = Instantiate(toastPrefab, container);
        toast.gameObject.SetActive(true);
        toast.Bind(message);

        _alive.Add(toast);
    }
}