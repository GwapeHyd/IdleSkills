using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemRowUGUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI amountText;

    public void Bind(Sprite sprite, string displayName, int amount)
    {
        if (icon != null) icon.sprite = sprite;
        if (nameText != null) nameText.text = displayName;
        if (amountText != null) amountText.text = amount.ToString();
    }
}