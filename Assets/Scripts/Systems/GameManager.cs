using UnityEngine;

public class GameManager : MonoBehaviour
{

    private void Awake()
    {
        // Run in background
        Application.runInBackground = true;
    }
    [ContextMenu("Hard Reset")]
    public void HardReset()
    {
        SaveSystem.HardReset();
    }
}
