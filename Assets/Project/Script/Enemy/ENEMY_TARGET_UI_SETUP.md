# Enemy Target UI - Quick Setup

## 🎯 Sistem Baru

**Timing bar sekarang:**

- ✅ **Terus terlihat** selama kamera lock target
- ✅ **Auto-hide** saat kamera free/unlock
- ✅ **Loop animation** (restart setiap 1.5s)
- ✅ **Integrated** dengan TargetingManager

## 📦 Setup Enemy dengan UI

### Langkah 1: Add Component ke Enemy

1. **Pilih Enemy GameObject**
2. **Add Component** → `ShieldCycle` (jika belum ada)
3. **Add Component** → `EnemyTargetUI` (NEW!)

### Langkah 2: Configure EnemyTargetUI

Di Inspector **EnemyTargetUI**:

#### References

- **Shield Cycle**: Auto-assigned (dari GetComponent)

#### UI Settings

- **Auto Show Timing Bar**: ✓ (checked)
- **Timing Bar Duration**: 1.5
- **Perfect Zone Start**: 0.4
- **Perfect Zone End**: 0.6

#### Debug

- **Log State Changes**: ✓ (untuk testing)

### Langkah 3: Configure ShieldCycle

Di Inspector **ShieldCycle**:

#### Cycle Settings

- **Cycle Duration**: 5 (total cycle time)
- **Expose Duration**: 1.5 (vulnerability window)
- **Auto Start**: ✓
- **Loop Cycle**: ✓

#### Perfect Zone Settings

- **Perfect Zone Start**: 0.4
- **Perfect Zone End**: 0.6

## 🎮 Cara Kerja

### Flow Diagram

```
Player locks camera (RT/R2)
    ↓
TargetingManager.GetCurrentTarget() == enemy
    ↓
EnemyTargetUI detects: isTargeted = true
    ↓
EnemyTargetUI.OnTargeted() called
    ↓
TimingBarUI.ShowPersistent() → Bar muncul
    ↓
Bar loop animation 0→1 setiap 1.5s
    ↓
[Bar terus terlihat selama lock]
    ↓
Player attacks → Check timing
    ↓
Perfect/Good/Blocked result
    ↓
[Bar tetap terlihat!]
    ↓
Player unlocks camera (RT/R2 lagi)
    ↓
EnemyTargetUI detects: isTargeted = false
    ↓
EnemyTargetUI.OnUntargeted() called
    ↓
TimingBarUI.Hide() → Bar hilang
```

## ✅ Perbedaan dengan Sistem Lama

| Aspek            | Sistem Lama          | Sistem Baru             |
| ---------------- | -------------------- | ----------------------- |
| **Timing Bar**   | Muncul saat Expose() | Muncul saat target lock |
| **Duration**     | 1.5s lalu hide       | Loop terus selama lock  |
| **Hide Trigger** | Auto setelah 1.5s    | Saat unlock camera      |
| **Control**      | ShieldCycle          | EnemyTargetUI           |

## 🔧 Komponen yang Dimodifikasi

### 1. TimingBarUI.cs

- ✅ Added `ShowPersistent()` method
- ✅ Changed animation to loop (tidak auto-hide)
- ✅ Bar restart setiap cycle selesai

### 2. ShieldCycle.cs

- ✅ Tidak lagi auto-hide timing bar
- ✅ Expose() cek apakah bar sudah active
- ✅ Compatible dengan EnemyTargetUI

### 3. WeaponPlayerController.cs

- ✅ Tidak auto-hide bar setelah attack
- ✅ Bar tetap visible untuk attack berikutnya

### 4. TargetingManager.cs

- ✅ Added `IsLocked()` method
- ✅ Public accessor untuk current target

### 5. EnemyTargetUI.cs (NEW!)

- ✅ Detect saat enemy di-target
- ✅ Auto show/hide timing bar
- ✅ Integrated dengan TargetingManager

## 🐛 Troubleshooting

### Bar tidak muncul saat lock

- ✓ Check EnemyTargetUI component ada di enemy
- ✓ Verify autoShowTimingBar = true
- ✓ Pastikan TimingBarUI GameObject ada di scene
- ✓ Check console log "TARGETED!"

### Bar tidak hilang saat unlock

- ✓ Check TargetingManager.ClearLock() dipanggil
- ✓ Verify EnemyTargetUI.OnUntargeted() running
- ✓ Check console log "Untargeted"

### Bar tidak loop

- ✓ Check UpdateTimingBar() restart startTime
- ✓ Verify isActive = true saat locked

## 📊 Console Output

Saat lock enemy:

```
[EnemyTargetUI] Enemy_1 - TARGETED!
[EnemyTargetUI] Enemy_1 - Timing bar displayed
[TimingBarUI] SHOW for target: Enemy_1
```

Saat unlock enemy:

```
[EnemyTargetUI] Enemy_1 - Untargeted
[TimingBarUI] HIDE
```

Saat attack (Perfect):

```
⚡ PERFECT HIT on Enemy_1 - INSTANT KILL!
[TimingBarUI] Feedback: PERFECT!
[ShieldCycle] Enemy_1 - PERFECT HIT! Instant Kill
```

## 🎯 Summary

**Setup Lengkap:**

1. ✅ Add `EnemyTargetUI` component ke setiap enemy
2. ✅ Timing bar otomatis muncul saat lock
3. ✅ Bar terus terlihat dengan loop animation
4. ✅ Bar hilang saat unlock/free camera

**No manual assignment needed!** Semua auto-detect via Singleton pattern.
