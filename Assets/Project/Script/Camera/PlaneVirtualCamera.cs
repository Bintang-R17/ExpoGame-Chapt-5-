using UnityEngine;
using Unity.Cinemachine;

public class PlaneVirtualCamera : MonoBehaviour
{
    [Header("Virtual Camera Setup")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Transform planeTarget;
    
    [Header("Camera Settings")]
    [SerializeField] private Vector3 followOffset = new Vector3(0, 3, -12);
    [SerializeField] private float damping = 1f;
    
    [Header("Look Settings")]
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0, 0, 5);
    
    [Header("Noise Settings (Camera Shake)")]
    [SerializeField] private bool enableNoise = true;
    [SerializeField] private float amplitudeGain = 0.5f;
    [SerializeField] private float frequencyGain = 1f;
    
    private CinemachineFollow followComponent;
    private CinemachineRotationComposer composerComponent;
    private CinemachineBasicMultiChannelPerlin noiseComponent;
    private ArcadePlaneController planeController;
    
    void Start()
    {
        // Auto-create virtual camera if not assigned
        if (virtualCamera == null)
        {
            GameObject vcamObj = new GameObject("PlaneVirtualCamera");
            vcamObj.transform.SetParent(transform);
            virtualCamera = vcamObj.AddComponent<CinemachineCamera>();
            virtualCamera.Priority.Value = 10;
        }
        
        // Auto-find plane if not assigned
        if (planeTarget == null)
        {
            planeController = FindFirstObjectByType<ArcadePlaneController>();
            if (planeController != null)
            {
                planeTarget = planeController.transform;
                Debug.Log("[PlaneVirtualCamera] Auto-detected plane: " + planeTarget.name);
            }
            else
            {
                Debug.LogError("[PlaneVirtualCamera] No plane target found! Please assign manually.");
                enabled = false;
                return;
            }
        }
        
        // Setup Follow and LookAt
        virtualCamera.Target.TrackingTarget = planeTarget;
        virtualCamera.Target.LookAtTarget = planeTarget;
        
        // Setup Follow behavior
        SetupFollow();
        
        // Setup Rotation Composer (Aim behavior)
        SetupComposer();
        
        // Setup Noise (Camera shake)
        if (enableNoise)
        {
            SetupNoise();
        }
        
        // Get plane controller reference if not already set
        if (planeController == null && planeTarget != null)
        {
            planeController = planeTarget.GetComponent<ArcadePlaneController>();
        }
    }
    
    void SetupFollow()
    {
        followComponent = virtualCamera.GetComponent<CinemachineFollow>();
        
        if (followComponent == null)
        {
            followComponent = virtualCamera.gameObject.AddComponent<CinemachineFollow>();
        }
        
        // Set follow offset (behind and above the plane)
        followComponent.FollowOffset = followOffset;
        
        // Set damping for smooth follow
        followComponent.TrackerSettings.PositionDamping = new Vector3(damping, damping, damping);
        followComponent.TrackerSettings.RotationDamping = new Vector3(damping * 1.5f, damping * 1.5f, damping * 1.5f);
    }
    
    void SetupComposer()
    {
        composerComponent = virtualCamera.GetComponent<CinemachineRotationComposer>();
        
        if (composerComponent == null)
        {
            composerComponent = virtualCamera.gameObject.AddComponent<CinemachineRotationComposer>();
        }
        
        // Set look at offset (look ahead of the plane)
        composerComponent.TargetOffset = lookAtOffset;
    }
    
    void SetupNoise()
    {
        noiseComponent = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        
        if (noiseComponent == null)
        {
            noiseComponent = virtualCamera.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
        }
        
        // Set noise profile
        noiseComponent.AmplitudeGain = amplitudeGain;
        noiseComponent.FrequencyGain = frequencyGain;
    }
    
    void Update()
    {
        // Dynamic noise based on plane speed/boost
        if (enableNoise && noiseComponent != null && planeController != null)
        {
            float speed = planeController.GetCurrentSpeed();
            bool isBoosting = planeController.IsBoosting();
            
            // Increase shake when boosting or at high speed
            float targetAmplitude = isBoosting ? amplitudeGain * 2f : amplitudeGain;
            targetAmplitude *= Mathf.Clamp01(speed / 80f); // Scale with speed
            
            noiseComponent.AmplitudeGain = Mathf.Lerp(noiseComponent.AmplitudeGain, targetAmplitude, Time.deltaTime * 2f);
        }
    }
    
    // Public methods
    public void SetTarget(Transform newTarget)
    {
        planeTarget = newTarget;
        if (virtualCamera != null)
        {
            virtualCamera.Target.TrackingTarget = newTarget;
            virtualCamera.Target.LookAtTarget = newTarget;
        }
        
        planeController = newTarget?.GetComponent<ArcadePlaneController>();
    }
    
    public void SetFollowOffset(Vector3 offset)
    {
        followOffset = offset;
        if (followComponent != null)
        {
            followComponent.FollowOffset = offset;
        }
    }
    
    public void SetLookAtOffset(Vector3 offset)
    {
        lookAtOffset = offset;
        if (composerComponent != null)
        {
            composerComponent.TargetOffset = offset;
        }
    }
    
    public void SetPriority(int priority)
    {
        if (virtualCamera != null)
        {
            virtualCamera.Priority.Value = priority;
        }
    }
    
    public CinemachineCamera GetVirtualCamera()
    {
        return virtualCamera;
    }
}
