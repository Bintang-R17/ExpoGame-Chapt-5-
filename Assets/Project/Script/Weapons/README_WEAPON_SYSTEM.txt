======================================
ORBITAL WEAPON SHOOTING SYSTEM
======================================

SETUP INSTRUCTIONS:
==================

1. PLAYER SETUP:
   - Pastikan Player GameObject memiliki:
     * PlayerController script
     * OrbitalWeapons script
   - Set Weapon Prefab di OrbitalWeapons Inspector
   - Set Enemy Layer di "Attack System > Enemy Layer"

2. ENEMY SETUP:
   - Tambahkan EnemyHealth script ke semua Enemy
   - Pastikan Enemy memiliki Tag "Enemy"
   - Set Max Health di Inspector (default: 100)
   - Optional: Tambahkan Health Bar UI Slider

3. WEAPON PREFAB:
   - Buat prefab pedang dengan:
     * Mesh/Model pedang
     * BoxCollider (trigger = true)
     * TrailRenderer (optional)
   - Assign prefab ke OrbitalWeapons.weaponPrefab

CONTROLS:
=========

GAMEPAD:
- Button East (B/Circle) = Shoot Weapon / Punch
- Button South (A/X) = Jump

KEYBOARD:
- Shift = Shoot Weapon / Punch
- Space = Jump
- Q = Toggle Mode (Defense/Combat)

HOW IT WORKS:
=============

1. SHOOTING:
   - Tekan B/Circle (gamepad) atau Shift (keyboard)
   - Pedang akan ditembakkan ke enemy terdekat
   - Auto-targeting ke enemy dalam radius
   - Pedang akan homing (mengikuti target)

2. AMMO SYSTEM:
   - Jumlah pedang = ammo
   - Setiap tembakan mengurangi 1 pedang
   - Saat habis, otomatis reload
   - Reload time: 2 detik (adjustable)

3. DAMAGE:
   - Default damage: 25 per pedang
   - Enemy health: 100
   - Butuh 4 tembakan untuk kill enemy

4. AUTO-RELOAD:
   - Otomatis reload saat pedang habis
   - Semua pedang kembali ke formasi
   - Bisa di-disable di Inspector

PARAMETERS (Inspector):
======================

Attack System:
- Enable Shooting: On/Off shooting system
- Shoot Speed: Kecepatan proyektil (20)
- Shoot Damage: Damage per pedang (25)
- Shoot Cooldown: Delay antar tembakan (0.3s)
- Reload Time: Waktu reload (2s)
- Target Detection Range: Jarak deteksi enemy (30)
- Enemy Layer: Layer untuk enemy
- Auto Reload: Reload otomatis (true)

TROUBLESHOOTING:
================

1. Pedang tidak menembak:
   - Cek "Enable Shooting" = true
   - Cek Weapon Prefab sudah di-assign
   - Cek ada enemy dengan tag "Enemy"

2. Tidak kena enemy:
   - Pastikan enemy punya Collider
   - Pastikan enemy tag = "Enemy"
   - Pastikan enemy ada EnemyHealth script

3. Pedang tidak reload:
   - Cek "Auto Reload" = true
   - Cek tidak ada error di console

4. Target tidak terdeteksi:
   - Cek Enemy Layer sudah diset
   - Cek Target Detection Range cukup besar
   - Cek enemy dalam radius

DEBUG:
======
- Enable "Show Debug Info" untuk visualisasi
- Check Console untuk log shooting
- Lihat Gizmos di Scene View untuk range

==========================================
Created for Astro Bot style gameplay
==========================================
