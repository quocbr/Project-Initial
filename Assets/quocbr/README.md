# Các module Unity tái sử dụng

Script tự viết được nhóm theo chức năng. Khi chuyển một script, luôn chuyển cả file
`.meta` để Unity giữ liên kết từ scene và prefab.

| Thư mục | Chức năng |
| --- | --- |
| `DesignPattern` | Singleton, event manager, factory, state machine và pooling. |
| `DesignPattern/Drafts` | Bản nháp object pool còn rỗng, chưa phải module runtime. |
| `Helpers`, `Constants`, `Common` | Hàm trợ giúp, hằng số và chẩn đoán. |
| `UISystem`, `PopupSystem` | Quản lý canvas và popup. |
| `Booster` | Cấu hình và quản lý booster. |
| `SDK/Ads` | Điều phối, cấu hình và adapter quảng cáo. |
| `SDK/Analytics` | Ghi sự kiện Firebase. |
| `SDK/Config` | Remote config. |
| `SDK/IAP` | Sản phẩm, giao dịch và phần thưởng IAP. |
| `SDK/Native` | Native ad và chuyển việc về main thread. |
| `SDK/Runtime` | Khởi tạo, biến lưu và bộ đếm thời gian. |
| `SDK/UI`, `SDK/Utilities` | UI và tiện ích phục vụ SDK. |
| `DebugLogger`, `Tags`, `ScriptTemplates` | Debug, căn sprite theo màn hình và mẫu script. |
| `Installer` | Panel chọn module và cài vào project Unity khác. |

Script và prefab thử nghiệm nằm ở `Assets/Samples/Tests`. Các gói bên thứ ba vẫn
nằm trong thư mục hiện có dưới `Assets` và `Packages`.

`EnumManagerWindow` dùng đường dẫn cố định tới `DesignPattern/Pooling/Core/PoolEnums.cs`.
Cần cập nhật hằng số này nếu tiếp tục di chuyển file enum.

Xem [hướng dẫn Module Installer](Installer/README.md) để xuất bộ cài và chọn
module trong project đích.
