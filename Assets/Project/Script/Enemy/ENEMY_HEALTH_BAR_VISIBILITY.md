# Enemy Health Bar - Target Visibility System

## 🎯 Sistem

**Health bar enemy sekarang:**

- ✅ **Tersembunyi** saat tidak di-target
- ✅ **Muncul** saat player lock camera ke enemy
- ✅ **Hilang** saat player unlock/switch target
- ✅ **Integrated** dengan TargetingManager

## 📦 Setup Enemy dengan Health Bar UI

### Langkah 1: Add Component ke Enemy

1. **Pilih Enemy GameObject**
2. **Add Component** → `EnemyHealth` (jika belum ada)
3. **Add Component** → `EnemyTargetUI` (NEW!)

### Langkah 2: Configure EnemyTargetUI

Di Inspector **EnemyTargetUI**:

#### References

- **Enemy Health**: Auto-assigned (dari GetComponent)
- **Health Bar UI**: Auto-found (dari EnemyHealth children)

#### UI Settings

- **Show Health Bar When Targeted**: ✓ (checked)
- **Hide Health Bar When Untargeted**: ✓ (checked)
- **Always Show Health Bar**: ✗ (uncheck untuk auto-hide)

#### Debug

- **Log State Changes**: ✓ (untuk testing)

### Langkah 3: Verify EnemyHealth

Di Inspector **EnemyHealth**:

#### UI Settings

- **Auto Create Health Bar**: ✓ (jika belum ada health bar)
- **Show Health Bar**: ✓

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
EnemyHealthBarUI.Show() → Health bar muncul
    ↓
[Health bar visible selama locked]
    ↓
Player attacks → Enemy takes damage
    ↓
Health bar updates (berubah merah/kuning/hijau)
    ↓
Player unlocks camera (RT/R2 lagi)
    ↓
EnemyTargetUI detects: isTargeted = false
    ↓
EnemyTargetUI.OnUntargeted() called
    ↓
EnemyHealthBarUI.Hide() → Health bar hilang
```

## ✅ Fitur

| Kondisi                   | Health Bar                               |
| ------------------------- | ---------------------------------------- |
| **Enemy tidak di-target** | Hidden (tersembunyi)                     |
| **Player lock ke enemy**  | Visible (muncul)                         |
| **Enemy kena damage**     | Update warna & nilai                     |
| **Player switch target**  | Hide dari enemy lama, Show di enemy baru |
| **Always Show = true**    | Selalu terlihat (ignore targeting)       |

## 🧪 Testing

1. **Play scene**
2. **Lock camera ke enemy** (tekan RT/R2)
   - ✅ Health bar harus muncul
   - ✅ Console: `[EnemyTargetUI] Enemy_1 - TARGETED!`
3. **Attack enemy**
   - ✅ Health bar update warna/nilai
4. **Unlock camera** (tekan RT/R2 lagi)
   - ✅ Health bar harus hilang
   - ✅ Console: `[EnemyTargetUI] Enemy_1 - UNTARGETED`

## 🐛 Troubleshooting

### Health bar tidak muncul?

1. Check `EnemyHealth` ada di GameObject
2. Check `EnemyHealthBarUI` ada sebagai child GameObject
3. Check `showHealthBarWhenTargeted = true`
4. Check console untuk error

### Health bar tidak hilang?

1. Check `hideHealthBarWhenUntargeted = true`
2. Check `alwaysShowHealthBar = false`
3. Check TargetingManager unlock camera properly

### Health bar tidak update?

1. Check `EnemyHealth.TakeDamage()` dipanggil
2. Check `EnemyHealthBarUI.SetHealth()` works
3. Check `healthBarUI` reference di EnemyTargetUI

## 📌 Catatan

- Health bar system **berbeda** dengan timing bar!
- **Timing bar** = untuk shield timing mechanic (di TimingBarUI Singleton)
- **Health bar** = untuk show enemy HP (di EnemyHealthBarUI per-enemy)
- EnemyTargetUI hanya mengontrol **visibility** health bar, bukan nilainya
- Health value diupdate oleh `EnemyHealth.TakeDamage()` otomatis
