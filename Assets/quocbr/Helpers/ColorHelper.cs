/*
Github: https://github.com/quocbr
Updated: Refactored for better logic & extensions
*/

using UnityEngine;

namespace quocbr.Helpers
{
    public static class ColorHelper
    {
        #region 1. Random Generators (HSV Based)

        public static Color GetRandomPastelColor(float alpha = 1f)
        {
            // Pastel: Saturation trung bình thấp (0.3-0.6), Value cao (0.8-1.0)
            return GetRandomColor(0f, 1f, 0.3f, 0.6f, 0.8f, 1f, alpha);
        }

        public static Color GetRandomVividColor(float alpha = 1f)
        {
            // Vivid: Saturation cao (0.7-1.0), Value cao (0.8-1.0)
            return GetRandomColor(0f, 1f, 0.7f, 1f, 0.8f, 1f, alpha);
        }

        public static Color GetRandomDarkColor(float alpha = 1f)
        {
            // Dark: Value thấp
            return GetRandomColor(0f, 1f, 0.5f, 1f, 0.2f, 0.4f, alpha);
        }

        /// <summary>
        /// Hàm gốc để random màu theo khoảng HSV tùy chỉnh
        /// </summary>
        public static Color GetRandomColor(
            float minHue = 0f, float maxHue = 1f,
            float minSat = 0f, float maxSat = 1f,
            float minVal = 0f, float maxVal = 1f,
            float alpha = 1f)
        {
            float h = Random.Range(minHue, maxHue);
            float s = Random.Range(minSat, maxSat);
            float v = Random.Range(minVal, maxVal);
            
            Color c = Color.HSVToRGB(h, s, v);
            c.a = alpha;
            return c;
        }

        #endregion

        #region 2. Contrast & Readability (Fixed Logic)

        /// <summary>
        /// Trả về màu Đen hoặc Trắng tùy thuộc vào độ sáng của background.
        /// Dùng cho text hiển thị trên nền màu động.
        /// </summary>
        public static Color GetReadableTextColor(Color backgroundColor)
        {
            // Nếu nền tối (< 0.5) -> Text trắng. Nền sáng -> Text đen (hoặc xám đậm 0.1f cho dịu mắt)
            return CalculateLuminance(backgroundColor) < 0.5f ? Color.white : new Color(0.1f, 0.1f, 0.1f);
        }

        /// <summary>
        /// Tạo một cặp màu tương phản ngẫu nhiên (VD: Nền và Text)
        /// Sử dụng Tuple (C# 7+) thay vì KeyValuePair
        /// </summary>
        public static (Color Background, Color Foreground) GetRandomContrastPair()
        {
            Color bg = GetRandomPastelColor();
            
            // Tính độ sáng của BG vừa tạo
            float lum = CalculateLuminance(bg);
            
            Color fg;
            if (lum < 0.5f)
            {
                // Nền tối -> Chọn màu sáng (Pastel hoặc Vivid sáng)
                fg = GetRandomColor(0f, 1f, 0.3f, 0.6f, 0.8f, 1f); 
            }
            else
            {
                // Nền sáng -> Chọn màu tối
                fg = GetRandomDarkColor();
            }

            return (bg, fg);
        }

        #endregion

        #region 3. Color Theory Utilities

        /// <summary>
        /// Lấy màu bổ túc (đối diện trên vòng tròn màu)
        /// </summary>
        public static Color GetComplementaryColor(Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            
            // Cộng thêm 0.5 (180 độ) để lấy màu đối diện
            float newH = (h + 0.5f) % 1f;
            
            return Color.HSVToRGB(newH, s, v);
        }

        /// <summary>
        /// Tính độ chói (Luminance) chuẩn công thức W3C
        /// </summary>
        public static float CalculateLuminance(Color color)
        {
            // Linearize giá trị RGB trước khi tính toán (Gamma correction)
            float r = color.r <= 0.03928f ? color.r / 12.92f : Mathf.Pow((color.r + 0.055f) / 1.055f, 2.4f);
            float g = color.g <= 0.03928f ? color.g / 12.92f : Mathf.Pow((color.g + 0.055f) / 1.055f, 2.4f);
            float b = color.b <= 0.03928f ? color.b / 12.92f : Mathf.Pow((color.b + 0.055f) / 1.055f, 2.4f);
            
            return 0.2126f * r + 0.7152f * g + 0.0722f * b;
        }

        #endregion
    }

    /// <summary>
    /// Các phương thức mở rộng giúp code gọn gàng hơn: myColor.ToHex()
    /// </summary>
    public static class ColorExtensions
    {
        // Thay đổi Alpha nhanh: myColor.WithAlpha(0.5f)
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        // Làm sáng màu: myColor.Lighten(0.2f)
        public static Color Lighten(this Color color, float amount)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            v = Mathf.Clamp01(v + amount);
            return Color.HSVToRGB(h, s, v);
        }

        // Làm tối màu: myColor.Darken(0.2f)
        public static Color Darken(this Color color, float amount)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            v = Mathf.Clamp01(v - amount);
            return Color.HSVToRGB(h, s, v);
        }

        // Chuyển Color sang Hex String (#RRGGBB)
        public static string ToHex(this Color color, bool includeAlpha = false)
        {
            return includeAlpha ? ColorUtility.ToHtmlStringRGBA(color) : ColorUtility.ToHtmlStringRGB(color);
        }

        // Tạo Color từ Hex String
        public static Color FromHex(this string hex)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;
            
            Debug.LogWarning($"Invalid Hex Color: {hex}");
            return Color.white;
        }
    }
}