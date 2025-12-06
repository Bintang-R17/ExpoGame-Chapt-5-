# Attack Timing Bar UI - Setup Guide

## 📋 Overview

UI bar vertikal di kanan layar untuk menampilkan timing window saat menyerang enemy. Bar bergerak dari atas ke bawah dengan zona "sweet spot" untuk timing optimal.

---

## 🎨 UI Setup di Unity

### 1. Create Canvas (jika belum ada)

1. Right-click di Hierarchy → **UI > Canvas**
2. Set Canvas Scaler:
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1920 x 1080**
   - Match: **0.5** (balance width/height)

### 2. Create Timing Bar GameObject

```
Canvas
└── AttackTimingBar (Panel)
    ├── Background (Image)
    ├── Fill Area (Empty GameObject)
    │   └── Fill (Image)
    ├── SweetSpot (Image)
    └── StatusText (TextMeshProUGUI)
```

#### AttackTimingBar (Panel)

- **Position:** Anchored ke kanan layar
- **Anchor:** Right-Middle (X: 1, Y: 0.5)
- **Pivot:** (1, 0.5)
- **Pos X:** -50 (dari kanan)
- **Pos Y:** 0 (center vertical)
- **Width:** 40
- **Height:** 300
- Component: **AttackTimingBar.cs**

#### Background (Image)

- **Anchor:** Stretch all
- **Pos:** (0, 0, 0)
- **Color:** Dark gray (R:26, G:26, B:26, A:200)
- **Image Type:** Sliced (optional border)

#### Fill Area → Fill (Image)

- **Anchor:** Stretch all
- **Color:** Cyan (R:77, G:204, B:255, A:255)
- **Fill Method:** Vertical (Bottom to Top)

#### SweetSpot (Image)

- **Anchor:** Middle-Center
- **Width:** 40 (sama dengan bar)
- **Height:** 60 (sweet spot zone)
- **Color:** Yellow with transparency (R:255, G:255, B:0, A:100)
- **Pos Y:** Akan di-set oleh script

#### StatusText (TextMeshProUGUI)

- **Anchor:** Top-Center
- **Pos Y:** 20 (di atas bar)
- **Font Size:** 24
- **Alignment:** Center
- **Color:** White
- **Text:** "" (kosong, diisi script)

---

## 🔧 Component Setup

### AttackTimingBar.cs Settings

#### UI References (Drag & Drop)

- **Timing Slider:** (Buat Slider component di AttackTimingBar)
  - Direction: Bottom To Top
  - Min Value: 0
  - Max Value: 1
  - Value: 0
  - Fill Rect: Fill GameObject
- **Fill Image:** Fill Image component
- **Background Image:** Background Image component
- **Sweet Spot Indicator:** SweetSpot GameObject RectTransform
- **Status Text:** StatusText TextMeshProUGUI

#### Bar Settings

- **Bar Speed:** 1 (kecepatan gerak bar)
- **Auto Reverse:** ✓ (bar balik otomatis)
- **Min Value:** 0
- **Max Value:** 1

#### Sweet Spot Settings

- **Sweet Spot Start:** 0.4 (40% dari bawah)
- **Sweet Spot End:** 0.6 (60% dari bawah)
- **Sweet Spot Color:** Yellow (255, 255, 0, 100)

#### Colors

- **Normal Color:** Cyan (77, 204, 255)
- **Perfect Color:** Green (0, 255, 0)
- **Miss Color:** Red (255, 0, 0)
- **Background Color:** Dark (26, 26, 26, 200)

#### Animation

- **Enable Pulse:** ✓
- **Pulse Speed:** 2
- **Pulse Amount:** 0.1

#### Display Settings

- **Show Status Text:** ✓
- **Feedback Duration:** 0.5 seconds

---

## 🎮 Player Setup

### Add AttackTimingManager to Player

1. Select Player GameObject
2. Add Component: **AttackTimingManager.cs**

#### AttackTimingManager Settings

##### References

- **Timing Bar:** Drag AttackTimingBar GameObject
- **Weapon Controller:** Auto-detected (WeaponPlayerController)

##### Timing Settings

- **Enable Timing System:** ✓
- **Show Bar Only Near Enemy:** ✓ (recommended)
- **Detection Range:** 10 (meter)
- **Enemy Layer:** Select "Enemy" layer

##### Difficulty Settings

- **Easy Speed:** 0.5
- **Normal Speed:** 1.0
- **Hard Speed:** 1.5

---

## 🎯 How It Works

### Gameplay Flow

1. **Bar Activation:** Bar muncul dan bergerak saat player dekat enemy
2. **Timing Window:** Player harus attack saat bar di zona kuning (sweet spot)
3. **Feedback:** Text muncul menunjukkan hasil (PERFECT/GREAT/GOOD/MISS)
4. **Damage Bonus:** Timing yang bagus = damage multiplier lebih tinggi

### Timing Grades

- **PERFECT** (accuracy > 80%): 2x damage
- **GREAT** (accuracy > 50%): 1.5x damage
- **GOOD** (accuracy > 0%): 1.2x damage
- **MISS** (outside sweet spot): 0.5x damage

---

## 🎨 Visual Customization

### Color Schemes

#### Default (Sci-Fi)

- Normal: Cyan
- Perfect: Green
- Miss: Red
- Sweet Spot: Yellow

#### Fire Theme

- Normal: Orange (255, 165, 0)
- Perfect: Yellow (255, 255, 0)
- Miss: Dark Red (139, 0, 0)
- Sweet Spot: Light Orange (255, 200, 100)

#### Ice Theme

- Normal: Light Blue (173, 216, 230)
- Perfect: White (255, 255, 255)
- Miss: Dark Blue (0, 0, 139)
- Sweet Spot: Cyan (0, 255, 255)

### Size Variations

#### Compact (untuk mobile)

- Width: 30
- Height: 200
- Font Size: 18

#### Large (untuk PC)

- Width: 50
- Height: 400
- Font Size: 28

---

## ⚙️ Advanced Settings

### Position Presets

#### Right Side (Default)

```
Anchor: (1, 0.5)
Pivot: (1, 0.5)
Position: (-50, 0)
```

#### Left Side

```
Anchor: (0, 0.5)
Pivot: (0, 0.5)
Position: (50, 0)
```

#### Bottom Center

```
Anchor: (0.5, 0)
Pivot: (0.5, 0)
Position: (0, 50)
Rotation: Z = -90 (horizontal bar)
```

### Slider Setup Detail

1. Create Slider: Right-click AttackTimingBar → **UI > Slider**
2. Delete "Handle Slide Area" child (tidak perlu handle)
3. Set Slider Direction: **Bottom To Top**
4. Set Fill Rect: Assign "Fill" GameObject
5. Transition: None (no interactive)
6. Interactable: ✗ (disable)

---

## 🐛 Troubleshooting

### Bar Not Moving

- ✓ Check `isActive` is true (call `Activate()`)
- ✓ Check `barSpeed` > 0
- ✓ Check Slider direction is Bottom To Top

### Sweet Spot Not Visible

- ✓ Check SweetSpot Image has color with alpha > 0
- ✓ Check SweetSpot is child of AttackTimingBar
- ✓ Check Canvas is in Screen Space - Overlay mode

### Status Text Not Showing

- ✓ Enable "Show Status Text" in inspector
- ✓ Check TextMeshProUGUI is assigned
- ✓ Check text color is not transparent

### Bar Not Appearing Near Enemy

- ✓ Enemy has correct Layer set to "Enemy"
- ✓ Detection Range is large enough
- ✓ "Show Bar Only Near Enemy" is enabled

---

## 🎓 Integration with Weapon System

Timing bar sudah terintegrasi dengan weapon system. Nanti kamu tinggal panggil method `OnPlayerAttack()` dari `AttackTimingManager` saat player attack untuk mendapat damage multiplier.

Example integration akan dijelaskan setelah UI setup selesai!
