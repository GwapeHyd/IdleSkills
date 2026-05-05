using System;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveKey = "IDLE_SAVE_V1";

    public static void Save(PlayerState state)
    {
        state.lastSeenUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static PlayerState LoadOrCreate()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            var s = new PlayerState();
            s.lastSeenUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return s;
        }

        var json = PlayerPrefs.GetString(SaveKey);
        try
        {
            var state = JsonUtility.FromJson<PlayerState>(json);
            if (state == null) state = new PlayerState();
            return state;
        }
        catch
        {
            return new PlayerState();
        }
    }

    public static void HardReset()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }
}