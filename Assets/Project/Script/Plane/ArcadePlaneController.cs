using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ArcadePlaneController : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float baseSpeed = 40f; // Kecepatan default (auto forward)
    [SerializeField] private float maxSpeed = 80f;
    [SerializeField] private float minSpeed = 20f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 5f;
    
    [Header("Control Settings")]
    [Tooltip("Select input method: Mouse Only, Analog Only, or Hybrid (both)")]
    [SerializeField] private ControlInputMode controlMode = ControlInputMode.Hybrid;
    [Tooltip("GTA Style: Left Stick = Pitch/Roll, Right Stick = Yaw/Camera")]
    [SerializeField] private bool gtaStyleControls = true;
    [SerializeField] private float pitchSpeed = 50f;   // Up/Down (Left Stick Y)
    [SerializeField] private float rollSpeed = 100f;    // Left/Right tilt (Left Stick X)
    [SerializeField] private float yawSpeed = 30f;      // Turn left/right (Right Stick X)
    [SerializeField] private bool invertPitch = false;  // Invert Y axis
    [SerializeField] private bool invertYaw = false;    // Invert yaw axis
    
    public enum ControlInputMode
    {
        MouseOnly,      // Hanya mouse
        AnalogOnly,     // Hanya gamepad analog
        Hybrid          // Keduanya (mouse + analog)
    }
    
    [Header("Mouse Settings (Mouse Only / Hybrid)")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private bool invertMouseY = false;
    [SerializeField] private float mouseSmoothing = 0.1f;  // Lower = smoother
    
    [Header("Motion Control (Gyroscope)")]
    [Tooltip("Enable physical gamepad motion control (tilt controller = tilt plane)")]
    [SerializeField] private bool enableMotionControl = false;
    [SerializeField] private MotionControlMode motionMode = MotionControlMode.HybridRightStick;
    [SerializeField] private float motionSensitivity = 2f;        // Sensitivitas gyro
    [SerializeField] private float motionPitchMultiplier = 1f;    // Multiplier pitch dari tilt depan/belakang
    [SerializeField] private float motionRollMultiplier = 1f;     // Multiplier roll dari tilt kiri/kanan
    [SerializeField] private bool motionOverrideStick = false;    // Motion menggantikan stick input sepenuhnya
    [SerializeField] private float motionDeadzone = 0.05f;        // Dead zone untuk motion kecil
    
    public enum MotionControlMode
    {
        AutoDetect,          // Coba detect sensor otomatis
        HybridRightStick,    // Right stick sebagai motion simulator (recommend untuk DS4Windows)
        SimulatedOnly        // Full simulated dengan right stick
    }
    
    [Header("Physics Settings")]
    [SerializeField] private float lift = 20f;          // Gaya angkat (anti-gravity)
    [SerializeField] private float drag = 0.5f;         // Air resistance
    [SerializeField] private float angularDrag = 2f;
    
    [Header("Altitude Limits")]
    [SerializeField] private float minAltitude = 5f;    // Ketinggian minimum
    [SerializeField] private float maxAltitude = 200f;  // Ketinggian maksimum
    [SerializeField] private bool enableAltitudeLimits = true;
    
    [Header("Auto-Stabilization")]
    [SerializeField] private bool autoLevelWings = true;  // Auto level roll saat ga ada input
    [SerializeField] private float levelSpeed = 2f;       // Seberapa cepat auto-level
    
    [Header("Boost")]
    [SerializeField] private bool enableBoost = true;
    [SerializeField] private float boostMultiplier = 1.5f;
    [SerializeField] private float boostDuration = 3f;
    [SerializeField] private float boostCooldown = 5f;
    
    [Header("Floating Effect")]
    [SerializeField] private bool enableFloating = true;
    [SerializeField] private float floatAmplitude = 0.3f;      // Gerakan naik/turun
    [SerializeField] private float floatFrequency = 1f;        // Kecepatan float
    [SerializeField] private float rollFloatAmount = 2f;       // Roll halus kiri/kanan
    [SerializeField] private float rollFloatFrequency = 0.8f;  // Kecepatan roll float
    
    // Components
    private Rigidbody rb;
    
    // Input
    private Vector2 moveInput;      // Left stick (pitch & roll)
    private Vector2 lookInput;      // Right stick (yaw control) - GTA style
    private Vector2 mouseInput;     // Mouse delta
    private Vector2 smoothMouseInput; // Smoothed mouse input
    private float throttleInput;    // Triggers (speed up/down)
    private bool boostPressed;
    
    // Motion Control
    private Vector3 gyroInput;      // Raw gyroscope angular velocity
    private Vector3 accelInput;     // Raw accelerometer (gravity direction)
    private bool hasMotionSupport;  // Apakah gamepad support motion
    
    // State
    private float currentSpeed;
    private float targetSpeed;
    private float boostTimer;
    private float boostCooldownTimer;
    private bool isBoosting;
    private float floatTime;  // Untuk floating effect
    
    // Debug
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Configure Rigidbody
        rb.useGravity = false;  // Custom gravity via lift
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        currentSpeed = baseSpeed;
        targetSpeed = baseSpeed;
        
        // Check motion control support
        CheckMotionSupport();
    }
    
    void FixedUpdate()
    {
        HandleSpeed();
        HandleRotation();
        HandleMovement();
        HandleAltitudeLimits();
        HandleAutoLevel();
        
        if (enableFloating)
        {
            ApplyFloatingEffect();
        }
        
        if (showDebugInfo)
        {
            DrawDebugInfo();
        }
        
        // Reset mouse input setiap frame (mouse delta hanya valid 1 frame)
        if (controlMode == ControlInputMode.MouseOnly || controlMode == ControlInputMode.Hybrid)
        {
            mouseInput = Vector2.Lerp(mouseInput, Vector2.zero, Time.fixedDeltaTime * 10f);
        }
    }
    
    void HandleSpeed()
    {
        // Base speed (always moving forward)
        targetSpeed = baseSpeed;
        
        // Throttle control (triggers atau keyboard)
        if (throttleInput > 0.1f)
        {
            targetSpeed += (maxSpeed - baseSpeed) * throttleInput;
        }
        else if (throttleInput < -0.1f)
        {
            targetSpeed = Mathf.Lerp(baseSpeed, minSpeed, Mathf.Abs(throttleInput));
        }
        
        // Boost
        if (enableBoost && isBoosting)
        {
            targetSpeed *= boostMultiplier;
            boostTimer -= Time.fixedDeltaTime;
            
            if (boostTimer <= 0)
            {
                isBoosting = false;
                boostCooldownTimer = boostCooldown;
            }
        }
        
        // Cooldown
        if (boostCooldownTimer > 0)
        {
            boostCooldownTimer -= Time.fixedDeltaTime;
        }
        
        // Smooth speed transition
        float speedChangeRate = (currentSpeed < targetSpeed) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate * Time.fixedDeltaTime);
    }
    
    void HandleRotation()
    {
        // Get input dari stick atau motion
        Vector2 finalMoveInput = moveInput;
        Vector2 finalLookInput = lookInput;
        
        // Mouse Control (jika mode Mouse Only atau Hybrid)
        if ((controlMode == ControlInputMode.MouseOnly || controlMode == ControlInputMode.Hybrid) 
            && mouseInput.sqrMagnitude > 0.001f)
        {
            // Smooth mouse input
            smoothMouseInput = Vector2.Lerp(smoothMouseInput, mouseInput, 1f - mouseSmoothing);
            
            // Convert mouse delta to control input
            float mousePitch = (invertMouseY ? smoothMouseInput.y : -smoothMouseInput.y) * mouseSensitivity * 0.1f;
            float mouseRoll = smoothMouseInput.x * mouseSensitivity * 0.1f;
            
            if (controlMode == ControlInputMode.MouseOnly)
            {
                // Mouse Only: Mouse control pitch (Y) dan roll (X)
                finalMoveInput = new Vector2(mouseRoll, mousePitch);
            }
            else // Hybrid
            {
                // Hybrid: Mouse HANYA untuk pitch (atas/bawah), analog kiri untuk roll (kiri/kanan)
                // Mouse Y -> Pitch, Analog X -> Roll
                finalMoveInput = new Vector2(
                    moveInput.x,  // Roll dari analog stick kiri (X axis)
                    mousePitch    // Pitch dari mouse (Y axis)
                );
            }
        }
        
        // Analog Control (jika mode Analog Only atau Hybrid)
        // Jika Mouse Only, abaikan analog input
        if (controlMode == ControlInputMode.MouseOnly)
        {
            finalMoveInput = (mouseInput.sqrMagnitude > 0.001f) ? finalMoveInput : Vector2.zero;
        }
        
        // Motion Control Override
        if (enableMotionControl && hasMotionSupport)
        {
            Vector2 motionInput = GetMotionInput();
            
            if (motionOverrideStick)
            {
                // Motion menggantikan stick sepenuhnya
                finalMoveInput = motionInput;
            }
            else
            {
                // Motion + Stick (additif)
                finalMoveInput += motionInput;
                finalMoveInput = Vector2.ClampMagnitude(finalMoveInput, 1f);
            }
        }
        
        if (gtaStyleControls)
        {
            // GTA Style Controls
            // Left Stick Y = Pitch (up/down)
            float pitch = (invertPitch ? finalMoveInput.y : -finalMoveInput.y) * pitchSpeed * Time.fixedDeltaTime;
            
            // Left Stick X = Roll (tilt left/right)
            float roll = -finalMoveInput.x * rollSpeed * Time.fixedDeltaTime;
            
            // Right Stick X = Yaw (turn left/right) - GTA style!
            float yaw = (invertYaw ? -finalLookInput.x : finalLookInput.x) * yawSpeed * Time.fixedDeltaTime;
            
            // Apply rotation
            transform.Rotate(pitch, yaw, roll, Space.Self);
        }
        else
        {
            // Classic Arcade Style (original)
            float pitch = -finalMoveInput.y * pitchSpeed * Time.fixedDeltaTime;
            float roll = -finalMoveInput.x * rollSpeed * Time.fixedDeltaTime;
            float yaw = finalMoveInput.x * yawSpeed * Time.fixedDeltaTime;
            
            transform.Rotate(pitch, yaw, roll, Space.Self);
        }
    }
    
    void HandleMovement()
    {
        // Forward thrust (always moving forward)
        Vector3 forwardForce = transform.forward * currentSpeed;
        rb.linearVelocity = forwardForce;
        
        // Lift force (anti-gravity)
        Vector3 liftForce = transform.up * lift;
        rb.AddForce(liftForce, ForceMode.Force);
        
        // Additional lift based on speed (faster = more lift)
        float speedLift = (currentSpeed / maxSpeed) * lift * 0.5f;
        rb.AddForce(Vector3.up * speedLift, ForceMode.Force);
    }
    
    void HandleAltitudeLimits()
    {
        if (!enableAltitudeLimits) return;
        
        float currentAltitude = transform.position.y;
        
        // Minimum altitude (bounce up)
        if (currentAltitude < minAltitude)
        {
            Vector3 pos = transform.position;
            pos.y = minAltitude;
            transform.position = pos;
            
            // Push upward
            Vector3 vel = rb.linearVelocity;
            vel.y = Mathf.Max(vel.y, 5f);
            rb.linearVelocity = vel;
            
            // Level out pitch
            Vector3 euler = transform.eulerAngles;
            euler.x = 0;
            transform.eulerAngles = euler;
        }
        
        // Maximum altitude (push down)
        if (currentAltitude > maxAltitude)
        {
            Vector3 pos = transform.position;
            pos.y = maxAltitude;
            transform.position = pos;
            
            // Push downward
            Vector3 vel = rb.linearVelocity;
            vel.y = Mathf.Min(vel.y, -5f);
            rb.linearVelocity = vel;
        }
    }
    
    void HandleAutoLevel()
    {
        if (!autoLevelWings) return;
        
        // Auto-level roll saat tidak ada input horizontal
        if (Mathf.Abs(moveInput.x) < 0.1f)
        {
            Vector3 currentRotation = transform.eulerAngles;
            float currentRoll = currentRotation.z;
            
            // Normalize roll to -180 to 180
            if (currentRoll > 180f)
                currentRoll -= 360f;
            
            // Smoothly return to zero roll
            float targetRoll = 0f;
            float newRoll = Mathf.Lerp(currentRoll, targetRoll, levelSpeed * Time.fixedDeltaTime);
            
            currentRotation.z = newRoll;
            transform.eulerAngles = currentRotation;
        }
    }
    
    void ApplyFloatingEffect()
    {
        floatTime += Time.fixedDeltaTime;
        
        // Gerakan vertikal halus (naik-turun)
        float verticalFloat = Mathf.Sin(floatTime * floatFrequency) * floatAmplitude;
        rb.AddForce(Vector3.up * verticalFloat, ForceMode.VelocityChange);
        
        // Gerakan roll halus (miring kiri-kanan)
        float rollFloat = Mathf.Sin(floatTime * rollFloatFrequency) * rollFloatAmount;
        
        // Apply torque untuk efek roll yang smooth
        Vector3 rollTorque = transform.forward * rollFloat;
        rb.AddTorque(rollTorque, ForceMode.Acceleration);
    }
    
    void CheckMotionSupport()
    {
        // Check apakah ada gamepad dengan sensor motion
        var gamepad = UnityEngine.InputSystem.Gamepad.current;
        
        if (gamepad != null)
        {
            Debug.Log($"[Motion Control] Gamepad detected: {gamepad.name}");
            
            // Check berdasarkan mode
            if (motionMode == MotionControlMode.SimulatedOnly || motionMode == MotionControlMode.HybridRightStick)
            {
                hasMotionSupport = true;
                Debug.Log($"[Motion Control] Mode: {motionMode} - Using RIGHT STICK as motion input!");
                Debug.Log("[Motion Control] Hold gamepad and tilt right stick = tilt plane!");
            }
            else
            {
                // Try auto-detect sensors
                try
                {
                    var device = UnityEngine.InputSystem.InputSystem.GetDevice<UnityEngine.InputSystem.Sensor>();
                    hasMotionSupport = device != null;
                    
                    if (hasMotionSupport)
                    {
                        Debug.Log("[Motion Control] Real motion sensors detected!");
                    }
                }
                catch
                {
                    hasMotionSupport = false;
                }
                
                if (!hasMotionSupport)
                {
                    Debug.LogWarning("[Motion Control] No motion sensors detected!");
                    Debug.LogWarning("[Motion Control] Switch to 'Hybrid Right Stick' mode for DS4Windows support!");
                    // Auto fallback to hybrid mode
                    motionMode = MotionControlMode.HybridRightStick;
                    hasMotionSupport = true;
                }
            }
        }
        else
        {
            hasMotionSupport = false;
            Debug.LogWarning("[Motion Control] No gamepad connected!");
        }
    }
    
    Vector2 GetMotionInput()
    {
        // Update motion sensors
        ReadMotionSensors();
        
        // Convert accelerometer (gravity direction) ke pitch/roll
        // Accelerometer mendeteksi arah gravitasi:
        // - Tilt maju = accelInput.z negatif (pitch up)
        // - Tilt mundur = accelInput.z positif (pitch down)
        // - Tilt kiri = accelInput.x negatif (roll left)
        // - Tilt kanan = accelInput.x positif (roll right)
        
        float motionPitch = -accelInput.z * motionSensitivity * motionPitchMultiplier;
        float motionRoll = -accelInput.x * motionSensitivity * motionRollMultiplier;
        
        // Apply deadzone
        if (Mathf.Abs(motionPitch) < motionDeadzone) motionPitch = 0;
        if (Mathf.Abs(motionRoll) < motionDeadzone) motionRoll = 0;
        
        // Clamp values
        motionPitch = Mathf.Clamp(motionPitch, -1f, 1f);
        motionRoll = Mathf.Clamp(motionRoll, -1f, 1f);
        
        return new Vector2(motionRoll, motionPitch);
    }
    
    void ReadMotionSensors()
    {
        var gamepad = UnityEngine.InputSystem.Gamepad.current;
        if (gamepad == null || !hasMotionSupport) return;
        
        // Mode: Simulated atau Hybrid - gunakan right stick sebagai motion input
        if (motionMode == MotionControlMode.SimulatedOnly || motionMode == MotionControlMode.HybridRightStick)
        {
            // Right stick menjadi motion simulator
            // Ini perfect untuk DS4Windows yang sensor sixaxis-nya tidak terbaca Unity
            if (lookInput.magnitude > 0.1f)
            {
                // Map right stick ke tilt direction
                // X axis = roll (miring kiri/kanan)
                // Y axis = pitch (depan/belakang)
                accelInput = new Vector3(
                    lookInput.x * 0.8f,     // Roll dari right stick X
                    0,
                    -lookInput.y * 0.8f     // Pitch dari right stick Y
                );
            }
            else
            {
                // Smooth return to zero
                accelInput = Vector3.Lerp(accelInput, Vector3.zero, Time.fixedDeltaTime * 5f);
            }
            return;
        }
        
        // Mode: Auto-detect - coba baca sensor asli
        try
        {
            var attitudeSensor = UnityEngine.InputSystem.AttitudeSensor.current;
            var accelerometer = UnityEngine.InputSystem.Accelerometer.current;
            var gyroscope = UnityEngine.InputSystem.Gyroscope.current;
            
            if (attitudeSensor != null)
            {
                // Attitude sensor (orientation sebagai quaternion)
                var attitude = attitudeSensor.attitude.ReadValue();
                
                // Convert quaternion ke euler angles untuk mendapat tilt
                Vector3 euler = attitude.eulerAngles;
                
                // Normalize ke -180 to 180
                if (euler.x > 180) euler.x -= 360;
                if (euler.z > 180) euler.z -= 360;
                
                // Map euler angles ke accelerometer-like input
                accelInput = new Vector3(
                    Mathf.Sin(euler.z * Mathf.Deg2Rad),  // Roll tilt
                    0,
                    Mathf.Sin(euler.x * Mathf.Deg2Rad)   // Pitch tilt
                );
            }
            else if (accelerometer != null)
            {
                // Direct accelerometer reading
                var accel = accelerometer.acceleration.ReadValue();
                accelInput = new Vector3(accel.x, accel.y, accel.z);
            }
            
            if (gyroscope != null)
            {
                // Gyroscope (angular velocity)
                var gyro = gyroscope.angularVelocity.ReadValue();
                gyroInput = new Vector3(gyro.x, gyro.y, gyro.z);
            }
        }
        catch (System.Exception e)
        {
            // Fallback to simulated mode
            if (lookInput.magnitude > 0.1f)
            {
                accelInput = new Vector3(lookInput.x, 0, -lookInput.y) * 0.5f;
            }
            else
            {
                accelInput = Vector3.Lerp(accelInput, Vector3.zero, Time.fixedDeltaTime * 5f);
            }
        }
    }
    
    // Input System Callbacks
    public void OnMove(InputValue value)
    {
        // Hanya terima input jika mode Analog Only atau Hybrid
        if (controlMode == ControlInputMode.AnalogOnly || controlMode == ControlInputMode.Hybrid)
        {
            moveInput = value.Get<Vector2>();
        }
        else // Mouse Only: Abaikan analog move input
        {
            moveInput = Vector2.zero;
        }
    }
    
    public void OnLook(InputValue value)
    {
        // Right stick untuk yaw control (GTA style)
        // ATAU mouse delta untuk kontrol mouse
        Vector2 input = value.Get<Vector2>();
        
        // Detect apakah ini dari mouse (delta lebih besar) atau analog stick (normalized -1 to 1)
        if (Mathf.Abs(input.x) > 2f || Mathf.Abs(input.y) > 2f)
        {
            // Ini dari mouse (delta values besar)
            // Hanya terima jika mode Mouse Only atau Hybrid
            if (controlMode == ControlInputMode.MouseOnly || controlMode == ControlInputMode.Hybrid)
            {
                mouseInput = input;
            }
        }
        else
        {
            // Ini dari analog stick (right stick untuk yaw)
            // Hanya terima jika mode Analog Only atau Hybrid
            if (controlMode == ControlInputMode.AnalogOnly || controlMode == ControlInputMode.Hybrid)
            {
                lookInput = input;
            }
            mouseInput = Vector2.zero; // Reset mouse input
        }
    }
    
    public void OnThrottle(InputValue value)
    {
        // Triggers: RT = positive (speed up), LT = negative (slow down)
        throttleInput = value.Get<float>();
    }
    
    public void OnBoost(InputValue value)
    {
        if (!enableBoost) return;
        
        if (value.isPressed && boostCooldownTimer <= 0 && !isBoosting)
        {
            isBoosting = true;
            boostTimer = boostDuration;
            Debug.Log("🚀 BOOST ACTIVATED!");
        }
    }
    
    // Public methods
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    public float GetAltitude()
    {
        return transform.position.y;
    }
    
    public bool IsBoosting()
    {
        return isBoosting;
    }
    
    public float GetBoostCooldown()
    {
        return boostCooldownTimer;
    }
    
    // Debug visualization
    void DrawDebugInfo()
    {
        // Forward direction
        Debug.DrawRay(transform.position, transform.forward * 10f, Color.blue);
        
        // Up direction
        Debug.DrawRay(transform.position, transform.up * 5f, Color.green);
        
        // Right direction
        Debug.DrawRay(transform.position, transform.right * 5f, Color.red);
        
        // Velocity
        Debug.DrawRay(transform.position, rb.linearVelocity.normalized * 8f, Color.yellow);
    }
    
    void OnDrawGizmos()
    {
        if (!enableAltitudeLimits) return;
        
        // Draw altitude limits
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(0, minAltitude, 0), new Vector3(1000, 0.1f, 1000));
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(0, maxAltitude, 0), new Vector3(1000, 0.1f, 1000));
    }
}
