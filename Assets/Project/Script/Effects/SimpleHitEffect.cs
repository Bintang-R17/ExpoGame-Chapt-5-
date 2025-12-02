using UnityEngine;

/// <summary>
/// Simple hit effect untuk weapon impact
/// Attach ke prefab dengan particle system atau animasi
/// </summary>
public class SimpleHitEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private bool autoDestroy = true;
    [SerializeField] private bool playParticles = true;
    [SerializeField] private bool playAnimation = true;
    [SerializeField] private bool playSound = true;
    
    [Header("Scale Animation")]
    [SerializeField] private bool animateScale = true;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float maxScale = 2f;
    
    [Header("Rotation")]
    [SerializeField] private bool randomRotation = true;
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 360, 0);
    
    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float volume = 0.5f;

    private ParticleSystem particles;
    private Animator animator;
    private AudioSource audioSource;
    private float timer;
    private Vector3 initialScale;

    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        initialScale = transform.localScale;
        
        // Random rotation
        if (randomRotation)
        {
            transform.rotation = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            );
        }
        
        // Play particle system
        if (playParticles && particles != null)
        {
            particles.Play();
        }
        
        // Play animation
        if (playAnimation && animator != null)
        {
            animator.SetTrigger("Play");
        }
        
        // Play sound
        if (playSound && hitSound != null)
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            audioSource.clip = hitSound;
            audioSource.volume = volume;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.Play();
        }
        
        // Auto destroy
        if (autoDestroy)
        {
            Destroy(gameObject, lifetime);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / lifetime);
        
        // Scale animation
        if (animateScale)
        {
            float scaleValue = scaleCurve.Evaluate(progress);
            transform.localScale = initialScale * (1f + scaleValue * maxScale);
        }
        
        // Rotation
        if (rotationSpeed.sqrMagnitude > 0)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
}
