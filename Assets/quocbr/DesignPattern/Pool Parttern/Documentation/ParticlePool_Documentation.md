# ParticlePool - Hướng Dẫn Sử Dụng Chi Tiết

**Author:** quocbr  
**Github:** https://github.com/quocbr  
**Created:** 2026-02-05  
**Last Updated:** 2026-02-06

---

## 📋 Mục Lục

1. [Tổng Quan](#tổng-quan)
2. [Cài Đặt](#cài-đặt)
3. [Cách Sử Dụng Cơ Bản](#cách-sử-dụng-cơ-bản)
4. [Cách Sử Dụng Nâng Cao](#cách-sử-dụng-nâng-cao)
5. [API Reference](#api-reference)
6. [Best Practices](#best-practices)
7. [Troubleshooting](#troubleshooting)
8. [Examples](#examples)

---

## 🎯 Tổng Quan

### ParticlePool là gì?

`ParticlePool` là một static pool manager được thiết kế đặc biệt cho **ParticleSystem effects** trong Unity. Nó giúp tối ưu hiệu suất bằng cách tái sử dụng các particle effects thay vì liên tục tạo mới và hủy chúng.

### Tại sao cần ParticlePool?

#### ❌ Vấn đề khi KHÔNG dùng Pool:
```csharp
// Mỗi lần bắn đạn, tạo mới effect
ParticleSystem explosion = Instantiate(explosionPrefab, position, rotation);
Destroy(explosion.gameObject, 2f);

// Vấn đề:
// - Garbage Collection liên tục → Lag spike
// - Hiệu suất giảm khi có nhiều effects cùng lúc
// - Memory allocation không tối ưu
```

#### ✅ Giải pháp với ParticlePool:
```csharp
// Chỉ Play effect từ pool có sẵn
ParticlePool.Play(explosionPrefab, position, rotation);

// Lợi ích:
// ✓ Không có Garbage Collection
// ✓ Hiệu suất ổn định
// ✓ Tối ưu memory
// ✓ Tự động quản lý lifecycle
```

### Đặc Điểm Chính

- ✅ **Static Pattern**: Truy cập dễ dàng từ bất kỳ đâu
- ✅ **Auto-Config**: Tự động fix các settings không phù hợp của ParticleSystem
- ✅ **Type-Safe**: Hỗ trợ ParticleType enum cho type-safe access
- ✅ **Auto-Expand**: Tự động tăng pool size khi cần
- ✅ **Zero GC**: Không tạo garbage khi sử dụng đúng cách
- ✅ **Editor Integration**: Tích hợp với PoolController

---

## 🔧 Cài Đặt

### Bước 1: Chuẩn Bị ParticleSystem Prefab

ParticlePool sẽ **tự động kiểm tra và fix** các settings của ParticleSystem trong Editor:

| Setting | Yêu Cầu | Auto-Fix |
|---------|---------|----------|
| Loop | `false` | ✅ |
| Play On Awake | `false` | ✅ |
| Stop Action | `None` | ✅ |
| Duration | `≤ 1s` | ✅ (set về 1s) |

**Lưu ý:** Auto-fix chỉ chạy trong Unity Editor (UNITY_EDITOR). Trong build, đảm bảo prefab đã được fix đúng.

### Bước 2: Tạo ParticleType Enum (Tùy Chọn)

Nếu muốn sử dụng type-safe access, tạo file `ParticleType.cs`:

```csharp
/// <summary>
/// Enum định nghĩa các loại particle effects trong game
/// </summary>
public enum ParticleType
{
    None = 0,
    
    // Combat Effects
    Explosion = 1,
    MuzzleFlash = 2,
    BloodSplash = 3,
    HitSpark = 4,
    
    // Magic Effects
    FireBall = 10,
    IceBlast = 11,
    Lightning = 12,
    Heal = 13,
    
    // Environment Effects
    Dust = 20,
    Smoke = 21,
    Splash = 22,
    Leaves = 23,
    
    // UI Effects
    LevelUp = 30,
    Collect = 31,
    StarBurst = 32
}
```

### Bước 3: Setup PoolController (Khuyến Nghị)

Tạo GameObject trong scene với component `PoolController` để quản lý tập trung:

```csharp
// PoolController sẽ tự động:
// - Tạo root transform cho tất cả pools
// - Preload các effects thường dùng
// - Đăng ký shortcuts cho ParticleType
```

**Cấu hình trong Inspector:**
```
PoolController
├── Particle Configs
│   ├── [0] Explosion
│   │   ├── Particle Type: Explosion
│   │   ├── Prefab: ExplosionEffect
│   │   └── Pool Amount: 10
│   ├── [1] MuzzleFlash
│   │   ├── Particle Type: MuzzleFlash
│   │   ├── Prefab: MuzzleEffect
│   │   └── Pool Amount: 20
│   └── ...
```

---

## 🚀 Cách Sử Dụng Cơ Bản

### 1. Play Effect Đơn Giản (Direct Prefab)

Cách đơn giản nhất - chỉ cần prefab:

```csharp
public class Weapon : MonoBehaviour
{
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    [SerializeField] private ParticleSystem explosionPrefab;
    
    public void Shoot()
    {
        // Play muzzle flash tại nòng súng
        ParticlePool.Play(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
    }
    
    public void Explode(Vector3 position)
    {
        // Play explosion tại vị trí
        ParticlePool.Play(explosionPrefab, position, Quaternion.identity);
    }
}
```

**Ưu điểm:**
- ✅ Dễ sử dụng, không cần setup trước
- ✅ Pool tự động tạo lần đầu tiên

**Nhược điểm:**
- ⚠️ Lần đầu Play sẽ tạo pool → có thể lag một chút
- ⚠️ Không type-safe

### 2. Play Effect với ParticleType (Khuyến Nghị)

Sử dụng enum để type-safe và dễ quản lý:

```csharp
public class Weapon : MonoBehaviour
{
    public void Shoot()
    {
        // Type-safe, auto-complete trong IDE
        ParticlePool.Play(ParticleType.MuzzleFlash, firePoint.position, firePoint.rotation);
    }
    
    public void Explode(Vector3 position)
    {
        ParticlePool.Play(ParticleType.Explosion, position, Quaternion.identity);
    }
}
```

**Yêu cầu:** Phải setup trong PoolController trước (xem [Bước 3](#bước-3-setup-poolcontroller-khuyến-nghị))

**Ưu điểm:**
- ✅ Type-safe, IDE auto-complete
- ✅ Dễ refactor
- ✅ Pool đã được preload → không lag
- ✅ Code sạch hơn

### 3. Preload Pool Trước Khi Dùng

Để tránh lag lần đầu tiên, preload pool trước:

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionPrefab;
    [SerializeField] private ParticleSystem smokePrefab;
    
    private void Start()
    {
        // Preload 10 explosions
        ParticlePool.Preload(explosionPrefab, qty: 10);
        
        // Preload 5 smokes
        ParticlePool.Preload(smokePrefab, qty: 5);
        
        // Có thể chỉ định parent transform
        Transform effectsRoot = transform.Find("Effects");
        ParticlePool.Preload(explosionPrefab, qty: 10, parent: effectsRoot);
    }
}
```

**Khi nào nên Preload:**
- ✅ Effects dùng nhiều (explosions, hits, bullets)
- ✅ Effects cần chạy ngay lập tức (intro, cutscene)
- ✅ Boss fight effects trước khi vào boss room

**Khi nào KHÔNG cần Preload:**
- ❌ Effects hiếm dùng (special abilities)
- ❌ Effects chỉ dùng 1 lần (tutorial)

---

## 🎓 Cách Sử Dụng Nâng Cao

### 1. Tích Hợp Với Damage System

```csharp
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private Transform hitPoint;
    
    public void TakeDamage(int damage, DamageType damageType)
    {
        // Play effect phù hợp với damage type
        switch (damageType)
        {
            case DamageType.Physical:
                ParticlePool.Play(ParticleType.BloodSplash, hitPoint.position, Quaternion.identity);
                break;
                
            case DamageType.Fire:
                ParticlePool.Play(ParticleType.FireBall, hitPoint.position, Quaternion.identity);
                break;
                
            case DamageType.Ice:
                ParticlePool.Play(ParticleType.IceBlast, hitPoint.position, Quaternion.identity);
                break;
                
            case DamageType.Lightning:
                ParticlePool.Play(ParticleType.Lightning, hitPoint.position, Quaternion.identity);
                break;
        }
    }
}
```

### 2. Tích Hợp Với Sound System

```csharp
public class EffectManager : MonoBehaviour
{
    public void PlayExplosion(Vector3 position)
    {
        // Play particle effect
        ParticlePool.Play(ParticleType.Explosion, position, Quaternion.identity);
        
        // Play sound đồng bộ
        AudioManager.PlaySFX(SoundType.Explosion, position);
    }
    
    public void PlayHitEffect(Vector3 position, Vector3 normal)
    {
        // Calculate rotation from normal
        Quaternion rotation = Quaternion.LookRotation(normal);
        
        // Play effect hướng theo surface normal
        ParticlePool.Play(ParticleType.HitSpark, position, rotation);
        
        // Play sound
        AudioManager.PlaySFX(SoundType.HitMetal, position);
    }
}
```

### 3. Effect Theo Dõi Target (Following Effect)

ParticlePool không trực tiếp hỗ trợ following, nhưng có thể wrapper:

```csharp
public class FollowingEffect : MonoBehaviour
{
    private Transform target;
    private ParticleSystem ps;
    
    public static void PlayFollowing(ParticleType type, Transform target)
    {
        // Play effect
        Vector3 pos = target.position;
        ParticlePool.Play(type, pos, Quaternion.identity);
        
        // TODO: Implement following logic nếu cần
        // Lưu ý: Particle đã được pool, không nên modify sau khi Play
    }
}
```

**Lưu ý:** ParticlePool tối ưu cho **fire-and-forget effects**. Nếu cần effects phức tạp hơn (following, interactive), nên dùng SimplePool thay vì ParticlePool.

### 4. Combo Effects (Multiple Effects)

```csharp
public class SkillSystem : MonoBehaviour
{
    public void PlayUltimateSkill(Vector3 center)
    {
        StartCoroutine(UltimateSequence(center));
    }
    
    private IEnumerator UltimateSequence(Vector3 center)
    {
        // Charge up effect
        ParticlePool.Play(ParticleType.ChargeUp, center, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        
        // Main explosion
        ParticlePool.Play(ParticleType.Explosion, center, Quaternion.identity);
        
        // Shockwave
        yield return new WaitForSeconds(0.1f);
        ParticlePool.Play(ParticleType.Shockwave, center, Quaternion.identity);
        
        // Debris
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * 3f;
            ParticlePool.Play(ParticleType.Debris, randomPos, Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
        }
    }
}
```

### 5. Surface-Based Effects

```csharp
public class SurfaceEffect : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceConfig
    {
        public string surfaceTag;
        public ParticleType particleType;
    }
    
    [SerializeField] private List<SurfaceConfig> surfaceConfigs;
    
    public void PlayImpactEffect(RaycastHit hit)
    {
        // Tìm particle type phù hợp với surface
        ParticleType type = GetParticleTypeForSurface(hit.collider.tag);
        
        // Calculate rotation từ normal
        Quaternion rotation = Quaternion.LookRotation(hit.normal);
        
        // Play effect
        ParticlePool.Play(type, hit.point, rotation);
    }
    
    private ParticleType GetParticleTypeForSurface(string tag)
    {
        foreach (var config in surfaceConfigs)
        {
            if (config.surfaceTag == tag)
                return config.particleType;
        }
        
        return ParticleType.Dust; // Default
    }
}
```

### 6. Release Pool Khi Không Cần

Giải phóng memory khi chuyển scene hoặc không cần effect nữa:

```csharp
public class SceneTransition : MonoBehaviour
{
    public void OnLeaveBossRoom()
    {
        // Release specific particles
        ParticlePool.Release(ParticleType.BossAttack);
        ParticlePool.Release(ParticleType.BossSpecial);
        
        // Hoặc release bằng prefab reference
        ParticlePool.Release(bossAttackPrefab);
    }
}
```

---

## 📖 API Reference

### Public Methods

#### `Play(ParticleSystem prefab, Vector3 pos, Quaternion rot)`
**Mô tả:** Play particle effect từ pool tại vị trí và rotation chỉ định

**Parameters:**
- `prefab` (ParticleSystem): Prefab của particle effect
- `pos` (Vector3): Vị trí world space
- `rot` (Quaternion): Rotation

**Returns:** void

**Example:**
```csharp
ParticlePool.Play(explosionPrefab, transform.position, Quaternion.identity);
```

**Lưu ý:**
- Pool tự động tạo nếu chưa tồn tại
- Tự động expand nếu tất cả particles đang active

---

#### `Play(ParticleType particleType, Vector3 pos, Quaternion rot)`
**Mô tả:** Play particle effect sử dụng ParticleType enum (type-safe)

**Parameters:**
- `particleType` (ParticleType): Loại particle từ enum
- `pos` (Vector3): Vị trí world space
- `rot` (Quaternion): Rotation

**Returns:** void

**Example:**
```csharp
ParticlePool.Play(ParticleType.Explosion, transform.position, Quaternion.identity);
```

**Lưu ý:**
- Yêu cầu: ParticleType phải được đăng ký trước qua `Shortcut()` hoặc PoolController
- Ném error trong Editor nếu type chưa được đăng ký

---

#### `Preload(ParticleSystem prefab, int qty = 1, Transform parent = null)`
**Mô tả:** Preload pool với số lượng instances chỉ định

**Parameters:**
- `prefab` (ParticleSystem): Prefab để preload
- `qty` (int, optional): Số lượng instances. Default = 1
- `parent` (Transform, optional): Parent transform. Default = Pool Root

**Returns:** void

**Example:**
```csharp
// Preload 10 explosions
ParticlePool.Preload(explosionPrefab, 10);

// Preload với custom parent
Transform vfxRoot = GameObject.Find("VFX").transform;
ParticlePool.Preload(explosionPrefab, 10, vfxRoot);
```

**Best Practice:**
- Preload trong Start() hoặc loading screen
- Số lượng = số effects có thể active cùng lúc + buffer

---

#### `Release(ParticleSystem prefab)`
**Mô tả:** Giải phóng pool của prefab, destroy tất cả instances

**Parameters:**
- `prefab` (ParticleSystem): Prefab cần release

**Returns:** void

**Example:**
```csharp
ParticlePool.Release(explosionPrefab);
```

**Khi nào dùng:**
- Chuyển scene (cleanup)
- Kết thúc boss fight (effects đặc biệt)
- Low memory warning

---

#### `Release(ParticleType particleType)`
**Mô tả:** Giải phóng pool sử dụng ParticleType enum

**Parameters:**
- `particleType` (ParticleType): Loại particle cần release

**Returns:** void

**Example:**
```csharp
ParticlePool.Release(ParticleType.BossAttack);
```

---

#### `Shortcut(ParticleType particleType, ParticleSystem particleSystem)`
**Mô tả:** Đăng ký shortcut từ ParticleType enum đến ParticleSystem prefab

**Parameters:**
- `particleType` (ParticleType): Enum type
- `particleSystem` (ParticleSystem): Prefab tương ứng

**Returns:** void

**Example:**
```csharp
ParticlePool.Shortcut(ParticleType.Explosion, explosionPrefab);
```

**Lưu ý:**
- Thường được gọi tự động bởi PoolController
- Chỉ cần gọi thủ công nếu không dùng PoolController

---

### Public Properties

#### `Root` (Transform, readonly)
**Mô tả:** Root transform chứa tất cả pools

**Returns:** Transform của PoolController hoặc auto-created "ParticlePool" GameObject

**Example:**
```csharp
Transform poolRoot = ParticlePool.Root;
Debug.Log($"Pool has {poolRoot.childCount} effect types");
```

---

### Private Implementation Details

#### Auto-Fix Features (Editor Only)

ParticlePool tự động kiểm tra và fix các settings trong Unity Editor:

```csharp
// Tự động fix các vấn đề:
✓ Loop = false (particle không loop vô hạn)
✓ Play On Awake = false (không tự play)
✓ Stop Action = None (không tự destroy/disable)
✓ Duration ≤ 1s (giới hạn duration)
```

**Lưu ý:** 
- Auto-fix chỉ chạy trong Editor (#if UNITY_EDITOR)
- Sử dụng `Undo.RegisterCompleteObjectUndo` để có thể undo
- Log thông báo mỗi khi fix

#### Pool Expansion Strategy

```csharp
// Khi tất cả particles trong pool đang active:
if (obj.isPlaying)
{
    // → Tạo thêm 1 instance mới
    obj = GameObject.Instantiate(prefab, m_sRoot);
    obj.Stop();
    inactive.Insert(index, obj);
}
```

**Đặc điểm:**
- Tăng từng 1 instance (không double size như SimplePool)
- Insert vào đúng vị trí index hiện tại
- Không có giới hạn max size

---

## 💡 Best Practices

### 1. ✅ Nên Làm

#### Sử dụng ParticleType Enum
```csharp
// ✅ GOOD - Type-safe, dễ refactor
ParticlePool.Play(ParticleType.Explosion, pos, rot);

// ❌ BAD - Error-prone, khó refactor
ParticlePool.Play(explosionPrefab, pos, rot);
```

#### Preload Effects Quan Trọng
```csharp
// ✅ GOOD - Preload trong loading
private void Awake()
{
    ParticlePool.Preload(explosionPrefab, 20);
    ParticlePool.Preload(muzzleFlashPrefab, 50);
}

// ❌ BAD - Lazy load → lag spike lần đầu
// Không preload gì cả
```

#### Cấu Hình Đúng ParticleSystem
```csharp
// ✅ GOOD - Settings phù hợp với pool
Main Module:
├── Duration: 1s
├── Looping: false
├── Play On Awake: false
└── Stop Action: None

// ❌ BAD - Settings sẽ gây lỗi
Main Module:
├── Duration: 5s      // Quá dài
├── Looping: true     // Loop vô hạn
└── Stop Action: Destroy  // Tự destroy
```

#### Release Khi Chuyển Scene
```csharp
// ✅ GOOD - Cleanup memory
private void OnDestroy()
{
    ParticlePool.Release(ParticleType.BossAttack);
    ParticlePool.Release(ParticleType.BossSpecial);
}

// ❌ BAD - Memory leak
// Không release, pools tồn tại mãi mãi
```

### 2. ❌ Không Nên Làm

#### Không Dùng Cho Interactive Effects
```csharp
// ❌ BAD - Không nên dùng ParticlePool
// Particle cần follow player → dùng SimplePool
ParticlePool.Play(healAuraPrefab, player.position, Quaternion.identity);

// ✅ GOOD - Dùng SimplePool cho complex effects
var aura = SimplePool.Spawn(healAuraPrefab, player.position, Quaternion.identity);
aura.transform.SetParent(player.transform);
```

#### Không Modify Particle Sau Khi Play
```csharp
// ❌ BAD - Không có cách để get reference
ParticlePool.Play(ParticleType.Explosion, pos, rot);
// Làm sao để modify particle?? → Không được!

// ✅ GOOD - ParticlePool là fire-and-forget
// Mọi setting phải configure trong prefab trước
```

#### Không Dùng Loop = True
```csharp
// ❌ BAD - Particle loop vô hạn
Main Module:
└── Looping: true  // Particle không bao giờ dừng!

// ✅ GOOD - One-shot effect
Main Module:
└── Looping: false  // Play một lần rồi dừng
```

### 3. ⚡ Performance Tips

#### Pool Size Hợp Lý
```csharp
// ✅ GOOD - Pool size = expected concurrent effects
ParticlePool.Preload(bulletHitPrefab, 30);  // Có thể 30 hits cùng lúc
ParticlePool.Preload(explosionPrefab, 10);  // Ít explosions hơn

// ❌ BAD - Pool quá lớn
ParticlePool.Preload(bulletHitPrefab, 1000);  // Lãng phí memory
```

#### Reuse Prefabs
```csharp
// ✅ GOOD - 1 prefab cho nhiều mục đích
ParticlePool.Play(genericSmokePrefab, pos1, rot1);
ParticlePool.Play(genericSmokePrefab, pos2, rot2);

// ❌ BAD - Nhiều prefabs giống nhau
ParticlePool.Play(smoke1Prefab, pos1, rot1);
ParticlePool.Play(smoke2Prefab, pos2, rot2);  // Duplicate
```

#### Short Duration
```csharp
// ✅ GOOD - Duration ngắn
Duration: 0.5s - 1s  // Particles recycle nhanh

// ❌ BAD - Duration quá dài
Duration: 5s - 10s  // Particles bị lock lâu
```

---

## 🐛 Troubleshooting

### Vấn Đề 1: Effect Không Xuất Hiện

**Triệu chứng:**
```csharp
ParticlePool.Play(ParticleType.Explosion, pos, rot);
// Không có gì xảy ra
```

**Nguyên nhân & Giải pháp:**

#### A) ParticleType chưa được đăng ký
```csharp
// Kiểm tra trong Console:
// "Explosion is needs install at pool container!!!"

// Giải pháp: Thêm vào PoolController
PoolController → Particle Configs → Add Element
└── Particle Type: Explosion
└── Prefab: Your Explosion Prefab
```

#### B) Prefab = null
```csharp
// Kiểm tra Console:
// "NullReferenceException: prefab is null!"

// Giải pháp: Assign prefab trong Inspector
[SerializeField] private ParticleSystem explosionPrefab;  // ← Assign này!
```

#### C) Particle bị ẩn sau object khác
```csharp
// Kiểm tra Sorting Layer & Order in Layer
Renderer Settings:
└── Sorting Layer: Effects
└── Order in Layer: 100  // Cao hơn các layer khác
```

### Vấn Đề 2: Particle Loop Vô Hạn

**Triệu chứng:**
- Particle không bao giờ dừng
- Pool không recycle được
- Hiệu suất giảm dần theo thời gian

**Nguyên nhân & Giải pháp:**
```csharp
// Kiểm tra Main Module:
Main Module:
└── Looping: true  // ← ĐÂY LÀ VẤN ĐỀ!

// Giải pháp: Set về false
Main Module:
└── Looping: false

// ParticlePool sẽ auto-fix trong Editor
// Nhưng đảm bảo prefab đúng trước khi build!
```

### Vấn Đề 3: Lag Spike Lần Đầu Play

**Triệu chứng:**
- FPS drop lần đầu tiên chạy effect
- Sau đó mượt mà

**Nguyên nhân:**
- Pool chưa được preload
- Phải instantiate lần đầu

**Giải pháp:**
```csharp
// Preload trong Awake/Start
private void Awake()
{
    ParticlePool.Preload(explosionPrefab, 10);
    ParticlePool.Preload(muzzleFlashPrefab, 20);
}

// Hoặc setup trong PoolController
```

### Vấn Đề 4: Effect Bị Destroy Tự Động

**Triệu chứng:**
- Effect biến mất sau một lúc
- Console log: "MissingReferenceException"

**Nguyên nhân & Giải pháp:**
```csharp
// Kiểm tra Stop Action:
Main Module:
└── Stop Action: Destroy  // ← ĐÂY LÀ VẤN ĐỀ!

// Giải pháp: Set về None
Main Module:
└── Stop Action: None

// ParticlePool sẽ auto-fix trong Editor
```

### Vấn Đề 5: Memory Leak

**Triệu chứng:**
- Memory tăng dần không giảm
- Nhiều pool objects trong Hierarchy

**Nguyên nhân & Giải pháp:**
```csharp
// A) Không release khi chuyển scene
// Giải pháp:
private void OnDestroy()
{
    ParticlePool.Release(ParticleType.Explosion);
}

// B) Pool expand quá nhiều
// Giải pháp: Tăng initial pool size
ParticlePool.Preload(prefab, 50);  // Tăng từ 10 → 50
```

### Vấn Đề 6: Effect Bị Sai Rotation

**Triệu chứng:**
- Effect không hướng đúng surface normal

**Giải pháp:**
```csharp
// ❌ BAD
ParticlePool.Play(prefab, hit.point, Quaternion.identity);

// ✅ GOOD - Calculate rotation from normal
Quaternion rotation = Quaternion.LookRotation(hit.normal);
ParticlePool.Play(prefab, hit.point, rotation);

// Hoặc inverse nếu particle hướng ngược
Quaternion rotation = Quaternion.LookRotation(-hit.normal);
```

---

## 📚 Examples

### Example 1: Basic Weapon System

```csharp
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    [SerializeField] private ParticleSystem bulletImpactPrefab;
    
    private void Start()
    {
        // Preload effects
        ParticlePool.Preload(muzzleFlashPrefab, 10);
        ParticlePool.Preload(bulletImpactPrefab, 30);
    }
    
    public void Fire()
    {
        // Play muzzle flash
        ParticlePool.Play(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
        
        // Raycast to find hit
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, 100f))
        {
            // Play impact effect
            Quaternion rotation = Quaternion.LookRotation(hit.normal);
            ParticlePool.Play(bulletImpactPrefab, hit.point, rotation);
        }
    }
}
```

### Example 2: Magic Spell System (Type-Safe)

```csharp
using UnityEngine;

public class MagicSpell : MonoBehaviour
{
    [Header("Spell Config")]
    [SerializeField] private ParticleType castEffect;
    [SerializeField] private ParticleType impactEffect;
    [SerializeField] private ParticleType auraEffect;
    
    [Header("Settings")]
    [SerializeField] private float castTime = 1f;
    [SerializeField] private float speed = 10f;
    
    public void Cast(Vector3 origin, Vector3 direction)
    {
        StartCoroutine(CastSequence(origin, direction));
    }
    
    private IEnumerator CastSequence(Vector3 origin, Vector3 direction)
    {
        // Cast effect at player position
        ParticlePool.Play(castEffect, origin, Quaternion.LookRotation(direction));
        
        yield return new WaitForSeconds(castTime);
        
        // Shoot projectile
        Vector3 currentPos = origin;
        float distance = 0f;
        
        while (distance < 50f)
        {
            currentPos += direction * speed * Time.deltaTime;
            distance += speed * Time.deltaTime;
            
            // Check collision
            if (Physics.Raycast(currentPos, direction, out RaycastHit hit, speed * Time.deltaTime))
            {
                // Impact effect
                ParticlePool.Play(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                
                // Aura effect around impact
                ParticlePool.Play(auraEffect, hit.point, Quaternion.identity);
                
                break;
            }
            
            yield return null;
        }
    }
}
```

### Example 3: Footstep System

```csharp
using UnityEngine;

public class FootstepSystem : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceEffects
    {
        public string surfaceTag;
        public ParticleType dustType;
        public ParticleType splashType;
    }
    
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform rightFoot;
    [SerializeField] private SurfaceEffects[] surfaceEffects;
    
    private bool isLeftFoot = true;
    
    // Call from Animation Event
    public void OnFootstep()
    {
        Transform foot = isLeftFoot ? leftFoot : rightFoot;
        isLeftFoot = !isLeftFoot;
        
        // Raycast down from foot
        if (Physics.Raycast(foot.position, Vector3.down, out RaycastHit hit, 0.5f))
        {
            // Find matching surface
            foreach (var surface in surfaceEffects)
            {
                if (hit.collider.CompareTag(surface.surfaceTag))
                {
                    // Play appropriate effect
                    ParticlePool.Play(surface.dustType, hit.point, Quaternion.identity);
                    break;
                }
            }
        }
    }
}
```

### Example 4: Combo System với Multiple Effects

```csharp
using UnityEngine;
using System.Collections;

public class ComboAttack : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    
    public void ExecuteCombo()
    {
        StartCoroutine(ComboSequence());
    }
    
    private IEnumerator ComboSequence()
    {
        // Hit 1 - Quick slash
        ParticlePool.Play(ParticleType.SlashEffect, attackPoint.position, attackPoint.rotation);
        yield return new WaitForSeconds(0.3f);
        
        // Hit 2 - Upper cut
        ParticlePool.Play(ParticleType.SlashEffect, attackPoint.position, attackPoint.rotation * Quaternion.Euler(45, 0, 0));
        yield return new WaitForSeconds(0.3f);
        
        // Hit 3 - Spin attack
        for (int i = 0; i < 4; i++)
        {
            Quaternion rot = attackPoint.rotation * Quaternion.Euler(0, i * 90, 0);
            ParticlePool.Play(ParticleType.SlashEffect, attackPoint.position, rot);
            yield return new WaitForSeconds(0.1f);
        }
        
        // Final hit - Explosion
        ParticlePool.Play(ParticleType.Explosion, attackPoint.position, Quaternion.identity);
        
        // Shockwave
        yield return new WaitForSeconds(0.2f);
        ParticlePool.Play(ParticleType.Shockwave, attackPoint.position, Quaternion.identity);
    }
}
```

### Example 5: Environment Interaction

```csharp
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [SerializeField] private ParticleType breakEffect;
    [SerializeField] private ParticleType smokeEffect;
    
    public void OnDestroyed()
    {
        Vector3 center = transform.position;
        
        // Main break effect at center
        ParticlePool.Play(breakEffect, center, Quaternion.identity);
        
        // Smoke clouds around
        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 2f;
            offset.y = Mathf.Abs(offset.y);
            
            ParticlePool.Play(smokeEffect, center + offset, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
}
```

### Example 6: Boss Battle Effects

```csharp
using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Boss Effects")]
    [SerializeField] private ParticleType chargeEffect;
    [SerializeField] private ParticleType attackEffect;
    [SerializeField] private ParticleType specialEffect;
    [SerializeField] private ParticleType rageEffect;
    
    [SerializeField] private Transform[] attackPoints;
    
    private void Start()
    {
        // Preload boss effects
        var prefabs = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var prefab in prefabs)
        {
            ParticlePool.Preload(prefab, 10);
        }
    }
    
    public void SpecialAttack()
    {
        StartCoroutine(SpecialAttackSequence());
    }
    
    private IEnumerator SpecialAttackSequence()
    {
        // Phase 1: Charge
        ParticlePool.Play(chargeEffect, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(2f);
        
        // Phase 2: Multiple attacks
        foreach (var point in attackPoints)
        {
            ParticlePool.Play(attackEffect, point.position, point.rotation);
            yield return new WaitForSeconds(0.2f);
        }
        
        // Phase 3: Special finish
        ParticlePool.Play(specialEffect, transform.position, Quaternion.identity);
    }
    
    public void EnterRageMode()
    {
        // Continuous rage aura
        StartCoroutine(RageAura());
    }
    
    private IEnumerator RageAura()
    {
        while (true)
        {
            ParticlePool.Play(rageEffect, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(1f);
        }
    }
    
    private void OnDestroy()
    {
        // Cleanup boss-specific effects
        ParticlePool.Release(chargeEffect);
        ParticlePool.Release(attackEffect);
        ParticlePool.Release(specialEffect);
        ParticlePool.Release(rageEffect);
    }
}
```

---

## 🔗 So Sánh Với SimplePool

| Tính Năng | ParticlePool | SimplePool |
|-----------|--------------|------------|
| **Mục đích** | Particle effects | GameObjects |
| **Auto-Config** | ✅ Yes | ❌ No |
| **Return Reference** | ❌ No (fire-and-forget) | ✅ Yes |
| **Following/Parenting** | ❌ No | ✅ Yes |
| **Auto-Despawn** | ✅ Yes (tự động) | ⚠️ Manual |
| **Pool Expansion** | +1 per time | Double size |
| **Use Case** | VFX, explosions, hits | Bullets, enemies, items |

**Khi nào dùng ParticlePool:**
- ✅ One-shot effects (explosions, hits, sparks)
- ✅ Fire-and-forget effects
- ✅ Không cần modify sau khi spawn
- ✅ Auto-despawn theo duration

**Khi nào dùng SimplePool:**
- ✅ Cần reference để modify
- ✅ Cần parent/follow objects
- ✅ Complex lifecycle management
- ✅ GameObjects thông thường (bullets, enemies)

---

## 📝 Summary

### Key Takeaways

1. **ParticlePool là gì:**
   - Static pool manager cho ParticleSystem effects
   - Tối ưu hiệu suất bằng object reuse
   - Auto-config particle settings

2. **Cách sử dụng:**
   - Basic: `ParticlePool.Play(prefab, pos, rot)`
   - Type-safe: `ParticlePool.Play(ParticleType.Explosion, pos, rot)`
   - Preload: `ParticlePool.Preload(prefab, qty)`

3. **Best Practices:**
   - ✅ Sử dụng ParticleType enum
   - ✅ Preload effects quan trọng
   - ✅ Release khi chuyển scene
   - ✅ Duration ngắn (≤1s)
   - ❌ Không dùng cho interactive effects

4. **Performance:**
   - Zero GC khi sử dụng đúng
   - Pool size = concurrent effects + buffer
   - Auto-expand khi cần

---

## 📞 Support

**Author:** quocbr  
**Github:** https://github.com/quocbr  
**Email:** [Your Email]

**Related Documentation:**
- [SimplePool Documentation](./SimplePool_Documentation.md)
- [PoolController Documentation](./PoolController_Documentation.md)
- [StateMachine Pattern Documentation](./StateMachine_Documentation.md)

**Unity Documentation:**
- [ParticleSystem Reference](https://docs.unity3d.com/ScriptReference/ParticleSystem.html)
- [Object Pooling Best Practices](https://unity.com/how-to/use-object-pooling-boost-performance)

---

**Version:** 1.0.0  
**Last Updated:** 2026-02-06  
**Unity Version:** 2021.3+
