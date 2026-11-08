using System;
using System.Collections.Generic;
using System.Linq;
// using Spine;
// using Spine.Unity;
using UnityEngine;

public static class Ultils
{
    private static Vector2Int bannerScreenSize;
    private static float bannerHeightPixels;
    private static float nextBannerSizeCheck;
    private static bool bannerSizeReady;

    public static float GetBannerHeightPixels()
    {
        if (VariableSystem.RemoveAds || VariableSystem.RemoveAdsHack) return 0f;

        Vector2Int screenSize = new(Screen.width, Screen.height);
        if (bannerScreenSize == screenSize &&
            (bannerSizeReady || Time.unscaledTime < nextBannerSizeCheck))
        {
            return bannerHeightPixels;
        }

        // MAX returns dp/points. Convert to screen pixels before using Canvas or camera coordinates.
        float height = 50f;
        float density = 1f;
#if UNITY_EDITOR
        bannerSizeReady = true;
#elif UNITY_ANDROID || UNITY_IOS
        density = MaxSdkUtils.GetScreenDensity();
        bannerSizeReady = MaxSdk.IsInitialized();
        if (bannerSizeReady)
        {
            float adaptiveHeight = MaxSdkUtils.GetAdaptiveBannerHeight();
            if (adaptiveHeight > 0f) height = adaptiveHeight;
            else bannerSizeReady = false;
        }
#else
        bannerSizeReady = true;
#endif
        bannerScreenSize = screenSize;
        bannerHeightPixels = height * (density > 0f ? density : 1f);
        // Retry until SDK initialization completes; avoid native SDK calls every frame.
        nextBannerSizeCheck = Time.unscaledTime + 0.5f;
        return bannerHeightPixels;
    }

    public static Vector3 UIToWorldOverLay(RectTransform uiElement)
    {
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, uiElement.position);
        screenPos.z = Camera.main.nearClipPlane;

        return Camera.main.ScreenToWorldPoint(screenPos);
    }


    #region▶ Convert

    public static string ConvertIntToText(int value)
    {
        if (value < 10000)
        {
            return value.ToString();
        }

        if (value >= 10000 && value < 1000000)
        {
            return (value / 1000f).ToString(".00") + "K";
        }

        if (value >= 1000000 && value < 1000000000)
        {
            return ((float)value / 1000000).ToString(".00") + "M";
        }

        return ((float)value / 1000000000).ToString(".00") + "B";
    }

    public static Vector3 ConvertWorldToUIPosition(Vector3 worldPosition, RectTransform canvasRect)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvasRect.GetComponentInParent<Canvas>().worldCamera,
            out Vector2 uiPosition
        );

        return uiPosition;
    }
    public static Vector3 ConvertWorldToUIOverlayPosition(Vector3 worldPosition, RectTransform canvasRect)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            null, // 👈 QUAN TRỌNG cho Overlay
            out Vector2 uiPosition
        );

        return uiPosition;
    }

    public static Vector3 ConvertUIToWorldPosition(RectTransform uiElement)
    {
        // Get the screen position of the UI element
        Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, uiElement.position);

        // Convert the screen position to world position
        Vector3 worldPosition =
            Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y,
                Camera.main.nearClipPlane));

        return worldPosition;
    }

    public static Vector3 UIToWorld(RectTransform rectTransform, Camera worldCamera)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);

        float distance = Mathf.Abs(worldCamera.transform.position.z);

        Vector3 worldPoint = worldCamera.ScreenToWorldPoint(
            new Vector3(screenPoint.x, screenPoint.y, distance));

        worldPoint.z = 0f;

        return worldPoint;
    }

    public static Vector3 UIToWorld3D(RectTransform rectTransform, Camera worldCamera)
    {
        // 1. UI position → screen point (Overlay)
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);

        // 2. Screen point → ray từ camera xuyên qua màn hình
        Ray ray = worldCamera.ScreenPointToRay(screenPoint);

        // 3. Giao ray với mặt phẳng y=0 (mặt đất 3D)
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);  // ← điểm chính xác trên mặt đất 3D
        }

        // Fallback
        return Vector3.zero;
    }

    #endregion

    #region ▶ Time Conversion

    public static string ReturnSecondsToMinutes(int seconds)
    {
        if (seconds / 60 > 0) return (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
        return "00:" + (seconds % 60).ToString("00");
    }

    public static string ConvertSecondsToHours(int seconds)
    {
        int hours = seconds / 3600;
        int remainingSeconds = seconds % 3600;
        int minutes = remainingSeconds / 60;
        remainingSeconds %= 60;

        return $"{hours:D2}:{minutes:D2}:{remainingSeconds:D2}";
    }

    public static string FormatTime(TimeSpan time)
    {
        if (time.TotalDays >= 1)
            return $"{(int)time.TotalDays:00}:{time.Hours:00}:{time.Minutes:00}"; // dd:hh:mm

        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}"; // hh:mm:ss

        return $"{time.Minutes:00}:{time.Seconds:00}"; // mm:ss
    }

    public static string ReturnSecondsToDays(TimeSpan time)
    {
        if (time.TotalDays >= 1)
            return $"{(int)time.TotalDays:00}d{time.Hours:00}h";

        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours:00}h{time.Minutes:00}m";

        if (time.TotalMinutes >= 1)
            return $"{(int)time.TotalMinutes:00}m{time.Seconds:00}s";

        return $"00:{time.Seconds:00}";
    }



    public static string ReturnSecondsToDays(int seconds)
    {
        int days = seconds / 86400;
        seconds %= 86400;
        int hours = seconds / 3600;
        seconds %= 3600;
        int minutes = seconds / 60;
        seconds %= 60;

        if (days > 0)
            return $"{days}d{(hours > 0 ? $"{hours}h" : "")}";
        if (hours > 0)
            return $"{hours}h{(minutes > 0 ? $"{minutes:00}m" : "")}";
        if (minutes > 0)
            return $"{minutes}m {(seconds > 0 ? $"{seconds:00}s" : "")}";
        if (seconds > 0)
            return $"{seconds:00}s";
        return "0";
    }

    #endregion

    // #region▶ Animation Spine

    // public static void PlayAnimation(SkeletonAnimation skeletonAnimation, string animationName,
    //     Action onStart = null, Action onComplete = null, bool loop = false,
    //     float timeScale = 1f)
    // {
    //     Spine.AnimationState.TrackEntryDelegate completeDelegate = null;
    //     completeDelegate = (trackEntry) =>
    //     {
    //         if (trackEntry.Animation.Name == animationName)
    //         {
    //             onComplete?.Invoke();
    //             skeletonAnimation.AnimationState.Complete -= completeDelegate;
    //         }
    //     };

    //     skeletonAnimation.AnimationState.ClearTracks();
    //     skeletonAnimation.timeScale = timeScale;
    //     skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
    //     skeletonAnimation.AnimationState.Complete += completeDelegate;
    //     onStart?.Invoke();
    // }

    // public static void PlayAnimationUI(SkeletonGraphic skeletonGraphic, string animationName, string skinName = null,
    //     Action onStart = null, Action onComplete = null, bool loop = false,
    //     float timeScale = 1f)
    // {
    //     Spine.AnimationState.TrackEntryDelegate completeDelegate = null;
    //     completeDelegate = (trackEntry) =>
    //     {
    //         if (trackEntry.Animation.Name == animationName)
    //         {
    //             onComplete?.Invoke();
    //             skeletonGraphic.AnimationState.Complete -= completeDelegate;
    //         }
    //     };
    //     if (!string.IsNullOrEmpty(skinName))
    //     {
    //         Skin skin = skeletonGraphic.Skeleton.Data.FindSkin(skinName);

    //         if (skin != null)
    //         {
    //             skeletonGraphic.Skeleton.SetSkin(skin);
    //         }
    //         else
    //         {
    //             UnityEngine.Debug.LogWarning($"Skin not found: {skinName}");
    //         }
    //     }

    //     skeletonGraphic.AnimationState.ClearTracks();
    //     skeletonGraphic.timeScale = timeScale;
    //     skeletonGraphic.AnimationState.SetAnimation(0, animationName, loop);
    //     skeletonGraphic.AnimationState.Complete += completeDelegate;
    //     onStart?.Invoke();
    // }

    //#endregion

    #region ▶ Other

    public static Vector3 GetBorderVector()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return Vector3.zero;
        }

        Rect safeArea = Screen.safeArea;

        Vector3 safeAreaTopRight = new Vector3(safeArea.xMax, safeArea.yMax, 0f);

        Vector3 screenBorderWorldPos = mainCamera.ScreenToWorldPoint(safeAreaTopRight);

        return new Vector3(Mathf.Abs(screenBorderWorldPos.x), Mathf.Abs(screenBorderWorldPos.y), 0f);
    }

    public static class GenericCache<TKey, TValue>
    {
        private static readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

        // Retrieves the value associated with the key, or default(TValue) if the key does not exist
        public static TValue Get(TKey key)
        {
            return _cache.GetValueOrDefault(key);
        }

        // Adds a new key-value pair to the cache or updates the value if the key already exists
        public static void Add(TKey key, TValue value)
        {
            _cache[key] = value;
        }

        // Checks if the specified key exists in the cache
        public static bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }

        // Checks if the specified key exists in the cache and retrieves the value if it exists
        public static bool ContainsKey(TKey key, out TValue value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public static void Remove(TKey key)
        {
            // if (!_cache.ContainsKey(key))
            // {
            //     return;
            // }

            _cache.Remove(key);
        }

        public static void Clear()
        {
            _cache.Clear();
        }
    }

    #endregion

    public static List<Vector3> SimplifyPath(List<Vector3> path, float tolerance = 0.001f)
    {
        if (path == null || path.Count <= 2)
            return path?.ToList() ?? new List<Vector3>();

        List<Vector3> result = new List<Vector3> { path[0] };

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector3 prev = path[i - 1];
            Vector3 curr = path[i];
            Vector3 next = path[i + 1];

            Vector3 dir1 = curr - prev;
            Vector3 dir2 = next - curr;

            // Bỏ qua điểm trùng hoặc quá gần
            if (dir1.sqrMagnitude < tolerance * tolerance ||
                dir2.sqrMagnitude < tolerance * tolerance)
            {
                continue; // hoặc result.Add(curr) nếu muốn giữ điểm gần
            }

            // Kiểm tra colinear bằng cross product (rất chính xác)
            Vector3 cross = Vector3.Cross(dir1.normalized, dir2.normalized);
            float sinAngle = cross.magnitude; // |sin(theta)|

            if (sinAngle > tolerance) // Không thẳng hàng
            {
                result.Add(curr);
            }
        }

        result.Add(path[^1]);

        return result;
    }


}
