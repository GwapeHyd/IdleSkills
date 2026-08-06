using UnityEngine;
using UnityEngine.UI;
public class ContentSizeFitterWithMax : ContentSizeFitter {
    public Vector2 sizeMin = Vector2.zero;
    public Vector2 sizeMax = new Vector2(200f, 500f);

    public override void SetLayoutHorizontal() {
        base.SetLayoutHorizontal();
        var rectTransform = transform as RectTransform;
        var sizeDelta = rectTransform.sizeDelta;
        sizeDelta.x = Mathf.Clamp(sizeDelta.x, sizeMin.x, sizeMax.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeDelta.x);
    }

    public override void SetLayoutVertical() {
        base.SetLayoutVertical();
        var rectTransform = transform as RectTransform;
        var sizeDelta = rectTransform.sizeDelta;
        sizeDelta.y = Mathf.Clamp(sizeDelta.y, sizeMin.y, sizeMax.y);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeDelta.y);
    }
}