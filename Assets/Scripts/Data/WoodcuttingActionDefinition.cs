using UnityEngine;

[CreateAssetMenu(menuName = "Idle/Data/Woodcutting Action")]
public class WoodcuttingActionDefinition : ScriptableObject
{
    public string id;                 
    public string displayName;        
    public int requiredLevel = 1;
    public float actionDuration = 2f; 
    public ItemDefinition outputItem; 
    public int outputAmount = 1;      
    public int xpPerAction = 10;     
    public Sprite actionIcon; 
}