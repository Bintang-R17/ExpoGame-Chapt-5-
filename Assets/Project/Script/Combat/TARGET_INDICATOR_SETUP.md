# Target Indicator System - Setup Guide

## 🎯 Overview

Visual ring indicator that appears under locked target with object pooling for performance.

## 📦 Components

### 1. TargetIndicator.cs

- Follows target position with Y offset
- Billboard effect (always faces camera)
- Auto-rotate animation
- Pulse scale animation

### 2. TargetIndicatorPool.cs

- Object pooling (no Instantiate/Destroy spam)
- Auto-expand when needed
- Pool statistics and debugging

### 3. TargetingManager Integration

- Automatic indicator spawning on target lock
- Returns indicator to pool on target change
- Clears indicator when lock released

## 🎨 Create Indicator Prefab

### Option A: Simple Quad Ring (Recommended)

1. **Create GameObject**: "TargetIndicator_Ring"
2. **Add Quad**:

   - Right-click → 3D Object → Quad
   - Rotation: X = 90 (flat on ground)
   - Scale: (2, 2, 2)

3. **Material Setup**:

   - Create Material: "TargetRing_Mat"
   - Shader: **Unlit/Transparent**
   - Rendering Mode: **Transparent**
   - Texture: Ring texture (PNG with transparency)
   - Color: White with Alpha 0.8

4. **Add Component**: `TargetIndicator` script

### Option B: Sprite Billboard

1. **Create GameObject**: "TargetIndicator_Sprite"
2. **Add Sprite Renderer**:
   - Sprite: Ring sprite asset
   - Material: Unlit/Transparent
   - Sorting Layer: Above ground
3. **Add Component**: `TargetIndicator` script

### Option C: Particle System (Advanced)

1. **Create GameObject**: "TargetIndicator_Particles"
2. **Add Particle System**:
   - Shape: Circle
   - Emission: Continuous
   - Renderer: Billboard
3. **Add Component**: `TargetIndicator` script

## 🛠️ Setup Instructions

### 1. Create Indicator Prefab

```
Assets/
└── Prefabs/
    └── Combat/
        └── TargetIndicatorRing.prefab
```

**Prefab Structure:**

```
TargetIndicatorRing (GameObject)
├── TargetIndicator (Component)
└── Quad (MeshRenderer)
    └── Material: TargetRing_Mat (Unlit/Transparent)
```

### 2. Setup TargetingManager

1. **Select Player GameObject**
2. **Find TargetingManager Component**
3. **Configure Visual Indicator Section**:

#### References

- **Indicator Pool**: Auto-created (leave empty)

#### Visual Indicator Settings

- **Show Indicator**: ✓
- **Indicator Y Offset**: 0.1 (height above ground)

### 3. Setup IndicatorPool

The pool is auto-created, but you can create manually:

1. **Create Child GameObject** under Player: "IndicatorPool"
2. **Add Component**: `TargetIndicatorPool`
3. **Configure Settings**:

#### Pool Settings

- **Indicator Prefab**: Drag your TargetIndicatorRing prefab
- **Initial Pool Size**: 3
- **Max Pool Size**: 10
- **Auto Expand**: ✓

#### Debug

- **Log Pool Activity**: ✓ (for testing)

## 🎨 Indicator Component Settings

### TargetIndicator Inspector

#### Settings

- **Y Offset**: 0.1 (distance above ground)
- **Billboard**: ✓ (always face camera)
- **Rotation Speed**: 45 (degrees per second)
- **Auto Rotate**: ✓ (spin animation)

#### Scale Animation

- **Enable Pulse**: ✓
- **Pulse Speed**: 2 (frequency)
- **Pulse Amount**: 0.1 (scale variation 0-1)

## 🎨 Material Setup

### Create Ring Texture (Photoshop/GIMP)

1. **New Image**: 512x512, transparent background
2. **Draw Ring**:
   - Outer circle: 256px radius
   - Inner circle: 200px radius
   - Color: White (#FFFFFF)
   - Add glow/gradient for effect
3. **Export**: PNG with transparency
4. **Import to Unity**:
   - Texture Type: Sprite (2D and UI)
   - Alpha Is Transparency: ✓

### Unity Material Settings

```
Material: TargetRing_Mat
├── Shader: Unlit/Transparent
├── Rendering Mode: Transparent
├── Main Maps
│   ├── Base Map: RingTexture.png
│   └── Tint: White (255, 255, 255, 204)
└── Advanced Options
    └── Queue: Transparent
```

## 📊 How It Works

### Pooling Flow

```
Target Selected:
1. TargetingManager.SelectCurrentTarget()
2. UpdateIndicator(target) called
3. Return previous indicator to pool (if exists)
4. Get new indicator from pool
5. indicator.SetTarget(target)
6. Indicator follows target in Update()

Target Changed:
1. Previous indicator returned to pool
2. New indicator retrieved from pool
3. No Instantiate/Destroy overhead

Target Cleared:
1. ClearIndicator() called
2. Indicator returned to pool
3. GameObject.SetActive(false)
```

### Performance Benefits

- **No GC spikes**: Reuses existing GameObjects
- **Fast switching**: No instantiation delay
- **Scalable**: Auto-expands pool if needed
- **Memory efficient**: Fixed pool size

## 🎯 Acceptance Criteria

✅ **Ring appears under locked target's feet**

- Indicator spawns at `target.position + Vector3.up * yOffset`
- Follows target smoothly in Update()

✅ **Changing target moves ring**

- Old indicator returned to pool
- New indicator spawned from pool
- Seamless transition

✅ **Releasing lock hides ring**

- Indicator returned to pool via `ReturnIndicator()`
- GameObject deactivated, not destroyed

✅ **No Instantiate/Destroy on each change**

- Pool pre-warmed with 3 indicators
- Reuses existing instances
- Auto-expands up to max size (10)

## 🐛 Troubleshooting

### Indicator not showing

- ✓ Check `showIndicator` is enabled in TargetingManager
- ✓ Verify prefab assigned in IndicatorPool
- ✓ Check indicator material is visible (not culled)
- ✓ Ensure Y offset > 0

### Indicator doesn't follow target

- ✓ Check TargetIndicator.Update() is running
- ✓ Verify target Transform is valid
- ✓ Check indicator GameObject is active

### Pool exhausted warning

- ✓ Increase `maxPoolSize` in IndicatorPool
- ✓ Enable `autoExpand`
- ✓ Check indicators are being returned properly

### Performance issues

- ✓ Reduce `pulseSpeed` and disable animations
- ✓ Use simpler material (no transparency)
- ✓ Decrease `initialPoolSize`

## 🎨 Visual Customization

### Color Variants

```csharp
// Red indicator for enemies
material.color = new Color(1f, 0f, 0f, 0.8f);

// Green indicator for allies
material.color = new Color(0f, 1f, 0f, 0.8f);

// Pulsing color
float pulse = Mathf.PingPong(Time.time, 1f);
material.color = Color.Lerp(Color.red, Color.yellow, pulse);
```

### Size Variations

```csharp
// Larger indicator
transform.localScale = Vector3.one * 3f;

// Distance-based scaling
float distance = Vector3.Distance(camera.position, transform.position);
float scale = Mathf.Lerp(1f, 2f, distance / maxDistance);
```

### Animation Presets

#### Fast Spin

```
Rotation Speed: 180
Pulse Speed: 4
```

#### Slow Pulse

```
Rotation Speed: 30
Pulse Speed: 1
Pulse Amount: 0.2
```

#### Static

```
Auto Rotate: ✗
Enable Pulse: ✗
```

## 🔗 Integration Example

```csharp
// Manual indicator control
TargetingManager targeting = GetComponent<TargetingManager>();

// Disable auto indicator
// (Set showIndicator = false in Inspector)

// Manually get/return indicators
TargetIndicatorPool pool = GetComponentInChildren<TargetIndicatorPool>();
TargetIndicator indicator = pool.GetIndicator();
indicator.SetTarget(enemyTransform);

// Later...
pool.ReturnIndicator(indicator);
```

## 📈 Pool Statistics

```csharp
TargetIndicatorPool pool = GetComponentInChildren<TargetIndicatorPool>();

int active, available, total;
pool.GetPoolStats(out active, out available, out total);

Debug.Log($"Pool: {active} active, {available} available, {total} total");
```
