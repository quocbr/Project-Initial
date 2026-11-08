using System;
using System.Collections;
using AdjustSdk;
using GoogleMobileAds.Api;
using quocbr.DesignPattern;
using UnityEngine;
using UnityEngine.Events;

public class AppOpenAdsManager : Singleton<AppOpenAdsManager>
{
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-4188057984229952/6117543753";
#elif UNITY_IOS
    private string _adUnitId = "ca-app-pub-4188057984229952/4276879283";
#else
    private string _adUnitId = "unused";
#endif

    private readonly TimeSpan APPOPEN_TIMEOUT = TimeSpan.FromHours(4);
    private DateTime appOpenExpireTime;
    private AppOpenAd appOpenAd;
    private int retryAttemp_AOA = 0;
    private UnityAction callback = null;

    private InterstitialAd interstitial;
    public bool isShowingAd = false;

    private float aoaLoadStartTime;
    private string aoaLoadPlacement = "auto_reload";
    private float aoaDisplayStartTime;

    public void InitAdmob()
    {
        if (KeepObject.Instance.mode != TypeMode.Release)
        {
            if (ActionHelper.IsAndroid())
            {
                _adUnitId = "ca-app-pub-3940256099942544/9257395921";
            }
            if (ActionHelper.IsIOS())
            {
                _adUnitId = "ca-app-pub-3940256099942544/5575463023";
            }
        }

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(
            (GoogleMobileAds.Api.InitializationStatus initStatus) =>
            {
                aoaLoadPlacement = "open_game";
                //LoadAppOpenAd();
            }
        );
    }

    #region AOA

    public bool IsAdAvailable
    {
        get
        {
            return appOpenAd != null && appOpenAd.CanShowAd() && DateTime.Now < appOpenExpireTime;
        }
    }

    /// <summary>
    /// Loads the app open ad.
    /// </summary>
    public void LoadAppOpenAd()
    {
        // Clean up the old ad before loading a new one.
        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        ActionHelper.Ad_Load(TypeAds.Aoa);
        aoaLoadStartTime = Time.realtimeSinceStartup;
        // send the request to load the ad.
        AppOpenAd.Load(
            _adUnitId,
            adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    //DataBucketEvent.Ins.AdRequest("aoa", "Admob", "Admob", aoaLoadPlacement, false, (Time.realtimeSinceStartup - aoaLoadStartTime) * 1000f);
                    aoaLoadPlacement = "auto_reload";
                    Invoke(
                        nameof(LoadAppOpenAd),
                        ActionHelper.GetTimeDelayReloadAds(ref retryAttemp_AOA)
                    );
                    return;
                }

                retryAttemp_AOA = 0;
                appOpenExpireTime = DateTime.Now + APPOPEN_TIMEOUT;
                appOpenAd = ad;
                ActionHelper.Ad_LoadSuccess(TypeAds.Aoa);
                //DataBucketEvent.Ins.AdRequest("aoa", "Admob", "Admob", aoaLoadPlacement, true, (Time.realtimeSinceStartup - aoaLoadStartTime) * 1000f);
                aoaLoadPlacement = "auto_reload";
                RegisterEventHandlers(ad);
            }
        );
    }

    public void ShowAdIfAvailable(UnityAction callback)
    {
        if (
            !IsAdAvailable
            || isShowingAd
            || CC_Max.Ins.isShowingNormalAd
            || CC_Max.Ins.isShowingAOA
        )
        {
            callback?.Invoke();
            return;
        }
        CanvasAllScene.Ins.panelLoading.SetActive(true);
        StartCoroutine(IE_WaitShowAOA(callback));
    }

    public IEnumerator IE_WaitShowAOA(UnityAction callback)
    {
        yield return new WaitForEndOfFrame();
        if (!CC_Interface.Ins.IsShowingAd)
        {
            this.callback = callback;
            CC_Interface.Ins.IsShowingAd = true;
            //   AdjustTracking.TrackingEvent(TypeAds.Aoa, TypeTracking.Show);
            ActionHelper.Ad_Impression(TypeAds.Aoa);
            ActionHelper.HideBanner();
            appOpenAd?.Show();
        }
        else
            callback?.Invoke();
    }

    private void RegisterEventHandlers(AppOpenAd ad)
    {
        //    AdjustTracking.TrackingEvent(TypeAds.Aoa, TypeTracking.Load);
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(
                String.Format("App open ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode)
            );
            TrackAdRevenue(adValue);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            ActionHelper.Ad_Show(TypeAds.Aoa);
            CanvasAllScene.Ins.panelLoading.SetActive(true);
            aoaDisplayStartTime = Time.realtimeSinceStartup;
            Debug.Log("App open ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            CanvasAllScene.Ins.panelLoading.SetActive(false);
            // AdjustTracking.TrackingEvent(TypeAds.Aoa, TypeTracking.Close);
            CC_Interface.Ins.IsShowingAd = false;
            // DataBucketEvent.Ins.AdComplete(
            //     "aoa",
            //     "Admob",
            //     "Admob",
            //     KeepObject.Ins.playerPosition,
            //     (Time.realtimeSinceStartup - aoaDisplayStartTime) * 1000f
            // );
            callback?.Invoke();
            LoadAppOpenAd();
            //if (ActionHelper.typeSceneCurrent == TypeSceneCurrent.GameplayScene)
            //{
            //    if (GameplayUIManager.Ins.panelShop.IsShow)
            //    {
            //        ActionHelper.DestroyBanner();
            //        ActionHelper.LoadBanner();
            //    }
            //}
            //StartCoroutine(ActionHelper.StartAction(() =>
            //{
            //    ActionHelper.ShowBanner();
            //}, 1));
            Debug.Log("App open ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            CC_Interface.Ins.IsShowingAd = false;
            //DataBucketEvent.Ins.AdShow("aoa", "Admob", "Admob", false, 0, "USD");
            callback?.Invoke();
            LoadAppOpenAd();
            Debug.LogError(
                "App open ad failed to open full screen content " + "with error : " + error
            );
        };
    }
    #endregion

    public void TrackAdRevenue(AdValue adInfo)
    {
        Debug.Log("TrackAdRevenue");
        var rev = ((double)adInfo.Value / 1000000f);

        //    AdjustTracking.TrackingEvent(TypeAds.Impression, TypeTracking.Rev, rev);
        //  AdjustTracking.TrackingEvent(TypeAds.Aoa, TypeTracking.Rev, rev);

        AdjustAdRevenue adRevenue = new AdjustAdRevenue(GameHelper.AdjustAdRevenueSourceAdMob);
        adRevenue.SetRevenue(rev, "USD");
        Debug.Log("value revenue = " + adInfo.Value);
        Adjust.TrackAdRevenue(adRevenue);
        OnAdRevenuePaidEvent(adInfo);
    }

    private void OnAdRevenuePaidEvent(AdValue impressionData)
    {
        Debug.Log("OnAdRevenuePaidEvent");
        var revenue = ((double)impressionData.Value / 1000000f);
        var impressionParameters = new[]
        {
            new Firebase.Analytics.Parameter("ad_platform", "GoogleAdMob"),
            new Firebase.Analytics.Parameter("ad_source", "Admob OpenAds"),
            new Firebase.Analytics.Parameter("ad_unit_name", ""),
            new Firebase.Analytics.Parameter("ad_format", ""),
            new Firebase.Analytics.Parameter("value", revenue),
            new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
        };
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
        //DataBucketEvent.Ins.AdShow("aoa", "Admob", "Admob", true, revenue, "USD");
    }
}
