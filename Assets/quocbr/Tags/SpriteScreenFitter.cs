using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace quocbr
{
    // [ExecuteAlways] giúp chạy logic ngay trong Editor mà không cần Play
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteScreenFitter : MonoBehaviour
    {
        public enum FitMode
        {
            NONE = 0,
            STRETCH_TO_FIT = 1, // Kéo dãn, méo hình để vừa khít
            FIT_INSIDE = 2,     // Giữ tỉ lệ, nằm trọn trong màn hình (có thể thừa viền)
            FILL_SCREEN = 3,    // Giữ tỉ lệ, lấp đầy màn hình (có thể bị crop)
            FIT_WIDTH = 4,      // Khớp theo chiều ngang
            FIT_HEIGHT = 5      // Khớp theo chiều dọc
        }
        
        [Header("Settings")]
        [SerializeField] private FitMode fitMode = FitMode.FIT_INSIDE;
        [SerializeField] private bool updateContinuously = false;
        [Tooltip("Giữ tỉ lệ gốc của ảnh trong các chế độ Fit/Fill")]
        [SerializeField] private bool maintainAspectRatio = true;

        [Header("Padding (Unity Units)")]
        [SerializeField] private float paddingHorizontal = 0f;
        [SerializeField] private float paddingVertical = 0f;
        
        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Cache để tối ưu hiệu năng
        private Vector2 _lastScreenSize;
        private float _lastCamSize;
        private Sprite _lastSprite;

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            FitToScreen();
        }

        private void Update()
        {
            // Trong Editor mode thì luôn update để preview
            // Trong Play mode thì chỉ update khi có cờ updateContinuously
            if (Application.isPlaying && !updateContinuously) return;

            CheckAndFit();
        }
        
        // Gọi khi thay đổi giá trị trên Inspector
        private void OnValidate()
        {
            Initialize();
            FitToScreen();
        }

        #endregion

        #region Core Logic

        private void Initialize()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void CheckAndFit()
        {
            if (targetCamera == null) return;

            // Kiểm tra xem có gì thay đổi không mới tính toán lại (Tối ưu performance)
            float currentCamHeight = targetCamera.orthographicSize;
            Vector2 currentScreen = new Vector2(Screen.width, Screen.height);
            Sprite currentSprite = spriteRenderer ? spriteRenderer.sprite : null;

            if (_lastCamSize != currentCamHeight || 
                _lastScreenSize != currentScreen || 
                _lastSprite != currentSprite)
            {
                FitToScreen();
            }
        }

        public void FitToScreen()
        {
            if (spriteRenderer == null || targetCamera == null) return;
            if (!targetCamera.orthographic)
            {
                Debug.LogWarning("[SpriteScreenFitter] Camera must be Orthographic!");
                return;
            }
            
            Sprite sprite = spriteRenderer.sprite;
            if (sprite == null) return;

            // 1. Tính toán kích thước màn hình (World Units)
            float cameraHeight = targetCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * targetCamera.aspect;

            // 2. Trừ đi Padding
            float targetW = cameraWidth - (paddingHorizontal * 2f);
            float targetH = cameraHeight - (paddingVertical * 2f);
            
            // 3. Lấy kích thước gốc của Sprite (World Units)
            float spriteW = sprite.bounds.size.x / transform.localScale.x;
            float spriteH = sprite.bounds.size.y / transform.localScale.y;

            if (spriteW <= 0 || spriteH <= 0) return;

            // 4. Tính toán Scale cần thiết
            Vector3 finalScale = CalculateScale(spriteW, spriteH, targetW, targetH);
            
            // 5. Apply (Chỉ set nếu khác biệt để tránh dirty flag)
            if (transform.localScale != finalScale)
            {
                transform.localScale = finalScale;
            }

            // Update cache
            _lastCamSize = targetCamera.orthographicSize;
            _lastScreenSize = new Vector2(Screen.width, Screen.height);
            _lastSprite = sprite;
        }

        private Vector3 CalculateScale(float sW, float sH, float tW, float tH)
        {
            float scaleX = tW / sW;
            float scaleY = tH / sH;

            switch (fitMode)
            {
                case FitMode.STRETCH_TO_FIT:
                    return new Vector3(scaleX, scaleY, 1f);

                case FitMode.FIT_INSIDE:
                    // Chọn scale nhỏ nhất để lọt thỏm vào trong
                    if (maintainAspectRatio)
                    {
                        float s = Mathf.Min(scaleX, scaleY);
                        return new Vector3(s, s, 1f);
                    }
                    return new Vector3(scaleX, scaleY, 1f);

                case FitMode.FILL_SCREEN:
                    // Chọn scale lớn nhất để lấp đầy (crop phần thừa)
                    if (maintainAspectRatio)
                    {
                        float s = Mathf.Max(scaleX, scaleY);
                        return new Vector3(s, s, 1f);
                    }
                    return new Vector3(scaleX, scaleY, 1f);

                case FitMode.FIT_WIDTH:
                    float sW_val = maintainAspectRatio ? scaleX : 1f;
                    return new Vector3(scaleX, maintainAspectRatio ? scaleX : scaleY, 1f);

                case FitMode.FIT_HEIGHT:
                    return new Vector3(maintainAspectRatio ? scaleY : scaleX, scaleY, 1f);

                default: // NONE
                    return transform.localScale;
            }
        }

        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SpriteScreenFitter))]
    public class SpriteScreenFitterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Quan trọng: Vẽ giao diện mặc định để hiện các biến
            DrawDefaultInspector();

            SpriteScreenFitter script = (SpriteScreenFitter)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Fit Screen Now"))
            {
                script.FitToScreen();
                EditorUtility.SetDirty(script); // Đánh dấu để lưu scene
            }
            
            // Cảnh báo nếu camera không phải Orthographic
            Camera cam = (Camera)serializedObject.FindProperty("targetCamera").objectReferenceValue;
            if (cam == null) cam = Camera.main;
            
            if (cam != null && !cam.orthographic)
            {
                EditorGUILayout.HelpBox("Camera được chọn không phải là Orthographic! Script này sẽ không hoạt động đúng.", MessageType.Warning);
            }
        }
    }
#endif
}