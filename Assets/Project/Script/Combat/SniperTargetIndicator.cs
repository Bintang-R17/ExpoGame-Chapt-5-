using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI indicator for sniper auto-targeting.
/// Expects to be placed under a Screen Space - Overlay/Camera Canvas.
/// </summary>
public class SniperTargetIndicator : MonoBehaviour
{
    [SerializeField] private Image ringImage;
    [SerializeField] private float minScale = 0.4f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float perfectScale = 0.25f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (ringImage == null)
        {
            ringImage = GetComponentInChildren<Image>();
        }
    }

    public void UpdatePosition(Vector3 screenPos)
    {
        if (rectTransform == null) return;
        rectTransform.position = screenPos;
    }

    public void UpdateLockProgress(float progress)
    {
        if (ringImage == null) return;

        float scale = Mathf.Lerp(maxScale, minScale, progress);
        rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, scale, progress);

        if (progress >= 1f)
        {
            rectTransform.localScale = Vector3.one * perfectScale;
        }
    }

    public void UpdateColor(Color color)
    {
        if (ringImage == null) return;
        ringImage.color = color;
    }

    public void Show()
    {
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
