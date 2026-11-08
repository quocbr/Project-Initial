# Quocbr Module Installer

## Cài panel vào project đích

### Qua Git URL (Unity 6)

Mở **Window > Package Manager**, chọn **+ > Add package from git URL...** và nhập:

```text
https://github.com/quocbr/Project-Initial.git?path=/Assets/quocbr/Installer#v0.4
```

URL này lấy đúng thư mục installer tại tag `v0.4`. Có thể bỏ `#v0.4` để lấy commit mới nhất từ nhánh mặc định. Máy chạy Unity cần có Git trong `PATH`. Package chỉ chứa panel; hãy clone repository về máy để làm project nguồn khi chọn module.

### Qua file `.unitypackage`

Trong project nguồn, mở Unity và chọn **Tools > Quocbr > Export Installer Package**.
Chỉ thư mục `Assets/quocbr/Installer` được xuất. Import file `.unitypackage`
vừa tạo vào project đích. Chỉ dùng một trong hai cách cài panel trong cùng project.

## Cài module

1. Sau khi cài panel, mở **Tools > Quocbr > Module Installer** trong project đích.
2. Chọn thư mục gốc của project nguồn, nơi có `Assets/quocbr`. Project nguồn
   phải khác project đích đang mở.
3. Tìm kiếm hoặc chọn nhóm module, rồi bấm vào bất kỳ vị trí nào trên thẻ
   module để chọn hoặc bỏ chọn. Có thể chọn nhanh các mục đang hiển thị.
4. Mở mục **Plugin cần thiết** để xem trạng thái. Nút **Nguồn** mở trang
   phát hành hoặc hướng dẫn chính thức; **Cài ngay** cài một plugin tự động
   dù module của nó chưa sẵn sàng.
5. Cài plugin được đánh dấu **Cần cài thủ công** từ nguồn hợp lệ của bạn.
6. Xem số module sẵn sàng ở thanh cuối panel, rồi nhấn **Cài ... module**.
   Panel cài UPM package, tải và kiểm tra các
   GitHub release được hỗ trợ, rồi chép asset và file `.meta`.

Nếu một module thiếu plugin thủ công, panel bỏ qua module đó và những module
phụ thuộc vào nó. Các module đủ điều kiện vẫn được cài. Sau khi xử lý plugin,
chạy Install lần nữa.

Google Mobile Ads được ghim ở bản 11.6.0 và AppLovin MAX ở bản 8.6.6,
trùng phiên bản đang dùng trong project nguồn. Panel kiểm tra SHA-256 của
file tải về trước khi import. Adjust được cài qua Git URL UPM trong
[hướng dẫn chính thức](https://dev.adjust.com/en/sdk/unity/).

Panel không ghi đè asset có nội dung hoặc GUID khác trong project đích.
Plugin đã có trong `Packages/manifest.json` được giữ nguyên phiên bản.
Những module SDK mobile hiện được đóng thành một lựa chọn vì có nhiều
tham chiếu chéo; bạn cần cấu hình app ID, store product và Firebase của
project đích sau khi import.

## Gỡ module

Mở tab **Gỡ module**, bấm vào thẻ của các module muốn gỡ rồi xem số asset
trong thanh cuối panel. Nếu module khác còn phụ thuộc, hãy chọn gỡ cả module
đó. Nhấn **Gỡ ... module** và xác nhận để xóa các asset do panel đã sao chép.

Panel lưu danh sách asset được quản lý trong
`ProjectSettings/QuocbrInstallerRegistry.json`. Tệp đã được sửa sau khi cài
sẽ chặn thao tác gỡ để tránh mất thay đổi. Asset có sẵn trước khi cài và
plugin bên ngoài không bị xóa; chúng có thể được dùng ở nơi khác. Những
module đã cài trước khi có registry cần được gỡ thủ công.

## Giới hạn hiện tại

- Project nguồn phải có sẵn trên máy đang chạy Unity; bộ cài không tải mã
  Quocbr từ Internet.
- Odin và DOTween cần nguồn cài đặt hợp lệ. Firebase cung cấp các gói đã
  dựng sẵn qua trang phân phối riêng, còn Google Play Plugins chuyển
  sang trang Google APIs for Unity; panel đưa liên kết và bước xử lý
  thủ công cho các trường hợp này.
- Phiên bản package UPM đã có trong project đích không bị thay đổi tự động.
