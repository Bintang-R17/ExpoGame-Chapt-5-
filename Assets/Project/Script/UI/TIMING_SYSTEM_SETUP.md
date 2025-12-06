# Shield/Timing Window System - Setup Guide

## 🎯 Overview

Contextual timing window system for enemy shield phases. Perfect timing = instant kill, other results vary.

## 📦 Components Created

### 1. TimingBarUI.cs

- Singleton timing bar with color zones (red/yellow/green)
- Shows when enemy calls `Expose()`
- Returns `TimingResult`: Perfect, Good, or Miss
- Auto-hides after duration expires

### 2. ShieldCycle.cs (Enemy Component)

- Controls shield active/vulnerable phases
- Calls `TimingBarUI.Show()` during expose window
- Handles Perfect/Good/Blocked hit callbacks
- Auto-cycling with configurable durations

### 3. WeaponPlayerController Integration

- Checks for active timing window on attack
- Calls appropriate hit handler based on timing result
- Perfect = instant kill, Good = 50% damage, Blocked = no damage + counter

## 🛠️ Setup Instructions

### Step 1: Create TimingBarUI in Scene

1. **Create UI GameObject**:

   - Right-click Hierarchy → UI → Canvas (if not exists)
   - Right-click Canvas → UI → Slider
   - Rename to "TimingBarUI"

2. **Configure Slider**:

   - Direction: Bottom to Top (vertical)
   - Min Value: 0
   - Max Value: 1
   - Value: 0
   - Whole Numbers: Off

3. **Add Component**: `TimingBarUI` script

4. **Assign References**:

   - **Timing Slider**: Drag the Slider component
   - **Fill Image**: Drag Fill/Fill image
   - **Background Image**: Drag Background image
   - **Perfect Zone Indicator**: Create child Image for perfect zone visual
   - **Feedback Text**: TextMeshPro for "PERFECT!"/"GOOD"/"BLOCKED!" messages

5. **Configure Colors**:

   - Miss Color: Red (1, 0, 0, 0.8)
   - Good Color: Yellow (1, 1, 0, 0.8)
   - Perfect Color: Green (0, 1, 0, 0.8)
   - Background Color: Dark (0.1, 0.1, 0.1, 0.8)

6. **Position**:

   - Anchor: Center-Right
   - Position: X=800, Y=0 (right side of screen)
   - Scale: Adjust for visibility

7. **Initially Hide**: Set GameObject active = false

### Step 2: Add ShieldCycle to Enemy

1. **Select Enemy GameObject** in Hierarchy

2. **Add Component**: `ShieldCycle` script

3. **Configure Cycle Settings**:

   - **Cycle Duration**: 5s (total cycle time)
   - **Expose Duration**: 1.5s (vulnerability window)
   - **Auto Start**: ✓
   - **Loop Cycle**: ✓

4. **Perfect Zone Settings**:

   - **Perfect Zone Start**: 0.4 (40% of bar)
   - **Perfect Zone End**: 0.6 (60% of bar)

5. **Optional - Shield Visual**:

   - Create child GameObject "Shield" with sphere mesh
   - Scale: 1.2x enemy size
   - Material: Transparent with emission
   - Drag to **Shield Visual** field

6. **Optional - Audio**:
   - Assign AudioSource component
   - **Shield Break Sound**: Expose sound effect
   - **Shield Restore Sound**: Shield up sound effect

### Step 3: Verify WeaponPlayerController

**Already integrated!** Check that Player has:

- WeaponPlayerController component ✓
- RifleAttack and SwordAttack references assigned ✓

The system will automatically check for timing windows on attack.

## 🎮 How It Works

### Flow Diagram

```
Enemy ShieldCycle starts
    ↓
Shield Active (protected)
    ↓
After (cycleDuration - exposeDuration) seconds
    ↓
ShieldCycle.Expose() called
    ↓
TimingBarUI.Show(enemy, 1.5s, 0.4, 0.6)
    ↓
Bar animates 0 → 1 over 1.5 seconds
    ↓
Player presses Attack button
    ↓
WeaponPlayerController checks timing
    ↓
┌─────────────────────────────┐
│ Check current bar value:    │
│ - In perfect zone (0.4-0.6) │
│   → OnPerfectHit()          │
│   → Instant kill            │
│                             │
│ - Near perfect (±0.15)      │
│   → OnGoodHit()             │
│   → 50% damage              │
│                             │
│ - Outside zones             │
│   → OnBlockedHit()          │
│   → No damage + counter     │
└─────────────────────────────┘
```

### Timing Zones

```
Bar Progress:  [0.0 -------- 0.4 ==== 0.6 -------- 1.0]
               |    MISS     | PERFECT |   MISS    |
               | GOOD (0.25) |         | GOOD(0.75)|
```

## 🎯 Acceptance Criteria Testing

### Test 1: Timing Bar Appears

**Steps:**

1. Play scene
2. Wait for enemy ShieldCycle to call Expose()
3. **Expected**: Timing bar appears on screen, animates upward

**Console Output:**

```
[ShieldCycle] Enemy_1 - EXPOSED!
  → Duration: 1.5s
  → Perfect Zone: 0.4-0.6
[TimingBarUI] SHOW for target: Enemy_1
```

### Test 2: Perfect Hit (Instant Kill)

**Steps:**

1. Wait for expose window
2. Press Attack when bar is in GREEN zone (middle)
3. **Expected**: "PERFECT!" text, enemy dies immediately

**Console Output:**

```
[TimingBarUI] Feedback: PERFECT!
⚡ PERFECT HIT on Enemy_1 - INSTANT KILL!
[ShieldCycle] Enemy_1 - PERFECT HIT! Instant Kill
```

### Test 3: Good Hit (Partial Damage)

**Steps:**

1. Wait for expose window
2. Press Attack when bar is in YELLOW zone (near perfect)
3. **Expected**: "GOOD" text, enemy takes 50% damage

**Console Output:**

```
[TimingBarUI] Feedback: GOOD
✓ GOOD HIT on Enemy_1 - Shield damaged
[ShieldCycle] Enemy_1 - GOOD HIT! Shield damaged: 25
```

### Test 4: Blocked Hit (No Damage)

**Steps:**

1. Wait for expose window
2. Press Attack when bar is in RED zone (too early/late)
3. **Expected**: "BLOCKED!" text, no damage, counter attack

**Console Output:**

```
[TimingBarUI] Feedback: BLOCKED!
✗ BLOCKED by Enemy_1 - No damage!
[ShieldCycle] Enemy_1 - BLOCKED! No damage
[ShieldCycle] Enemy_1 triggered COUNTER ATTACK!
```

### Test 5: Attack Outside Window

**Steps:**

1. Attack enemy when shield is active (no timing bar)
2. **Expected**: Normal attack, damage applied normally

**Console Output:**

```
Hit Enemy_1 for 25 damage!
```

## 🎨 Visual Customization

### Timing Bar Colors

```csharp
// In TimingBarUI Inspector
missColor = new Color(1f, 0f, 0f, 0.8f);     // Red
goodColor = new Color(1f, 1f, 0f, 0.8f);     // Yellow
perfectColor = new Color(0f, 1f, 0f, 0.8f);  // Green
```

### Shield Visual Effects

```csharp
// In ShieldCycle Inspector
shieldActiveColor = new Color(0f, 0.5f, 1f, 0.5f);      // Blue shield
shieldVulnerableColor = new Color(1f, 0.5f, 0f, 0.3f);  // Orange vulnerable
```

### Perfect Zone Adjustment

```csharp
// Easier (wider perfect zone)
perfectZoneStart = 0.35f;
perfectZoneEnd = 0.65f;

// Harder (narrower perfect zone)
perfectZoneStart = 0.45f;
perfectZoneEnd = 0.55f;
```

## ⚙️ Tunable Parameters

### Difficulty Presets

#### Easy Mode

```
Cycle Duration: 6s
Expose Duration: 2.0s
Perfect Zone: 0.35 - 0.65 (30% width)
Good Zone: ±0.2
```

#### Normal Mode (Default)

```
Cycle Duration: 5s
Expose Duration: 1.5s
Perfect Zone: 0.4 - 0.6 (20% width)
Good Zone: ±0.15
```

#### Hard Mode

```
Cycle Duration: 4s
Expose Duration: 1.0s
Perfect Zone: 0.45 - 0.55 (10% width)
Good Zone: ±0.1
```

## 🐛 Troubleshooting

### Timing bar doesn't appear

- ✓ Check TimingBarUI is in scene and active (will auto-hide)
- ✓ Verify ShieldCycle component is on enemy
- ✓ Check console for "EXPOSED!" message
- ✓ Ensure autoStart = true on ShieldCycle

### Perfect hit doesn't kill enemy

- ✓ Check enemy has EnemyHealth component
- ✓ Verify OnPerfectHit() is called (check console)
- ✓ Ensure damage is high enough (999999)

### Attacks always miss timing

- ✓ Check perfect zone settings (not too narrow)
- ✓ Verify bar animation speed matches expose duration
- ✓ Look at console timing feedback

### Counter attack doesn't trigger

- ✓ Implement TriggerCounter() in ShieldCycle (currently just logs)
- ✓ Add counter attack animation/damage logic

## 🔗 Integration with Existing Systems

### Camera Lock-On

When enemy is locked:

```csharp
// TargetingManager automatically tracks target
// TimingBarUI shows for locked enemy
// Bar follows enemy even if moving
```

### Multiple Enemies

- Only ONE timing window active at a time
- Last enemy to call Expose() takes priority
- Previous timing bars auto-hide

### Boss Fights

```csharp
// In boss script:
ShieldCycle cycle = GetComponent<ShieldCycle>();

// Phase 1: Slow cycle
cycle.cycleDuration = 8f;
cycle.exposeDuration = 2f;

// Phase 2: Faster cycle
cycle.cycleDuration = 4f;
cycle.exposeDuration = 1f;

// Restart cycle with new settings
cycle.StopCycle();
cycle.StartCycle();
```

## 📈 Future Enhancements

- [ ] Different timing patterns per enemy type
- [ ] Combo system (multiple perfect hits = bonus)
- [ ] Shield health bars (multiple good hits to break)
- [ ] Counter attack animations and player damage
- [ ] Audio feedback for timing zones
- [ ] Particle effects on perfect hits
- [ ] UI animations (shake, flash, zoom)

## 🎯 Summary

**System Complete!** All components ready:
✅ TimingBarUI with color zones
✅ ShieldCycle with auto-cycling
✅ WeaponPlayerController integration
✅ Perfect/Good/Blocked hit handlers
✅ Debug logging for testing

**Next Steps:**

1. Create TimingBarUI in scene
2. Add ShieldCycle to test enemy
3. Play and test timing windows!
