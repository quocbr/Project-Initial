# Factory Pattern System - Hướng dẫn sử dụng

## 📖 Tổng quan

Hệ thống Factory Pattern giúp quản lý và tạo các Unit một cách có tổ chức, dễ maintain và mở rộng.

## 🏗️ Kiến trúc

### 1. Base Classes
- **BaseFactorySO<TKey, TValue>**: Generic factory base class
  - TKey: Kiểu enum định danh (UnitType, ...)
  - TValue: Kiểu component/object (BaseUnit, ...)
  
- **BaseUnit**: Abstract class cho tất cả units

### 2. Factory Implementation
- **UnitFactorySO**: Factory tạo units (Soldier, Tank, Archer,...)

## 🚀 Cách sử dụng

### Bước 1: Tạo ScriptableObject Factory

1. Right-click trong Project window
2. Chọn `Create > Game > Factories > Unit Factory (Typed)`
3. Đặt tên cho factory (vd: `MyUnitFactory`)

### Bước 2: Config Factory trong Inspector

1. Chọn Factory vừa tạo
2. Thêm items vào list:
   - **ID**: Chọn UnitType (Soldier, Tank, Archer)
   - **Value**: Kéo prefab BaseUnit vào

### Bước 3: Sử dụng trong Code

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private UnitFactorySO unitFactory;
    
    void Start()
    {
        // Lấy unit prefab
        BaseUnit soldierPrefab = unitFactory.GetItem(UnitType.Soldier);
        
        // Spawn unit
        BaseUnit soldier = Instantiate(soldierPrefab, Vector3.zero, Quaternion.identity);
    }
}
```

## 📚 API Reference

### BaseFactorySO Methods

```csharp
// Lấy item theo key
TValue GetItem(TKey id)

// Kiểm tra key có tồn tại không
bool HasItem(TKey id)

// Lấy số lượng items
int Count { get; }

// Lấy tất cả keys
IEnumerable<TKey> GetAllKeys()
```

## ✨ Ví dụ nâng cao

### Spawn nhiều units

```csharp
public void SpawnArmy(UnitType type, int count, Vector3 center, float radius)
{
    BaseUnit prefab = unitFactory.GetItem(type);
    if (prefab == null) return;
    
    for (int i = 0; i < count; i++)
    {
        Vector3 randomPos = center + Random.insideUnitSphere * radius;
        randomPos.y = 0;
        Instantiate(prefab, randomPos, Quaternion.identity);
    }
}
```

### Kiểm tra và spawn

```csharp
public void SafeSpawn(UnitType type, Vector3 position)
{
    // Kiểm tra trước khi spawn
    if (!unitFactory.HasItem(type))
    {
        Debug.LogWarning($"Unit {type} không tồn tại trong factory!");
        return;
    }
    
    BaseUnit prefab = unitFactory.GetItem(type);
    Instantiate(prefab, position, Quaternion.identity);
}
```

## 🔧 Mở rộng

### Thêm Factory mới

1. Tạo enum mới:
```csharp
public enum WeaponType { Sword, Gun, Bow }
```

2. Tạo base class:
```csharp
public abstract class BaseWeapon : MonoBehaviour 
{
    public abstract void Attack();
}
```

3. Tạo factory:
```csharp
[CreateAssetMenu(fileName = "NewWeaponFactory", menuName = "Game/Factories/Weapon Factory")]
public class WeaponFactorySO : BaseFactorySO<WeaponType, BaseWeapon> { }
```

## 💡 Best Practices

1. ✅ Luôn kiểm tra `HasItem()` trước khi `GetItem()`
2. ✅ Sử dụng enum thay vì string cho keys
3. ✅ Tạo base class cho mỗi loại object
4. ✅ Validate dữ liệu trong Inspector (OnValidate đã handle)
5. ✅ Đặt tên factory rõ ràng (UnitFactory)

## ⚠️ Lưu ý

- Factory trả về **prefab component**, không phải GameObject
- Cần gọi `Instantiate()` để tạo instance trong scene
- Enum keys phải unique, không được trùng lặp
- Factory sẽ warning nếu có duplicate keys hoặc null values

## 🎯 Use Cases

- **RTS Games**: Spawn units
- **Tower Defense**: Spawn enemies, towers
- **RPG**: Tạo characters, NPCs
- **Action Games**: Spawn enemies, allies
- **Audio System**: Quản lý sound effects theo enum

---

Xem **FactoryUsageExample.cs** để biết thêm ví dụ chi tiết!
