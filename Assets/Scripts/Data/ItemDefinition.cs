using UnityEngine;

[CreateAssetMenu(menuName = "Idle/Data/Item")]
public class ItemDefinition : ScriptableObject
{
    public string id;          // ex: "log_normal"
    public string displayName; // ex: "Bûche"
    public Sprite icon;
    public int maxStack = 999;
}