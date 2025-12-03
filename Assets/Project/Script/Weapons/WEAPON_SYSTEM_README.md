# Weapon System Documentation

Sistem senjata modular untuk Unity (URP) dengan arsitektur berbasis interface.

## 📁 Struktur Folder

```
Assets/
├── Scripts/
│   ├── Weapons/
│   │   ├── IAttack.cs              # Interface untuk semua senjata
│   │   ├── IDamageable.cs          # Interface untuk entity yang bisa di-damage
│   │   ├── WeaponData.cs           # ScriptableObject untuk data senjata
│   │   ├── RifleAttack.cs          # Implementasi rifle
│   │   ├── SwordAttack.cs          # Implementasi sword
│   │   └── Bullet.cs               # Projectile untuk rifle
│   ├── Player/
│   │   └── WeaponPlayerController.cs  # Controller untuk player
│   └── Enemy/
│       └── Enemy.cs                # Contoh enemy dengan IDamageable
```

## 🎯 Komponen Utama

### 1. IAttack Interface

Interface dasar untuk semua jenis serangan senjata.

```csharp
public interface IAttack
{
    void Attack();
}
```

### 2. WeaponData ScriptableObject

Menyimpan data senjata yang dapat dikonfigurasi dari editor.

**Properties:**

- `weaponName` (string) - Nama senjata
- `damage` (float) - Damage yang diberikan
- `attackCooldown` (float) - Cooldown antar serangan (detik)
- `attackRange` (float) - Jarak serangan maksimal

### 3. RifleAttack

Implementasi senjata tipe ranged (rifle).

**Features:**

- Menembakkan bullet projectile
- Cooldown system
- Audio support
- Configurable bullet speed

**Required Setup:**

- Fire Point (Transform) - Posisi spawn bullet
- Bullet Prefab (GameObject) - Prefab bullet dengan component Bullet.cs

### 4. SwordAttack

Implementasi senjata tipe melee (sword).

**Features:**

- Sphere collision detection
- Animation trigger support
- Visual hitbox gizmo di editor
- LayerMask filtering

**Required Setup:**

- Animator dengan trigger "Attack"
- Hitbox Center (Transform) - Center point untuk deteksi
- Enemy Layer - Layer untuk enemy yang bisa di-hit

### 5. Bullet

Projectile untuk rifle dengan physics-based movement.

**Features:**

- Auto-destroy setelah lifetime
- Collision/Trigger detection
- Optional hit effects
- Trail renderer support

## 🎮 Setup Instruksi

### A. Membuat WeaponData Asset

1. Klik kanan di Project window
2. Pilih **Create > Weapon System > Weapon Data**
3. Atur properties:

   - **Rifle Example:**

     - Name: "Assault Rifle"
     - Damage: 25
     - Attack Cooldown: 0.1
     - Attack Range: 100

   - **Sword Example:**
     - Name: "Combat Sword"
     - Damage: 50
     - Attack Cooldown: 0.5
     - Attack Range: 2.5

### B. Setup Rifle Weapon

1. Buat GameObject kosong bernama "Rifle"
2. Tambahkan component `RifleAttack`
3. Assign:
   - Weapon Data (ScriptableObject yang dibuat)
   - Fire Point (child transform di ujung barrel)
   - Bullet Prefab
   - Bullet Speed (default: 20)
4. Optional: Tambahkan AudioSource dan fire sound

### C. Setup Bullet Prefab

1. Buat GameObject (Sphere/Capsule kecil)
2. Tambahkan component:
   - Bullet.cs
   - Rigidbody (Use Gravity: OFF)
   - Collider (Is Trigger: ON)
3. Scale ke ukuran kecil (0.2, 0.2, 0.5)
4. Optional: Tambahkan Trail Renderer

### D. Setup Sword Weapon

1. Buat GameObject "Sword" dengan model 3D
2. Tambahkan component `SwordAttack`
3. Assign:
   - Weapon Data
   - Animator (dengan parameter trigger "Attack")
   - Hitbox Center (biasanya di depan player)
   - Enemy Layer
4. Atur Hitbox Radius (visual gizmo merah di editor)

### E. Setup Player Controller

1. Pada Player GameObject, tambahkan `WeaponPlayerController`
2. Assign references:
   - Rifle Weapon (GameObject dengan RifleAttack)
   - Sword Weapon (GameObject dengan SwordAttack)
3. Set "Use New Input System" = true/false
4. Senjata akan di-parent sebagai child dari player

### F. Setup Enemy

1. Buat GameObject enemy
2. Tambahkan component `Enemy`
3. Atur layer ke "Enemy" (sesuai dengan sword LayerMask)
4. Set Max Health dan visual properties

## ⌨️ Controls

### Input System (New)

- **Attack**: Punch button (mouse left/gamepad button)
- **Switch to Rifle**: 1 atau Numpad 1
- **Switch to Sword**: 2 atau Numpad 2

### Legacy Input

- **Attack**: Left Mouse Button atau Fire1
- **Switch to Rifle**: 1 atau Numpad 1
- **Switch to Sword**: 2 atau Numpad 2

## 🔧 Cara Menambah Senjata Baru

### 1. Buat Script Senjata Baru

```csharp
using UnityEngine;

public class BowAttack : MonoBehaviour, IAttack
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private GameObject arrowPrefab;

    private float nextAttackTime = 0f;

    public void Attack()
    {
        if (Time.time < nextAttackTime) return;

        // Spawn arrow
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);

        // Set arrow properties...

        nextAttackTime = Time.time + weaponData.attackCooldown;
    }
}
```

### 2. Tambahkan ke PlayerController

Di `WeaponPlayerController.cs`:

```csharp
[SerializeField] private BowAttack bowWeapon;

public void SwitchToBow()
{
    if (bowWeapon != null)
    {
        EquipWeapon(bowWeapon);
    }
}
```

### 3. Bind Input (Optional)

Tambahkan di `HandleWeaponSwitching()`:

```csharp
if (Input.GetKeyDown(KeyCode.Alpha3))
{
    SwitchToBow();
}
```

## 📊 Layer Setup

Pastikan layers berikut ada di project:

- **Default** - Untuk environment/world objects
- **Player** - Untuk player (bullet akan ignore ini)
- **Enemy** - Untuk enemy yang bisa di-damage

## 🎨 Animation Setup (Sword)

Animator Controller harus memiliki:

- Parameter Type: **Trigger**
- Parameter Name: **"Attack"**
- Transition ke Attack animation state

## ⚡ Tips & Best Practices

1. **Modular Design**: Setiap senjata independen dan bisa ditambahkan tanpa mengubah kode lain
2. **ScriptableObject**: Gunakan WeaponData untuk balance tanpa recompile
3. **Interface**: Mudah extend dengan weapon type baru (Grenade, Magic, dll)
4. **Pooling**: Untuk production, implement object pooling untuk bullet
5. **VFX**: Tambahkan particle effects di RifleAttack dan SwordAttack
6. **Audio**: Gunakan AudioSource dengan PlayOneShot untuk sound effects

## 🐛 Troubleshooting

### Bullet tidak spawn

- Cek Fire Point sudah di-assign
- Pastikan Bullet Prefab ada component Bullet.cs dan Rigidbody

### Sword tidak hit enemy

- Cek Enemy Layer sudah benar
- Pastikan enemy ada component yang implement IDamageable
- Lihat red sphere gizmo di Scene view untuk hitbox range

### Weapon tidak switch

- Cek reference di WeaponPlayerController sudah di-assign
- Pastikan weapon GameObject aktif di hierarchy

### Input tidak berfungsi

- Cek "Use New Input System" sesuai dengan project
- Untuk New Input System, pastikan PlayerInput.inputactions ada
- Untuk Legacy, pastikan Input Manager settings correct

## 📝 Compatibility

- Unity 2021.3 LTS atau lebih baru
- Universal Render Pipeline (URP)
- Input System Package (untuk new input)
- Compatible dengan Unity 2022, 2023, 6

## 🚀 Future Enhancements

Ide untuk pengembangan lebih lanjut:

- Weapon durability system
- Ammunition system untuk rifle
- Combo system untuk sword
- Weapon upgrade/enchantment
- Multiple projectile patterns
- Weapon switching animation
- Recoil/kick system
- Aim down sights (ADS)
- Critical hit system

## 📄 License

Script ini dibuat untuk tujuan edukasi dan dapat digunakan secara bebas dalam project Anda.
