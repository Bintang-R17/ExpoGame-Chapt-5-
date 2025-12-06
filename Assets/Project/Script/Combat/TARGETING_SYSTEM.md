# Targeting Manager - D-Pad Cycling System

## 🎯 Overview

Deterministic lock-on system using **D-pad left/right** to cycle through targets in angular order around the camera forward direction.

## 🎮 Features

- **Sphere Detection**: Physics.OverlapSphere to find targets within radius
- **Angular Sorting**: Targets sorted by signed angle from camera forward
- **D-pad Cycling**: Left/Right to cycle through targets deterministically
- **Cooldown Protection**: 0.18s cooldown prevents input spam
- **Auto-Integration**: Automatically calls CameraFollow.SetLockTarget()

## ⚙️ Setup

### 1. Add TargetingManager to Player

1. Select **Player GameObject**
2. Add Component: **TargetingManager**

### 2. Inspector Settings

#### References (Auto-detected)

- **Player**: Player Transform (auto: self)
- **Main Camera**: Main Camera (auto: Camera.main)
- **Camera Follow**: CameraFollow component (auto-find)

#### Detection Settings

- **Detection Radius**: 12 (meters)
- **Target Layer**: Select "Enemy" layer
- **Update Interval**: 0.2 (seconds between target list updates)

#### Cycling Settings

- **Cycle Cooldown**: 0.18 (seconds)
- **Enable Cycling**: ✓

#### Debug

- **Show Debug Gizmos**: ✓
- **Log Target Changes**: ✓ (console output)

## 🎮 Controls

- **D-pad Left**: Cycle to previous target (counterclockwise)
- **D-pad Right**: Cycle to next target (clockwise)
- **L Key**: Quick toggle lock-on (CameraFollow fallback)

## 🔧 How It Works

### Target Detection

1. **Sphere Query**: `Physics.OverlapSphere` finds targets within radius
2. **Angle Calculation**: `Vector3.SignedAngle` computes angle from camera forward
3. **Sorting**: Targets ordered by angle (-180° to 180°)
4. **Update**: Target list refreshed every 0.2s

### Cycling Logic

```
Targets sorted by angle:
[-150°] Enemy_Left
[-45°]  Enemy_FrontLeft
[0°]    Enemy_Front      ← Camera Forward
[60°]   Enemy_FrontRight
[135°]  Enemy_Right

D-pad Right → Cycle: Front → FrontRight → Right → Left → FrontLeft → Front
D-pad Left  → Reverse cycle
```

### Angular Order

- **Left targets**: Negative angles (-180° to 0°)
- **Right targets**: Positive angles (0° to 180°)
- **Wrap-around**: Last target → First target (seamless loop)

## 📋 Public API

```csharp
// Get current locked target
public Transform GetCurrentTarget()

// Cycle to next target (right)
public void CycleRight()

// Cycle to previous target (left)
public void CycleLeft()

// Set first target in list as default
public void SetDefaultTarget()

// Clear lock and reset
public void ClearLock()

// Get number of available targets
public int GetTargetCount()

// Check if currently locked
public bool HasTarget()

// Get all detected targets
public List<Transform> GetAllTargets()
```

## 🎨 Debug Visualization

When `showDebugGizmos` is enabled:

- **Yellow sphere**: Detection radius around player
- **Blue ray**: Camera forward direction
- **Green spheres**: Available targets
- **Red sphere**: Current locked target
- **Gray/Red lines**: Player to targets (red = current)
- **Labels**: Target name, angle, distance

## 📊 Console Logging

Example output with `logTargetChanges` enabled:

```
[Targeting] Selected: Enemy_02 | Index: 1/4 | Cooldown: 0.18s
[Targeting] Selected: Enemy_03 | Index: 2/4 | Cooldown: 0.18s
[Targeting] Lock cleared - no targets available
```

## 🔗 Integration Example

### Automatic (Built-in)

TargetingManager automatically integrates with CameraFollow:

- Calls `CameraFollow.SetLockTarget()` on successful cycle
- Calls `CameraFollow.ClearLock()` when no targets

### Manual Control

```csharp
TargetingManager targeting = GetComponent<TargetingManager>();

// Manually cycle targets
targeting.CycleRight();
targeting.CycleLeft();

// Get current target
Transform currentTarget = targeting.GetCurrentTarget();

// Check target status
if (targeting.HasTarget())
{
    int count = targeting.GetTargetCount();
    Debug.Log($"Locked onto target {count} available");
}

// Set default target (first in list)
targeting.SetDefaultTarget();

// Clear all locks
targeting.ClearLock();
```

## ⚡ Performance

- **Sphere Query**: Cached and updated every 0.2s (not every frame)
- **Sorting**: LINQ OrderBy on detected targets only
- **Cooldown**: Time.time comparison (no coroutines)
- **Angular Calculation**: Vector3.SignedAngle (efficient built-in)

## 🎯 Acceptance Criteria

✅ **D-pad left/right cycles through nearby targets in angular order**

- Targets sorted by signed angle from camera forward
- Cycles wrap around (first ↔ last)

✅ **Console logs show selected target name and cooldown**

- Format: `[Targeting] Selected: {name} | Index: {n}/{total} | Cooldown: {time}s`
- Cooldown prevents rapid switches (0.18s)

✅ **CameraManager receives SetLockTarget on every successful cycle**

- Automatic integration via `cameraFollow.SetLockTarget(target)`
- Camera smoothly locks onto cycled targets

✅ **No targets → ClearLock()**

- Automatically clears lock when no valid targets
- Returns camera to Free mode

## 🐛 Troubleshooting

### D-pad not cycling

- ✓ Check `enableCycling` is true
- ✓ Check D-pad/stick input reaches threshold (±0.8)
- ✓ Wait for cooldown to expire (0.18s)

### No targets detected

- ✓ Verify enemies have correct Layer ("Enemy")
- ✓ Check `detectionRadius` is large enough
- ✓ Ensure enemies have Colliders

### Targets jump randomly

- ✓ Disable `logTargetChanges` for cleaner console
- ✓ Increase `updateInterval` for less frequent updates
- ✓ Check targets aren't being destroyed/spawned rapidly

### Camera not locking

- ✓ Verify `cameraFollow` reference is assigned
- ✓ Check CameraFollow component exists on Camera
- ✓ Ensure `targetLayer` matches enemy layer

## 🎓 Advanced Usage

### Custom Input Binding

Replace D-pad input with custom action:

```csharp
// In TargetingManager.cs, replace HandleCycleInput():
if (yourCustomCycleRightAction.triggered)
{
    CycleRight();
    nextCycleTime = Time.time + cycleCooldown;
}
```

### Filter by Distance

Add distance filtering to target detection:

```csharp
// In UpdateTargetList(), after angle calculation:
if (info.distance > maxLockDistance)
    continue; // Skip distant targets
```

### Priority Targeting

Sort by custom criteria (e.g., threat level):

```csharp
// Replace OrderBy in UpdateTargetList():
sortedTargets = sortedTargets
    .OrderBy(t => GetThreatLevel(t.transform))
    .ThenBy(t => t.angle)
    .ToList();
```

## 🔄 Integration with Combat

```csharp
public class CombatSystem : MonoBehaviour
{
    private TargetingManager targeting;

    void Start()
    {
        targeting = GetComponent<TargetingManager>();
    }

    void AttackCurrentTarget()
    {
        Transform target = targeting.GetCurrentTarget();
        if (target != null)
        {
            // Attack logic here
            Debug.Log($"Attacking {target.name}");
        }
    }
}
```
