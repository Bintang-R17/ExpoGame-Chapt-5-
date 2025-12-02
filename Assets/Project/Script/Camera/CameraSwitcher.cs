using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private Camera[] cameras;
    [SerializeField] private int defaultCameraIndex = 0;
    
    [Header("Input Settings")]
    [SerializeField] private bool useNumberKeys = true; // 1-9 keys
    [SerializeField] private bool useCycleKey = true;   // Tab key untuk cycle
    [SerializeField] private Key cycleKey = Key.Tab;
    
    private int currentCameraIndex;
    
    void Start()
    {
        // Validate cameras array
        if (cameras == null || cameras.Length == 0)
        {
            Debug.LogError("[CameraSwitcher] No cameras assigned!");
            enabled = false;
            return;
        }
        
        // Remove null entries
        cameras = System.Array.FindAll(cameras, cam => cam != null);
        
        if (cameras.Length == 0)
        {
            Debug.LogError("[CameraSwitcher] All camera references are null!");
            enabled = false;
            return;
        }
        
        // Clamp default index
        currentCameraIndex = Mathf.Clamp(defaultCameraIndex, 0, cameras.Length - 1);
        
        // Activate default camera
        SwitchToCamera(currentCameraIndex);
    }
    
    void Update()
    {
        // Number keys (1-9) untuk switch langsung
        if (useNumberKeys)
        {
            for (int i = 0; i < Mathf.Min(cameras.Length, 9); i++)
            {
                if (Keyboard.current != null && Keyboard.current[(Key)(Key.Digit1 + i)].wasPressedThisFrame)
                {
                    SwitchToCamera(i);
                    return;
                }
            }
        }
        
        // Cycle key (Tab) untuk switch ke kamera berikutnya
        if (useCycleKey && Keyboard.current != null && Keyboard.current[cycleKey].wasPressedThisFrame)
        {
            CycleToNextCamera();
        }
    }
    
    void SwitchToCamera(int index)
    {
        if (index < 0 || index >= cameras.Length)
        {
            Debug.LogWarning($"[CameraSwitcher] Invalid camera index: {index}");
            return;
        }
        
        // Disable all cameras
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
            {
                cameras[i].enabled = (i == index);
            }
        }
        
        currentCameraIndex = index;
        Debug.Log($"[CameraSwitcher] Switched to Camera {index + 1}: {cameras[index].name}");
    }
    
    void CycleToNextCamera()
    {
        int nextIndex = (currentCameraIndex + 1) % cameras.Length;
        SwitchToCamera(nextIndex);
    }
    
    // Public methods untuk dipanggil dari script lain
    public void SwitchToCameraByName(string cameraName)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].name == cameraName)
            {
                SwitchToCamera(i);
                return;
            }
        }
        Debug.LogWarning($"[CameraSwitcher] Camera with name '{cameraName}' not found!");
    }
    
    public Camera GetCurrentCamera()
    {
        return cameras[currentCameraIndex];
    }
    
    public int GetCurrentCameraIndex()
    {
        return currentCameraIndex;
    }
}

