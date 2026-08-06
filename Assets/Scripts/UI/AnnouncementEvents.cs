using System;
using UnityEngine;

public static class AnnouncementEvents
{
    public static event Action<string> OnAnnounce;

    public static void Announce(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        OnAnnounce?.Invoke(message);
    }
}