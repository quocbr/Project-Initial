# 🔧 ENUM MANAGER - HƯỚNG DẪN SỬ DỤNG

## ✨ Giới thiệu

**Enum Manager** là Editor Window cho phép bạn **thêm/xóa enum values** vào `PoolType` hoặc `ParticleType` một cách trực quan, không cần chỉnh sửa code!

**Menu**: `Tools → Pool Pattern → Enum Manager`

---

## 🎯 Cách mở

### Cách 1: Từ Menu
```
Unity Editor → Tools → Pool Pattern → Enum Manager
```

### Cách 2: Shortcut (nếu có)
```
Ctrl + Shift + E (có thể custom)
```

---

## 🎨 Giao diện

```
┌────────────────────────────────────────────┐
│  🔧 Pool Pattern Enum Manager              │
│  Add new PoolType or ParticleType values   │
├────────────────────────────────────────────┤
│                                            │
│  [📦 PoolType]    [🎆 ParticleType]        │ ← Chọn loại enum
│                                            │
├────────────────────────────────────────────┤
│  Current PoolType Values:                  │
│  ┌──────────────────────────────────────┐ │
│  │  0    None                            │ │
│  │  1    Type                            │ │
│  │  2    Bullet_Player    [🗑️ Delete]   │ │ ← Danh sách hiện tại
│  │  3    Enemy_Boss       [🗑️ Delete]   │ │
│  └──────────────────────────────────────┘ │
│  Total: 4 values                           │
├────────────────────────────────────────────┤
│  Add New Value:                            │
│  Name: [Bullet_Laser____________]          │ ← Nhập tên
│  ☑ Auto Generate Value                     │ ← Tick để auto
│  Value: [disabled]                         │
│  ✨ Value will be auto-generated...        │
│  Preview: Bullet_Laser = Auto              │
│                                            │
│  [➕ Add to PoolType]  [🧹 Clear]          │ ← Buttons
├────────────────────────────────────────────┤
│  📄 File: Assets/...PoolEnums.cs           │
│                            [📝 Open File]  │
└────────────────────────────────────────────┘
```

---

## 📖 Hướng dẫn sử dụng

### Thêm enum mới

#### Bước 1: Chọn loại enum
```
Click [📦 PoolType] hoặc [🎆 ParticleType]
```

#### Bước 2: Nhập tên
```
Name: Bullet_Laser
```

#### Bước 3: Chọn giá trị
**Option A: Auto Generate (Khuyên dùng)**
```
☑ Auto Generate Value
→ Giá trị = max_value + 1 (tự động)
```

**Option B: Manual Value**
```
☐ Auto Generate Value
Value: 100
→ Giá trị = 100 (thủ công)
```

#### Bước 4: Add
```
Click [➕ Add to PoolType]
→ Done!
```

---

### Xóa enum

#### Bước 1: Tìm enum trong list
```
Current PoolType Values:
  2    Bullet_Player    [🗑️ Delete]  ← Click nút này
```

#### Bước 2: Confirm
```
Dialog hiện:
"Are you sure you want to delete 'Bullet_Player'?"
[Yes] [No]

Click [Yes] → Deleted!
```

**⚠️ Lưu ý**: Không thể xóa `None` (enum mặc định)

---

## 🎨 Features

### 1. Visual Enum List
- ✅ Hiển thị tất cả enum values hiện có
- ✅ Sắp xếp theo giá trị (value)
- ✅ Scroll view cho danh sách dài
- ✅ Hiển thị tổng số values

### 2. Add New Value
- ✅ Input field cho tên
- ✅ Auto-generate hoặc manual value
- ✅ Preview trước khi add
- ✅ Validation đầy đủ

### 3. Delete Value
- ✅ Delete button cho mỗi enum
- ✅ Confirm dialog
- ✅ Không thể xóa `None`

### 4. Quick Actions
- ✅ Clear fields button
- ✅ Open file button
- ✅ Auto refresh Unity

---

## ✅ Validation

### Tên enum:
- ❌ Không được rỗng
- ❌ Không chứa ký tự đặc biệt
- ✅ Chỉ chữ cái, số, underscore
- ✅ Bắt đầu bằng chữ cái hoặc underscore
- ❌ Không trùng tên đã có

### Giá trị:
- ✅ Phải là số nguyên (nếu manual)
- ✅ Auto = max + 1

---

## 💡 Examples

### Example 1: Thêm Bullet Types

```
1. Mở Enum Manager
2. Click [📦 PoolType]
3. Name: Bullet_Player
4. ☑ Auto Generate Value
5. [➕ Add]

Result:
public enum PoolType
{
    None = 0,
    Type = 1,
    Bullet_Player = 2,  ← New!
}
```

---

### Example 2: Categorize với Manual Values

```
Thêm Bullets (100-199):
1. Name: Bullet_Normal
2. ☐ Auto Generate
3. Value: 100
4. [➕ Add]

Thêm Enemies (200-299):
1. Name: Enemy_Normal
2. ☐ Auto Generate
3. Value: 200
4. [➕ Add]

Result:
public enum PoolType
{
    None = 0,
    Type = 1,
    Bullet_Normal = 100,  ← Categorized
    Enemy_Normal = 200,   ← Categorized
}
```

---

### Example 3: Thêm Particle Types

```
1. Click [🎆 ParticleType]
2. Name: Explosion_Big
3. ☑ Auto Generate
4. [➕ Add]

Result:
public enum ParticleType
{
    None = 0,
    Type = 1,
    Explosion_Big = 2,  ← New!
}
```

---

### Example 4: Xóa enum không dùng

```
1. Tìm "Type" trong list
2. Click [🗑️ Delete]
3. Confirm [Yes]

Result:
public enum PoolType
{
    None = 0,
    // Type = 1,  ← Deleted!
    Bullet_Player = 2,
}
```

---

## 🎯 Best Practices

### ✅ DO

1. **Dùng Auto Generate** cho đơn giản
```
☑ Auto Generate Value
→ Tránh conflict
```

2. **Naming convention rõ ràng**
```
✅ Bullet_Player
✅ Enemy_Boss
✅ Item_HealthPotion
```

3. **Categorize bằng hundreds**
```
Bullets: 100-199
Enemies: 200-299
Items: 300-399
```

4. **Review trước khi Add**
```
Xem Preview:
"Preview: Bullet_Laser = 2"
→ Check trước khi click Add
```

---

### ❌ DON'T

1. **Đặt tên không rõ ràng**
```
❌ b1, e1, test
✅ Bullet_Player
```

2. **Xóa enum đang được dùng**
```
❌ Xóa enum đang có prefab sử dụng
→ Sẽ gây lỗi reference!
```

3. **Manual value trùng**
```
❌ Bullet_1 = 100
❌ Enemy_1 = 100  // Conflict!
```

---

## 🔄 Workflow

### Workflow hoàn chỉnh

```
1. Open Enum Manager
   ↓
2. Select PoolType/ParticleType
   ↓
3. View current values
   ↓
4. Add new value
   ↓
5. Unity auto-refresh
   ↓
6. Use new enum in code/Inspector
```

---

## 🎨 Color Coding

- **📦 PoolType**: Màu xanh lá (Green)
- **🎆 ParticleType**: Màu vàng (Orange)
- **➕ Add Button**: Màu theo enum type
- **🗑️ Delete Button**: Màu đỏ (Red)

---

## 🐛 Troubleshooting

### Problem: Window không mở

**Check**:
- Menu có item "Tools → Pool Pattern → Enum Manager"?
- File `EnumManagerWindow.cs` trong folder `Editor`?

**Fix**:
```
1. Check Console có error không
2. Reimport script: Right-click → Reimport
3. Restart Unity
```

---

### Problem: Add không hoạt động

**Check**:
- Tên có hợp lệ không?
- Value có đúng format không?
- File PoolEnums.cs có bị lock không?

**Fix**:
```
1. Check validation errors
2. Check file permissions
3. Close file nếu đang mở trong editor khác
```

---

### Problem: Enum không xuất hiện sau khi Add

**Check**:
- Unity đã refresh chưa?
- File có syntax error không?

**Fix**:
```
1. Assets → Refresh (Ctrl + R)
2. Check Console
3. Reopen scene
```

---

## 📊 Statistics Display

Window hiển thị thống kê:
- **Total values**: Tổng số enum values
- **Value range**: Min-Max values
- **File path**: Đường dẫn file PoolEnums.cs

---

## ⌨️ Keyboard Shortcuts (Planned)

- `Ctrl + N`: Clear fields
- `Ctrl + Enter`: Add new value
- `Delete`: Delete selected value
- `Ctrl + R`: Refresh list

---

## 🎯 Comparison

### vs Manual Edit

| Feature | Manual Edit | Enum Manager |
|---------|------------|--------------|
| **Speed** | Slow | ⚡ Fast |
| **Visual** | No | ✅ Yes |
| **Validation** | Manual | ✅ Auto |
| **Preview** | No | ✅ Yes |
| **Delete** | Manual | ✅ 1-click |
| **User-friendly** | ❌ No | ✅ Yes |

---

## ✨ Summary

**Enum Manager** giúp bạn:

1. ✅ **Thêm enum** trực quan
2. ✅ **Xóa enum** dễ dàng
3. ✅ **Xem danh sách** hiện tại
4. ✅ **Validation** tự động
5. ✅ **Preview** trước khi add
6. ✅ **Auto-refresh** Unity

**Không cần edit code thủ công nữa!**

---

**Menu**: `Tools → Pool Pattern → Enum Manager`

**Author**: quocbr  
**Date**: February 5, 2026  
**Version**: 1.0

**Enjoy easy enum management!** 🚀✨
