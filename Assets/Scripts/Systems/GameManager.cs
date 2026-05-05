using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerState State { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Application.runInBackground = true;

        State = SaveSystem.LoadOrCreate();
        SaveSystem.Save(State); // optionnel: écrit un save "propre" au démarrage
    }

    private void OnApplicationQuit() => SaveSystem.Save(State);
    private void OnApplicationPause(bool pause) { if (pause) SaveSystem.Save(State); }

    [ContextMenu("Hard Reset")]
    public void HardReset()
    {
        SaveSystem.HardReset();
        State = SaveSystem.LoadOrCreate(); // recharge un état neuf en mémoire
    }
}