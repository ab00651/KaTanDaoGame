using UnityEngine;

public class flash : MonoBehaviour
{
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField, Range(1f, 1.5f), Tooltip("高亮比原图大多少，值越大白边越宽")]
    private float highlightWidth = 1.1f;

    private SpriteRenderer highlightSr;

    void Start()
    {
        // 找到主 SpriteRenderer
        var mainSr = GetComponentInChildren<SpriteRenderer>();
        if (mainSr == null)
        {
            Debug.LogWarning($"flash: {name} 及其子对象未找到 SpriteRenderer");
            return;
        }

        // 创建高亮子对象，放在主 Sprite 后方
        var highlightGo = new GameObject("Highlight");
        highlightGo.transform.SetParent(mainSr.transform.parent);
        highlightGo.transform.localPosition = mainSr.transform.localPosition;
        highlightGo.transform.localRotation = mainSr.transform.localRotation;
        highlightGo.transform.localScale = mainSr.transform.localScale * highlightWidth;

        highlightSr = highlightGo.AddComponent<SpriteRenderer>();
        highlightSr.sprite = mainSr.sprite;
        highlightSr.color = highlightColor;
        highlightSr.sortingOrder = mainSr.sortingOrder - 1;
        highlightSr.enabled = false;
    }

    void OnMouseEnter()
    {
        if (highlightSr != null)
            highlightSr.enabled = true;
    }

    void OnMouseExit()
    {
        if (highlightSr != null)
            highlightSr.enabled = false;
    }
}
