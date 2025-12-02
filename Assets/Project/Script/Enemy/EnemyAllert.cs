using UnityEngine;

public class EnemyAllert : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0); // Offset di atas kepala enemy
    [SerializeField] private bool fadeInOut = true;
    [SerializeField] private float fadeSpeed = 5f;

    private Camera mainCam;
    private CanvasGroup canvasGroup;
    private Transform parentEnemy;
    private float targetAlpha;

    void Start()
    {
        mainCam = Camera.main;
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        parentEnemy = transform.parent;

        if (fadeInOut)
            canvasGroup.alpha = 0f;
    }

    void OnEnable()
    {
        if (fadeInOut && canvasGroup != null)
            targetAlpha = 1f;
    }

    void OnDisable()
    {
        if (fadeInOut && canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    void LateUpdate()
    {
        if (mainCam == null) return;

        // Posisi icon di atas enemy
        if (parentEnemy != null)
            transform.position = parentEnemy.position + offset;

        // Billboard effect - icon selalu menghadap kamera
        transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);

        // Fade in/out effect
        if (fadeInOut && canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }
    }
}