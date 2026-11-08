using System;
using System.Collections.Generic;
using System.Linq;
using GoogleMobileAds.Ump.Api;
using PimDeWitte.UnityMainThreadDispatcher;
using quocbr.DesignPattern;
using UnityEngine;
using UnityEngine.Events;

public class CC_Ads : Singleton<CC_Ads>
{
    public void InitAds()
    {
        CC_Max.Ins.InitAds();
    }

    #region CMP

    public void Start()
    {
        StartCMP();
    }

    void StartCMP()
    {
        if (IsCMPConsent())
        {
            MaxSdk.SetHasUserConsent(true);
            VariableSystem.IsHasUserConsent = true;
            InitAds();
        }
        else
        {
            MaxSdk.SetHasUserConsent(false);
            VariableSystem.IsHasUserConsent = false;
        }
    }

    private void ContinueLoading()
    {
        CanvasAllScene.Ins.panelLoading.SetActive(false);
        callbackHideCMP?.Invoke();
    }

    UnityAction callbackHideCMP = null;

    public void LevelShowCMP(UnityAction callback = null)
    {
        Debug.Log("Level Show CMP");
        if (!IsCMPConsent())
        {
            callbackHideCMP = callback;
            ResetCMP();
        }
        else
        {
            CanvasAllScene.Ins.panelLoading.SetActive(false);
            callback?.Invoke();
        }
    }

    public static int GetInt(string key, int defaultValue)
    {
        return GetValue<int>(key, defaultValue, "getInt");
    }

    public static float GetFloat(string key, float defaultValue)
    {
        return GetValue<float>(key, defaultValue, "getFloat");
    }

    public static string GetString(string key, string defaultValue)
    {
        return GetValue<string>(key, defaultValue, "getString");
    }

    private static T GetValue<T>(string key, T defaultValue, string methodName)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    string packageName = activity.Call<string>("getPackageName");
                    using (
                        var prefs = activity.Call<AndroidJavaObject>(
                            "getSharedPreferences",
                            packageName + "_preferences",
                            0
                        )
                    )
                    {
                        return prefs.Call<T>(methodName, key, defaultValue);
                    }
                }
            }
        }

        return defaultValue;
    }

    // Number:
    // 1 - GDPR applies in current context
    // 0 - GDPR does not apply in current context
    // Unset - undetermined (default before initialization)
    public static int GetGDPRApplicability()
    {
        return GetInt("IABTCF_gdprApplies", -1); // Default to -1 (undetermined) if not set
    }

    /// <summary>
    /// Check if user accepted consent
    /// </summary>
    bool IsCMPConsent()
    {
        //Debug.LogError("isCMPConsent CanShowAds: " + CanShowAds() + " ----- CanShowPersonalizedAds: " + CanShowPersonalizedAds());
        if (CanShowAds() || CanShowPersonalizedAds())
        {
            string CMPString = GetString("IABTCF_AddtlConsent", "NO");
#if UNITY_IPHONE
            CMPString = PlayerPrefs.GetString("IABTCF_AddtlConsent", "NO");
#endif
            Debug.Log("CMPString = " + CMPString);
            if (CMPString != null)
            {
                if (CMPString.Contains("2878"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private const string PURPOSE_CONSENT_KEY = "IABTCF_PurposeConsents";
    private const string VENDOR_CONSENT_KEY = "IABTCF_VendorConsents";
    private const string VENDOR_LI_KEY = "IABTCF_VendorLegitimateInterests";
    private const string PURPOSE_LI_KEY = "IABTCF_PurposeLegitimateInterests";

    /// <summary>
    ///Get ID consent on Android device
    /// </summary>
    //    public string GetString(string stringGet, string defaultValue = "")
    //    {
    //#if UNITY_EDITOR
    //        return defaultValue;

    //#else
    //        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    //        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    //        AndroidJavaClass preferenceManagerClass = new AndroidJavaClass("android.preference.PreferenceManager");
    //        AndroidJavaObject sharedPreferences = preferenceManagerClass.CallStatic<AndroidJavaObject>("getDefaultSharedPreferences", currentActivity);
    //        return sharedPreferences.Call<string>("getString", stringGet, defaultValue);
    //#endif
    //    }
    // Check if ads can be shown
    bool CanShowAds()
    {
        string purposeConsent = "";
        string vendorConsent = "";
        string vendorLI = "";
        string purposeLI = "";
#if UNITY_IPHONE
        purposeConsent = PlayerPrefs.GetString(PURPOSE_CONSENT_KEY, "");
        vendorConsent = PlayerPrefs.GetString(VENDOR_CONSENT_KEY, "");
        vendorLI = PlayerPrefs.GetString(VENDOR_LI_KEY, "");
        purposeLI = PlayerPrefs.GetString(PURPOSE_LI_KEY, "");
#elif UNITY_ANDROID
        purposeConsent = GetString(PURPOSE_CONSENT_KEY, "");
        vendorConsent = GetString(VENDOR_CONSENT_KEY, "");
        vendorLI = GetString(VENDOR_LI_KEY, "");
        purposeLI = GetString(PURPOSE_LI_KEY, "");
#endif

        int googleId = 755;
        bool hasGoogleVendorConsent = HasAttribute(vendorConsent, googleId);
        bool hasGoogleVendorLI = HasAttribute(vendorLI, googleId);

        // Minimum required for at least non-personalized ads
        return HasConsentFor(new List<int> { 1 }, purposeConsent, hasGoogleVendorConsent)
            && HasConsentOrLegitimateInterestFor(
                new List<int> { 2, 7, 9, 10 },
                purposeConsent,
                purposeLI,
                hasGoogleVendorConsent,
                hasGoogleVendorLI
            );
    }

    // Check if personalized ads can be shown
    bool CanShowPersonalizedAds()
    {
        string purposeConsent = "";
        string vendorConsent = "";
        string vendorLI = "";
        string purposeLI = "";

#if UNITY_IPHONE
        purposeConsent = PlayerPrefs.GetString(PURPOSE_CONSENT_KEY, "");
        vendorConsent = PlayerPrefs.GetString(VENDOR_CONSENT_KEY, "");
        vendorLI = PlayerPrefs.GetString(VENDOR_LI_KEY, "");
        purposeLI = PlayerPrefs.GetString(PURPOSE_LI_KEY, "");
#elif UNITY_ANDROID
        purposeConsent = GetString(PURPOSE_CONSENT_KEY, "");
        vendorConsent = GetString(VENDOR_CONSENT_KEY, "");
        vendorLI = GetString(VENDOR_LI_KEY, "");
        purposeLI = GetString(PURPOSE_LI_KEY, "");
#endif
        int googleId = 755;
        bool hasGoogleVendorConsent = HasAttribute(vendorConsent, googleId);
        bool hasGoogleVendorLI = HasAttribute(vendorLI, googleId);

        return HasConsentFor(new List<int> { 1, 3, 4 }, purposeConsent, hasGoogleVendorConsent)
            && HasConsentOrLegitimateInterestFor(
                new List<int> { 2, 7, 9, 10 },
                purposeConsent,
                purposeLI,
                hasGoogleVendorConsent,
                hasGoogleVendorLI
            );
    }

    // Check if a binary string has a "1" at position "indexLevel" (1-based)
    bool HasAttribute(string input, int index)
    {
        return input.Length >= index && input[index - 1] == '1';
    }

    // Check if consent is given for a list of purposes
    bool HasConsentFor(List<int> purposes, string purposeConsent, bool hasVendorConsent)
    {
        return purposes.All(p => HasAttribute(purposeConsent, p)) && hasVendorConsent;
    }

    // Check if a vendor either has consent or legitimate interest for a list of purposes
    bool HasConsentOrLegitimateInterestFor(
        List<int> purposes,
        string purposeConsent,
        string purposeLI,
        bool hasVendorConsent,
        bool hasVendorLI
    )
    {
        return purposes.All(p =>
            (HasAttribute(purposeLI, p) && hasVendorLI)
            || (HasAttribute(purposeConsent, p) && hasVendorConsent)
        );
    }

    public void CheckConsentReset()
    {
        Debug.Log("Check User EU:" + GetGDPRApplicability());
        if (GetGDPRApplicability() == 1)
        {
            if (!IsCMPConsent())
            {
                if (Time.time - timeCheckCMPReset > 30)
                {
                    timeCheckCMPReset = Time.time;
                    ResetCMP();
                }
            }
        }
    }

    float timeCheckCMPReset;

    public void ResetCMP()
    {
        Time.timeScale = 0;
        ConsentInformation.Reset();
        ConsentRequestParameters request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false,
        };
        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    // UMP (User Messaging Platform) gọi lại ConsentInformation.Update / LoadAndShowConsentFormIfRequired
    // từ một thread nền của SDK native, không phải main thread của Unity. Mọi API Unity đụng tới bên
    // trong (CanvasAllScene.Ins -> FindFirstObjectByType, Time.timeScale...) sẽ throw UnityException
    // nếu gọi thẳng ở đây, nên phải đẩy toàn bộ xử lý qua UnityMainThreadDispatcher trước.
    void OnConsentInfoUpdated(FormError consentError)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => HandleConsentInfoUpdated(consentError));
    }

    private void HandleConsentInfoUpdated(FormError consentError)
    {
        if (consentError != null)
        {
            ContinueLoading();
            // Handle the error.
            Time.timeScale = 1;
            UnityEngine.Debug.LogError(consentError);
            return;
        }

        if (ConsentInformation.IsConsentFormAvailable())
        {
            CanvasAllScene.Ins.panelLoading.SetActive(true);
        }
        // If the error is null, the consent information state was updated.
        // You are now ready to check if a form is available.
        ConsentForm.LoadAndShowConsentFormIfRequired(
            (FormError formError) =>
                UnityMainThreadDispatcher
                    .Instance()
                    .Enqueue(() => HandleConsentFormResult(formError))
        );
    }

    private void HandleConsentFormResult(FormError formError)
    {
        if (formError != null)
        {
            // Consent gathering failed.
            ContinueLoading();
            Time.timeScale = 1;
            UnityEngine.Debug.LogError(formError);
            return;
        }

        ContinueLoading();
        Time.timeScale = 1;
        //Request an ad.
        if (IsCMPConsent())
        {
            MaxSdk.SetHasUserConsent(true);
            VariableSystem.IsHasUserConsent = true;
            InitAds();
        }
        else
        {
            MaxSdk.SetHasUserConsent(false);
            VariableSystem.IsHasUserConsent = false;
        }
    }

    #endregion
}
