# Project Initial — thư viện module Unity của Quocbr

Project này tập hợp các script, prefab và công cụ Unity có thể tái sử dụng. Bạn có thể mở toàn bộ project để phát triển module, hoặc dùng **Quocbr Module Installer** để chọn từng phần và tích hợp vào project khác.

## Bắt đầu

1. Clone repository và mở thư mục project bằng Unity Hub. Phiên bản Editor của checkout được ghi trong [`ProjectSettings/ProjectVersion.txt`](ProjectSettings/ProjectVersion.txt).
2. Để làm việc với **toàn bộ project**, cài các SDK bên ngoài mà mã trong `Assets/quocbr/SDK` sử dụng theo mục [Phụ thuộc](#phụ-thuộc). Một số SDK không được lưu trong Git nên bản clone mới có thể báo lỗi biên dịch cho đến khi cài đủ.
3. Mở `Assets/Scenes/SampleScene.unity` nếu muốn xem scene mẫu. Mã tái sử dụng nằm trong `Assets/quocbr`.

Nếu chỉ muốn **lấy module sang project khác**, project nguồn có thể là một bản clone trên ổ đĩa. Import installer vào project đích rồi trỏ panel đến thư mục bản clone; không cần mở project nguồn trong Unity.

## Các phần chính

| Đường dẫn | Nội dung |
| --- | --- |
| [`Assets/quocbr/DesignPattern`](Assets/quocbr/DesignPattern) | Singleton, event manager, state machine, factory và pooling. `Drafts` là bản nháp, không nằm trong module runtime của installer. |
| [`Assets/quocbr/Helpers`](Assets/quocbr/Helpers), [`Constants`](Assets/quocbr/Constants), [`Common`](Assets/quocbr/Common) | Hàm trợ giúp, hằng số và mã dùng chung. |
| [`Assets/quocbr/UISystem`](Assets/quocbr/UISystem), [`PopupSystem`](Assets/quocbr/PopupSystem) | Quản lý canvas, màn hình và popup. |
| [`Assets/quocbr/DebugLogger`](Assets/quocbr/DebugLogger), [`Tags`](Assets/quocbr/Tags) | Công cụ debug và căn sprite theo màn hình. |
| [`Assets/quocbr/SDK`](Assets/quocbr/SDK), [`Booster`](Assets/quocbr/Booster) | Quảng cáo, analytics, remote config, IAP, native ad, runtime và booster. |
| [`Assets/quocbr/Installer`](Assets/quocbr/Installer) | Editor panel để cài và gỡ module trong project đích. |
| `Assets/Scenes`, `Assets/Settings` | Scene mẫu và cấu hình render của project nguồn. |

Danh sách module, các file được sao chép và phụ thuộc tương ứng được định nghĩa trong [`QuocbrModuleCatalog.cs`](Assets/quocbr/Installer/Editor/QuocbrModuleCatalog.cs). Tài liệu chi tiết hơn về cách tổ chức script nằm ở [`Assets/quocbr/README.md`](Assets/quocbr/README.md).

## Tích hợp vào project khác

1. Chuẩn bị gói `Quocbr-Module-Installer-v0.3.unitypackage`: tải từ GitHub Release khi asset đã được đính kèm, hoặc mở project nguồn đã cài đủ phụ thuộc và chọn **Tools > Quocbr > Export Installer Package** để tự export. File `.unitypackage` không được lưu trực tiếp trong Git.
2. Import gói vào project đích, rồi mở **Tools > Quocbr > Module Installer**.
3. Chọn thư mục project nguồn. Bấm vào thẻ của những module muốn dùng; panel sẽ tự thêm module phụ thuộc.
4. Kiểm tra danh sách plugin. Panel tự cài plugin được hỗ trợ; những plugin cần cài thủ công có liên kết đến nguồn tương ứng.
5. Nhấn **Cài ... module**. Khi cần gỡ, dùng tab **Gỡ module** trong cùng panel.

Panel chép file cùng `.meta` để giữ GUID, kiểm tra xung đột trước khi chép và lưu danh sách asset do nó quản lý trong project đích. Khi gỡ, panel giữ lại asset dùng chung và plugin bên ngoài. Xem [`hướng dẫn installer`](Assets/quocbr/Installer/README.md) và [`ghi chú v0.3`](Releases/v0.3.md) để biết các giới hạn hiện tại.

## Phụ thuộc

Các module lõi như Singleton, Event Manager, State Machine, Factory và Mini Pool có thể chọn riêng. UI, pooling mở rộng và đặc biệt là Mobile SDK Bundle cần thêm plugin. Mobile SDK Bundle hiện là một lựa chọn chung vì mã có nhiều tham chiếu chéo.

| Plugin / package | Cách xử lý trong installer |
| --- | --- |
| Unity UI, TextMeshPro, Unity Purchasing, External Dependency Manager | Cài qua Unity Package Manager khi cần. |
| [Adjust Unity SDK](https://dev.adjust.com/en/sdk/unity/) | Cài qua Git URL của Unity Package Manager. |
| [Google Mobile Ads Unity 11.6.0](https://github.com/googleads/googleads-mobile-unity/releases/tag/v11.6.0), [AppLovin MAX Unity 8.6.6](https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin/releases/tag/release_8_6_6) | Tải gói GitHub Release đã ghim và kiểm tra SHA-256 trước khi import. |
| [Firebase Unity SDK](https://github.com/firebase/firebase-unity-sdk/releases), [Google Play Review](https://developers.google.com/unity/packages) | Cài thủ công theo nguồn chính thức. |
| [DOTween](https://dotween.demigiant.com/), [Odin Inspector](https://odininspector.com/) | Cần plugin và giấy phép phù hợp; panel hướng dẫn cài thủ công nếu project đích chưa có. |

Repository hiện có một số file DOTween và Odin, nhưng không lưu đầy đủ các SDK mobile như Firebase, Google Mobile Ads, AppLovin MAX và Google Play Plugins. Khi mở toàn bộ project từ bản clone mới, hãy cài các SDK cần dùng và kiểm tra `Packages/manifest.json`. Trước khi phát hành game, kiểm tra lại app ID, ad unit ID, cấu hình Firebase, sản phẩm IAP và thiết lập nền tảng cho project đích.

## Đóng góp module

- Đặt mã tái sử dụng trong `Assets/quocbr`, kèm file `.meta` của asset và thư mục Unity.
- Khi thêm hoặc đổi đường dẫn module, cập nhật catalog của installer và thử cài vào một project Unity trống.
- Đặt scene, prefab và script chỉ dùng để thử nghiệm ở khu vực mẫu; tránh thêm chúng vào module runtime nếu không cần thiết.
