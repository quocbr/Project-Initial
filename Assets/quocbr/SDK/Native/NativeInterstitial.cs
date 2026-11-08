using AdjustSdk;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class NativeInterstitial : MonoBehaviour
{
#if UNITY_ANDROID
    private AndroidJavaObject nativeAdUtils;
    //private string NativeInterstitial1 = "ca-app-pub-4188057984229952/6765735855";
    //private string NativeInterstitial2 = "ca-app-pub-4188057984229952/4605347071";

    public string nativeKeys;
    private int retryCount = 0; // Số lần thử lại hiện tại
    private const int maxRetries = 3; // Giới hạn số lần thử
    private float retryDelay = 10f;

    void Start()
    {
        // if (VariableSystem.RemoveAds || VariableSystem.RemoveAdsHack) return;
        // if (KeepObject.Instance.mode != TypeMode.Release)
        // {
        //     if (ActionHelper.IsAndroid())
        //     {
        //         nativeKeys = "ca-app-pub-3940256099942544/1044960115";
        //     }
        //     if (ActionHelper.IsIOS())
        //     {
        //         nativeKeys = "ca-app-pub-3940256099942544/2521693316";
        //     }
        // }

        // // Khởi tạo đối tượng từ lớp Java
        // using (AndroidJavaClass nativeAdClass = new AndroidJavaClass("com.bazooka.libnativead.nativead.UnityNativeFullScreen"))
        // {
        //     // Tạo Instance của class
        //     nativeAdUtils = new AndroidJavaObject("com.bazooka.libnativead.nativead.UnityNativeFullScreen");
        // }
        // // Khởi tạo và cấu hình native keys
        // CallSetupNativeKey(nativeKeys);
        // LoadNativeAd();
    }
    /// <summary>
    /// Cấu hình key cho collap, bên native sẽ check load từng key, để nếu key nào lỗi thì tự động load sang key khác, nếu ko thì chỉ cần truyền 1 key duy nhất
    /// </summary>
    /// <param name="nativeKeys"></param>
    private void CallSetupNativeKey(string nativeKeys)
    {
        if (nativeAdUtils != null)
        {
            nativeAdUtils.Call("setupNativeKey", GetActivity(), nativeKeys);
        }

    }
    private AndroidJavaObject GetActivity()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (AndroidJavaClass unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
        return unityActivity.GetStatic<AndroidJavaObject>("currentActivity");
    }
#else
        return null;
#endif
    }

    /// <summary>
    /// Hàm để truyền các biến trong Remote,gọi hàm này khi load được remote từ firebase
    /// </summary>
    /// <param name="v1"></param> bật tắt vùng 1
    /// <param name="v2"></param> bật tắt vùng 2
    /// <param name="v3"></param> bật tắt vùng 3
    /// <param name="ctaPos"></param> Vị trí show cta (LEFT, CENTER, RIGHT, FULL)
    ///  <param name="setCloseButtonSize"> đặt kích thước của nút đóng quảng cáo trong inter (TINY, SMALL, NORMAL, LARGE)
    public void SetControlParram(bool v1, bool v2, bool v3, string ctaPos, string setCloseButtonSize)
    {
        if (nativeAdUtils != null)
        {
            //Phần này là lấy các giá trị từ remote để truyền vào, các e tự setup remote để truyền tham số
            nativeAdUtils.Call("setControlParams", v1, v2, v3, ctaPos);
            nativeAdUtils.Call("setCloseButtonSize", setCloseButtonSize);
        }
    }

    /// <summary>
    /// Kiểm tra quảng cáo collap đã load được chưa
    /// </summary>
    /// <returns></returns>

    public bool IsNativeFullLoaded(bool isLoad = true)
    {
        Debug.Log("IsNativeFullLoaded " + isLoad);
        if (!isLoad) return false;
        if (nativeAdUtils != null)
        {
            return nativeAdUtils.Call<bool>("isNativeFullLoaded");
        }
        else
        {
            LoadNativeAd();
            return false;
        }
    }

    /// <summary>
    /// Hàm load quảng cáo
    /// </summary>
    public void LoadNativeAd()
    {
        if (VariableSystem.RemoveAds || VariableSystem.RemoveAdsHack) return;
        // Gọi phương thức loadNativeFull() trong Java
        if (nativeAdUtils != null)
        {
            ActionHelper.Ad_Load(TypeAds.NativeInter);
            nativeAdUtils.Call("loadNativeFull", gameObject.name);
        }
    }
    private Action<bool> callback;
    /// <summary>
    /// Hàm show quảng cáo
    /// </summary>
    public void ShowNativeFull(Action<bool> callback)
    {
        Debug.Log("ShowNativeFull");
        if (nativeAdUtils != null)
        {
            ActionHelper.Ad_Impression(TypeAds.NativeInter);
            this.callback = callback;
            nativeAdUtils.Call("showNativeFull", gameObject.name);
        }
        else
            callback?.Invoke(true);
    }

    public void onAdClosed()
    {
        ActionHelper.Ad_Show(TypeAds.NativeInter);
        callback?.Invoke(true);
        StartCoroutine(IELoadAds());
    }
    IEnumerator IELoadAds()
    {
        yield return null;
        LoadNativeAd();
    }
    public void onLoadSuccess()
    {
        ActionHelper.Ad_LoadSuccess(TypeAds.NativeInter);
        retryCount = 0;
    }
    public void onAdShowFailed()
    {
        callback?.Invoke(true);
        if (retryCount < maxRetries)
        {
            retryCount += 1;
            Invoke(nameof(LoadNativeAd), retryDelay);
        }

    }
    public void onLoadFail()
    {
        if (retryCount < maxRetries)
        {
            retryCount += 1;
            Invoke(nameof(LoadNativeAd), retryDelay);
        }
    }

    public void onAdLoading()
    {
    }

    /// <summary>
    /// PaidEvent, cần bắn revenue lên adjust,firebase thì thêm code trong hàm này
    /// </summary>
    /// <param name="message"></param>
    public void OnAdPaidEvent(string message)
    {
        // Tách giá trị adValue và currencyCode từ chuỗi
        string[] parts = message.Split('|');
        if (parts.Length == 2)
        {
            double adValue = double.Parse(parts[0]) / 1000000f; // Chuyển đổi adValue từ chuỗi sang số
            string currencyCode = parts[1]; // Lấy currencyCode

            AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(GameHelper.AdjustAdRevenueSourceAdMob);
            adjustAdRevenue.SetRevenue(adValue, currencyCode);
            // track Adjust ad revenue
            Adjust.TrackAdRevenue(adjustAdRevenue);
            //Tracking firebase
            Firebase.Analytics.Parameter[] AdParameters = {
                 new Firebase.Analytics.Parameter("ad_platform", "nativeAd"),
                  new Firebase.Analytics.Parameter("currency",currencyCode),
                new Firebase.Analytics.Parameter("value", adValue)
            };
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdParameters);
        }
        else
        {
            Debug.LogWarning("OnAdPaidEvent received malformed message: " + message);
        }
    }
#endif

#if UNITY_IOS
    public string nativeKeys;
    private int retryCount = 0;//Số lần request hiện tại
    private const int maxRetries = 3;//Giới hạn tối đa số lần request
    private float retryDelay = 10f;
    bool isNativeLoaded;
    [DllImport("__Internal")]
    public static extern void UnityNativeFullSetupNative(string keyNative);
    [DllImport("__Internal")]
    public static extern void UnityNativeFullLoadNative(string objectName);

    [DllImport("__Internal")]
    public static extern void UnityNativeFullShowNative(string objectName);


    [DllImport("__Internal")]
    public static extern void UnityNativeFullSetSizeCTR(string sizeCTR);

    [DllImport("__Internal")]
    public static extern void UnityNativeFullSetControlParam(bool v1, bool v2, bool v3);

    //public GameObject objAds;
    //bool v1, v2, v3;
    //public TextMeshProUGUI txtV1, txtV2, txtV3;
    void Start()
    {
        //Gọi hàm tải quảng 
        //LoadNativeAds();
    }


    public void LoadNativeAds()
    {
        if (VariableSystem.RemoveAds || VariableSystem.RemoveAdsHack) return;
        if (KeepObject.Instance.mode != TypeMode.Release)
        {
            if (ActionHelper.IsAndroid())
            {
                nativeKeys = "ca-app-pub-3940256099942544/1044960115";
            }
            if (ActionHelper.IsIOS())
            {
                nativeKeys = "ca-app-pub-3940256099942544/2521693316";
            }
        }
        isNativeLoaded = false;
        Debug.Log("Unity Setup Native Key");
        UnityNativeFullSetupNative(nativeKeys);
        Debug.Log("Unity Load Native Key");
        UnityNativeFullLoadNative(gameObject.name);
        //Hàm setup param khi tải quảng cáo, tự xử lý lấy giá trị từ firebase remote để truyền vào
        SetControlParram(false, false, false, "", "LARGE");
    }

    /// <summary>
    /// Hàm để truyền các biến trong Remote,gọi hàm này khi load được remote từ firebase
    /// </summary>
    /// <param name="v1"></param> bật tắt vùng 1
    /// <param name="v2"></param> bật tắt vùng 2
    /// <param name="v3"></param> bật tắt vùng 3
    /// <param name="ctaPos"></param> Vị trí show cta (LEFT, CENTER, RIGHT, FULL)Truyền vào rỗng
    /// ///  <param name="setCloseButtonSize"> đặt kích thước của nút đóng quảng cáo trong inter (TINY, SMALL, NORMAL, LARGE)
    public void SetControlParram(bool v1, bool v2, bool v3, string ctaPos, string setCloseButtonSize)
    {
        //Phần này là lấy các giá trị từ remote để truyền vào, các e tự setup remote để truyền tham số
        SetParamControl(v1, v2, v3);
        SetCTRSize(setCloseButtonSize);
    }

    Action<bool> callback;
    /// <summary>
	/// Hàm show quảng cáo native full giống bên android
	/// </summary>
    public void ShowNativeFull(Action<bool> callback)
    {
        this.callback = null;
        Debug.Log("Unity Load Native Key");
        if (isNativeLoaded)
        {
            this.callback = callback;
            isNativeLoaded = false;
            //objAds.SetActive(true);
            UnityNativeFullShowNative(gameObject.name);
        }
        else
        {
            callback?.Invoke(false);
        }

    }

    ///Hàm truyền kích thước nút ctr 
    ///  <param name="size"> đặt kích thước của nút đóng quảng cáo trong inter (TINY, SMALL, NORMAL, LARGE)
    public void SetCTRSize(string size)
    {
        UnityNativeFullSetSizeCTR(size);
    }
    public void SetParamControl(bool v1, bool v2, bool v3)
    {
        UnityNativeFullSetControlParam(v1, v2, v3);
    }
    /// <summary>
	/// Hàm kiểm tra native đã load được chưa
	/// </summary>
	/// <returns></returns>
    public bool IsNativeFullLoaded()
    {
        return isNativeLoaded;
    }


    #region CallBack
    public void onAdClicked()
    {

        Debug.Log("Ad Clicked");
    }

    public void onAdClosed()
    {
        Debug.Log("Ad Closed");
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            callback?.Invoke(true);
           // objAds.SetActive(false);
            LoadNativeAds();
        });
    }

    public void onLoadSuccess()
    {
        Debug.Log("Ad Native Full Loaded");
        isNativeLoaded = true;
        retryCount = 0;

    }
    public void onLoadFail()
    {
        Debug.Log("Ad Native Full Load Failed");

        if (retryCount < maxRetries)
        {
            retryCount += 1;
            Invoke(nameof(LoadNativeAds), retryDelay);
        }
    }
    public void onAdShowFailed()
    {
        Debug.Log("Ad Native Full Show Failed");


    }
    public void onAdLoading()
    {
        Debug.Log("Ad Native Full Loading");

    }
    public void OnAdPaidEvent(string message)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {

            string[] parts = message.Split('|');
            if (parts.Length == 2)
            {
                double adValue = double.Parse(parts[0]) / 1000000f;
                string currencyCode = parts[1];
                Debug.Log("Ad value:" + adValue + "___Currency Code:" + currencyCode);
                //Thêm code bắn revenue adjust,firebase tại đây
                AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(GameHelper.AdjustAdRevenueSourceAdMob);
                adjustAdRevenue.SetRevenue(adValue, currencyCode);
                // track Adjust ad revenue
                Adjust.TrackAdRevenue(adjustAdRevenue);
                //Tracking firebase
                Firebase.Analytics.Parameter[] AdParameters = {
                 new Firebase.Analytics.Parameter("ad_platform", "nativeAd"),
                  new Firebase.Analytics.Parameter("currency",currencyCode),
                new Firebase.Analytics.Parameter("value", adValue)
            };
                Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdParameters);
            }
        });

    }
    #endregion
#endif
}
