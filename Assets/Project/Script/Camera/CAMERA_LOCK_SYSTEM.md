# Camera Lock-On System

## 🎮 Features

### Two Camera Modes:

1. **Free Mode** - Manual camera control via right stick/mouse
2. **Locked Mode** - Auto-focus camera on current target

## ⚙️ Setup

### Camera Settings (Inspector)

#### Lock-On Settings

- **Camera Mode**: Free (default)
- **Lock Transition Speed**: 0.15 (0.12-0.2s for smooth transition)
- **Target Layer**: Select "Enemy" layer
- **Lock On Range**: 20 (max distance to lock targets)

#### Debug

- **Show Debug Gizmos**: ✓ (shows lock range & target)
- **Lock Gizmo Color**: Red

## 🎯 Controls

- **L Key**: Toggle lock-on (test binding)
- **Right Stick / Mouse**: Free camera control (Free mode only)

## 📋 API Methods

### Public Methods

```csharp
// Toggle between Free and Locked modes
public void ToggleLockMode()

// Set specific target and enter Locked mode
public void SetLockTarget(Transform target)

// Clear lock and return to Free mode
public void ClearLock()

// Get current camera mode
public CameraMode GetCameraMode()

// Get current lock target
public Transform GetLockTarget()

// Check if currently locked
public bool IsLocked()
```

## 🔧 How It Works

### Free Mode

- Player controls camera rotation via right stick or mouse
- Smooth orbit around player target
- Pitch clamped between min/max angles

### Locked Mode

- Camera auto-rotates to face current lock target
- Uses `Quaternion.Slerp` for smooth rotation transitions
- Automatically returns to Free mode if target is lost
- Maintains smooth position follow via `Vector3.SmoothDamp`

### Lock Target Selection

- Press L to toggle lock
- Finds nearest enemy within `lockOnRange`
- Filters by `targetLayer` (Enemy layer)
- Visual feedback via debug gizmos (red sphere & lines)

## 🎨 Visual Debug

When `showDebugGizmos` is enabled:

- **Yellow sphere**: Lock-on detection range around player
- **Red line**: Camera to locked target
- **Red sphere**: Current lock target position
- **Cyan line**: Player to locked target

## ⚡ Performance

- Smooth transitions: 0.12-0.2s (configurable)
- No jitter during mode switches
- Efficient overlap sphere for target detection
- Safe null checks prevent exceptions

## 🔗 Integration Example

```csharp
// From another script (e.g., combat system)
CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();

// Lock onto specific enemy
if (enemyTransform != null)
{
    cameraFollow.SetLockTarget(enemyTransform);
}

// Check if locked
if (cameraFollow.IsLocked())
{
    Transform target = cameraFollow.GetLockTarget();
    // Do something with locked target
}

// Clear lock
cameraFollow.ClearLock();
```

## ✅ Acceptance Criteria

- ✓ Pressing L locks onto nearest enemy (if in range)
- ✓ Free mode allows manual camera rotation
- ✓ Smooth transitions without jitter (Slerp/SmoothDamp)
- ✓ Debug gizmos show current target
- ✓ Works with Enemy layer
- ✓ No exceptions or null reference errors
