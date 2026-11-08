using System;
using System.Collections;
using System.Collections.Generic;
using AdjustSdk;
using quocbr.DesignPattern;
using UnityEngine;
using UnityEngine.Events;
using static MaxSdkBase;

//#if UNITY_IOS
//using Unity.Advertisement.IosSupport;
//using System.Runtime.InteropServices;
//namespace AudienceNetwork
//{
//    public static class AdSettings
//    {
//        [DllImport("__Internal")]
//        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

//        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
//        {
//            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
//        }
//    }
//}
//#endif
public class CC_Max : Singleton<CC_Max>
{
#if UNITY_ANDROID
    private string InterAdUnitId = "8f458c51c9c92a16";
    private string RewardAdUnitId = "3689786686c82c28";
    private string BannerAdUnitId = "677d6bec41de3964";
    private string AppOpenAdUnitId = "";
#elif UNITY_IOS
    private string InterAdUnitId = "";
    private string RewardAdUnitId = "";
    private string BannerAdUnitId = "";
    private string AppOpenAdUnitId = "";
#else
    private string InterAdUnitId = "";
    private string RewardAdUnitId = "";
    private string BannerAdUnitId = "";
    private string AppOpenAdUnitId = "";
#endif
    private int retryAttemptInter,
        retryAttemptReward;

    private float interLoadStartTime;
    private float interDisplayStartTime;
    private string interLoadPlacement = "auto_reload";

    private float rewardedLoadStartTime;
    private float rewardedDisplayStartTime;
    private string rewardedLoadPlacement = "auto_reload";

    private float bannerLoadStartTime;

    public bool isWatchedVideo = false;
    public bool isRewardAdsIsReady = false;
    public bool isInterAdsIsReady;

    private Action<bool> RewardedCompleteCallback;
    private Action<bool> CallbackInter;
    private UnityAction RewardFailedCallback;

    public bool isShowingNormalAd = false;
    public bool isShowingAOA = false;

    private bool isInit = false;

    private void Start()
    {
        InitAds();
    }

    public void InitAds()
    {
        if (isInit)
            return;
        isInit = true;
        Debug.Log("init = " + isInit);
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            InitializeInterstitialAds();
            InitializeRewardedAds();
            InitializeBannerAds();

            //  InitAOA();
            // Show Mediation Debugger
        };

        MaxSdk.InitializeSdk();
    }

    #region Interstitial Ads
    public void InitializeInterstitialAds()
    {
        if (VariableSystem.RemoveAds || VariableSystem.RemoveAdsHack)
            return;
        // Attach callback
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterAdRevenuePaidEvent;

        // Load the first interstitial
        interLoadPlacement = "open_game";
        LoadInterstitial();
    }

    private void LoadInterstitial()
    {
        if (VariableSystem.RemoveAds || VariableSystem.RemoveAdsHack)
            return;
        ActionHelper.Ad_Load(TypeAds.Inter);
        interLoadStartTime = Time.realtimeSinceStartup;
        MaxSdk.LoadInterstitial(InterAdUnitId);
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'
        // Reset retry attempt
        isInterAdsIsReady = MaxSdk.IsInterstitialReady(adUnitId);
        retryAttemptInter = 0;
        //  AdjustTracking.TrackingEvent(TypeAds.Inter, TypeTracking.Load);
        ActionHelper.Ad_LoadSuccess(TypeAds.Inter);
        // DataBucketEvent.Ins.AdRequest(
        //     "interstitial",
        //     "Applovin_Max",
        //     adInfo.NetworkName,
        //     interLoadPlacement,
        //     true,
        //     (Time.realtimeSinceStartup - interLoadStartTime) * 1000f
        // );
        interLoadPlacement = "auto_reload";
        Debug.Log("Loaded Inter");
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)
        retryAttemptInter++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttemptInter));
        // DataBucketEvent.Ins.AdRequest(
        //     "interstitial",
        //     "Applovin_Max",
        //     "",
        //     interLoadPlacement,
        //     false,
        //     (Time.realtimeSinceStartup - interLoadStartTime) * 1000f
        // );
        interLoadPlacement = "auto_reload";
        Invoke(nameof(LoadInterstitial), (float)retryDelay);
        ConfigShowBannerAfterInter();
    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        StopWaitAdCallback();
        isShowingNormalAd = true;
        CanvasAllScene.Ins.panelLoading.SetActive(false);
        Time.timeScale = 0;
        AdsConfig.Ins.OnInterShown();
        ActionHelper.Ad_Impression(TypeAds.Inter);
        interDisplayStartTime = Time.realtimeSinceStartup;
    }

    private void OnInterstitialAdFailedToDisplayEvent(
        string adUnitId,
        MaxSdkBase.ErrorInfo errorInfo,
        MaxSdkBase.AdInfo adInfo
    )
    {
        StopWaitAdCallback();
        // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
        Time.timeScale = 1;
        // DataBucketEvent.Ins.AdShow(
        //     "interstitial",
        //     "Applovin_Max",
        //     adInfo.NetworkName,
        //     false,
        //     0,
        //     "USD"
        // );
        LoadInterstitial();
        ConfigShowBannerAfterInter();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void ConfigShowBannerAfterInter()
    {
        bool isContinueShowAds = true;
        switch (typeAdsInter)
        {
            case TypeAdsInter.Inter:
                ActionHelper.ShowBanner();

                SetCallback(true);
                ActionHelper.Ad_Show(TypeAds.Inter);

                break;
            case TypeAdsInter.Inter_Native:
                if (ActionHelper.IsNotVietnam())
                {
                    CanvasAllScene.Ins.imgFullOverlay.gameObject.SetActive(true);
                    isContinueShowAds = false;
                    KeepObject.Instance.ShowNativeFull(
                        (bool isDone) =>
                        {
                            CanvasAllScene.Ins.imgFullOverlay.gameObject.SetActive(false);
                            Debug.Log("False Full Overlay");
                            ActionHelper.ShowBanner();
                            ChangeIsShowingAds();
                            SetCallback(isDone);
                        }
                    );
                }
                else
                {
                    CanvasAllScene.Ins.imgFullOverlay.gameObject.SetActive(false);
                    ActionHelper.ShowBanner();
                    SetCallback(true);
                }

                break;
            case TypeAdsInter.Native_Native:
                ActionHelper.ShowBanner();
                SetCallback(true);

                break;
        }

        if (isContinueShowAds)
            ChangeIsShowingAds();
    }

    private void SetCallback(bool isDone)
    {
        CanvasAllScene.Ins.imgFullOverlay.gameObject.SetActive(false);
        CanvasAllScene.Ins.panelLoading.SetActive(false);
        CallbackInter?.Invoke(isDone);
        CallbackInter = null;
    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        StopWaitAdCallback();
        VariableSystem.CountWatchedReward++;
        Time.timeScale = 1;

        // DataBucketEvent.Ins.AdComplete(
        //     "interstitial",
        //     "Applovin_Max",
        //     adInfo.NetworkName,
        //     KeepObject.Ins.playerPosition,
        //     (Time.realtimeSinceStartup - interDisplayStartTime) * 1000f,
        //     "null"
        // );

        LoadInterstitial();
        ConfigShowBannerAfterInter();
        //   AdjustTracking.TrackingEvent(TypeAds.Inter, TypeTracking.Close);
    }

    private TypeAdsInter typeAdsInter;

    public void ShowInterstitial(Action<bool> callback, TypeAdsInter typeAdsInter)
    {
        this.CallbackInter = callback;
        this.typeAdsInter = typeAdsInter;
        Debug.Log("type ads inter = " + typeAdsInter);
        ActionHelper.HideBanner();

        if (typeAdsInter == TypeAdsInter.Native_Native && ActionHelper.IsNotVietnam())
        {
            KeepObject.Instance.ShowNativeFull(
                (bool isDone) =>
                {
                    CanvasAllScene.Ins.imgFullOverlay.gameObject.SetActive(true);
                    KeepObject.Instance.ShowNativeFull(
                        (bool isDone) =>
                        {
                            CanvasAllScene.Ins.imgFullOverlay.gameObject.SetActive(false);
                            ActionHelper.ShowBanner();
                            SetCallback(isDone);
                        }
                    );
                }
            );
        }
        else
        {
            CanvasAllScene.Ins.panelLoading.SetActive(true);
            if (MaxSdk.IsInterstitialReady(InterAdUnitId))
            {
                CanvasAllScene.Ins.panelLoading.SetActive(false);
                CanvasAllScene.Ins.imgFullOverlay.gameObject.SetActive(true);
                //    AdjustTracking.TrackingEvent(TypeAds.Inter, TypeTracking.Show);
                MaxSdk.ShowInterstitial(InterAdUnitId);

                Debug.Log("---------SHOW INTER ADS ------------");
                StartWaitAdCallback();
            }
            else
            {
                this.typeAdsInter = TypeAdsInter.None;
                CanvasAllScene.Ins.panelLoading.SetActive(false);
                ActionHelper.ShowBanner();
                //DataBucketEvent.Ins.AdShow("interstitial", "Applovin_Max", "", false, 0, "USD");
                callback?.Invoke(false);
                Debug.Log("Inter MAX not ready || Show Admob Inter Cover");
            }
        }
    }

    private Coroutine waitAdCallbackCoroutine;
    private const float AdTimeout = 4f;

    private void StopWaitAdCallback()
    {
        CanvasAllScene.Ins.panelLoading.SetActive(false);
        if (waitAdCallbackCoroutine != null)
        {
            StopCoroutine(waitAdCallbackCoroutine);
            waitAdCallbackCoroutine = null;
        }
    }

    private void StartWaitAdCallback()
    {
        CanvasAllScene.Ins.panelLoading.SetActive(true);
        if (waitAdCallbackCoroutine != null)
            StopCoroutine(waitAdCallbackCoroutine);

        waitAdCallbackCoroutine = StartCoroutine(IE_WaitAdCallback());
    }

    private IEnumerator IE_WaitAdCallback()
    {
        yield return new WaitForSecondsRealtime(AdTimeout);

        Time.timeScale = 1;
        ConfigShowBannerAfterInter(); // đảm bảo callback gọi
        waitAdCallbackCoroutine = null;
    }

    #endregion

    #region Rewarded ads


    public void InitializeRewardedAds()
    {
        // Attach callback
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardAdRevenuePaidEvent;

        // Load the first rewarded ad
        rewardedLoadPlacement = "open_game";
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        ActionHelper.Ad_Load(TypeAds.Reward);
        rewardedLoadStartTime = Time.realtimeSinceStartup;
        MaxSdk.LoadRewardedAd(RewardAdUnitId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.

        // Reset retry attempt
        isRewardAdsIsReady = MaxSdk.IsRewardedAdReady(adUnitId);
        retryAttemptReward = 0;
        // AdjustTracking.TrackingEvent(TypeAds.Reward, TypeTracking.Load);
        ActionHelper.Ad_LoadSuccess(TypeAds.Reward);
        // DataBucketEvent.Ins.AdRequest(
        //     "video_rewarded",
        //     "Applovin_Max",
        //     adInfo.NetworkName,
        //     rewardedLoadPlacement,
        //     true,
        //     (Time.realtimeSinceStartup - rewardedLoadStartTime) * 1000f
        // );
        rewardedLoadPlacement = "auto_reload";
    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
        retryAttemptReward++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttemptReward));

        // DataBucketEvent.Ins.AdRequest(
        //     "video_rewarded",
        //     "Applovin_Max",
        //     "",
        //     rewardedLoadPlacement,
        //     false,
        //     (Time.realtimeSinceStartup - rewardedLoadStartTime) * 1000f
        // );
        rewardedLoadPlacement = "auto_reload";

        Invoke(nameof(LoadRewardedAd), (float)retryDelay);
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isShowingNormalAd = true;
        ActionHelper.Ad_Impression(TypeAds.Reward);
        Time.timeScale = 0;
        rewardedDisplayStartTime = Time.realtimeSinceStartup;
    }

    private void OnRewardedAdFailedToDisplayEvent(
        string adUnitId,
        MaxSdkBase.ErrorInfo errorInfo,
        MaxSdkBase.AdInfo adInfo
    )
    {
        // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
        Time.timeScale = 1;
        // DataBucketEvent.Ins.AdShow(
        //     "video_rewarded",
        //     "Applovin_Max",
        //     adInfo.NetworkName,
        //     false,
        //     0,
        //     "USD"
        // );
        LoadRewardedAd();
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Time.timeScale = 1;
        // Rewarded ad is hidden. Pre-load the next ad
        //    AdjustTracking.TrackingEvent(TypeAds.Reward, TypeTracking.Close);
        if (isWatchedVideo)
            VariableSystem.CountWatchedReward++;

        // DataBucketEvent.Ins.AdComplete(
        //     "video_rewarded",
        //     "Applovin_Max",
        //     adInfo.NetworkName,
        //     KeepObject.Ins.playerPosition,
        //     (Time.realtimeSinceStartup - rewardedDisplayStartTime) * 1000f,
        //     isWatchedVideo ? "done" : "quit"
        // );

        RewardedCompleteCallback?.Invoke(isWatchedVideo);
        CanvasAllScene.Ins.panelLoading.SetActive(false);
        LoadRewardedAd();
        ChangeIsShowingAds();
    }

    private void OnRewardedAdReceivedRewardEvent(
        string adUnitId,
        MaxSdk.Reward reward,
        MaxSdkBase.AdInfo adInfo
    )
    {
        // The rewarded ad displayed and the user should receive the reward.
        isWatchedVideo = true;
        ActionHelper.Ad_Show(TypeAds.Reward);
    }

    public void ShowRewardedAd(Action<bool> callback)
    {
        if (MaxSdk.IsRewardedAdReady(RewardAdUnitId))
        {
            ShowRewardAds(callback, true);
        }
        else
        {
            CanvasAllScene.Ins.panelLoading.SetActive(true);
            StartCoroutine(WaitForRewardVideo(callback));
        }
    }

    IEnumerator WaitForRewardVideo(Action<bool> callback)
    {
        LoadRewardedAd();

        yield return new WaitForSecondsRealtime(3f);

        if (MaxSdk.IsRewardedAdReady(RewardAdUnitId))
        {
            ShowRewardAds(callback, true);
        }
        else
        {
            ShowRewardAds(callback, false);
        }
    }

    private void ShowRewardAds(Action<bool> callback, bool isShow)
    {
        CanvasAllScene.Ins.panelLoading.SetActive(false);
        isShowingNormalAd = isShow;
        if (isShow)
        {
            // AdjustTracking.TrackingEvent(TypeAds.Reward, TypeTracking.Show);
            MaxSdk.ShowRewardedAd(RewardAdUnitId);
            isWatchedVideo = false;
            if (callback != null)
                RewardedCompleteCallback = callback;
        }
        else
        {
            CanvasAllScene.Ins.panelLoading.SetActive(false);
            //GameEventBus.FireShowMessage("Rewarded Ads not available");
            // CanvasAllScene.Ins.ShowNoti("Ads not available");
            //DataBucketEvent.Ins.AdShow("video_rewarded", "Applovin_Max", "", false, 0, "USD");
            callback?.Invoke(false);
        }
    }

    #endregion

    #region Banner Ads

    public void InitializeBannerAds()
    {
        if (VariableSystem.RemoveAds || VariableSystem.RemoveAdsHack)
            return;

        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;

        var adViewConfiguration = new MaxSdk.AdViewConfiguration(
            MaxSdk.AdViewPosition.BottomCenter
        );
        MaxSdk.CreateBanner(BannerAdUnitId, adViewConfiguration);
    }

    public void LoadBanner()
    {
        if (VariableSystem.RemoveAds || VariableSystem.RemoveAdsHack)
            return;
        Debug.Log("Load Banner");
        ActionHelper.Ad_Load(TypeAds.Banner);
        bannerLoadStartTime = Time.realtimeSinceStartup;
        MaxSdk.LoadBanner(BannerAdUnitId);
    }

    public void ShowBanner()
    {
        if (VariableSystem.RemoveAds || VariableSystem.RemoveAdsHack)
            return;
        Debug.Log("Show Banner");
        ActionHelper.Ad_Show(TypeAds.Banner);
        ActionHelper.Ad_Impression(TypeAds.Banner);
        if (!isLoadedBanner)
            LoadBanner();
        MaxSdk.ShowBanner(BannerAdUnitId);
    }

    public void DestroyBanner()
    {
        MaxSdk.DestroyBanner(BannerAdUnitId);
    }

    public void HideBanner()
    {
        MaxSdk.HideBanner(BannerAdUnitId);
    }

    private bool isLoadedBanner = false;

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isLoadedBanner = true;
        // Banner ad is ready to be shown.
        // If you have already called MaxSdk.ShowBanner(BannerAdUnitId) it will automatically be shown on the next ad refresh.
        // AdjustTracking.TrackingEvent(TypeAds.Banner, TypeTracking.Load);
        //     AdjustTracking.TrackingEvent(TypeAds.Banner, TypeTracking.Show);
        Debug.Log("Banner loaded");
        ActionHelper.Ad_LoadSuccess(TypeAds.Banner);
        //DataBucketEvent.Ins.AdRequest("banner", "Applovin_Max", adInfo.NetworkName, "auto_reload", true, (Time.realtimeSinceStartup - bannerLoadStartTime) * 1000f);
    }

    private void OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Banner ad failed to load. MAX will automatically try loading a new ad internally.
        //DataBucketEvent.Ins.AdRequest("banner", "Applovin_Max", "", "auto_reload", false, (Time.realtimeSinceStartup - bannerLoadStartTime) * 1000f);
    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    #endregion

    private void ChangeIsShowingAds()
    {
        isShowingNormalAd = false;
    }

    #region AOA
    void InitAOA()
    {
        MaxSdk.LoadAppOpenAd(AppOpenAdUnitId);
        MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
        MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += HandleAdDidPresentFullScreenContent;
        //  MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAoaAdRevenuePaidEvent;
        MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAppOpenAdDisplayFailedEvent;
    }

    public void ShowAdIfReady()
    {
        if (isShowingAOA || isShowingNormalAd || AppOpenAdsManager.Ins.isShowingAd)
        {
            Debug.Log(
                "MAX AOA is not available - reason "
                    + "-isshowingad MAX "
                    + isShowingAOA
                    + "-showingnormal "
                    + isShowingNormalAd
                    + "-isShowingAd ADMOB"
                    + AppOpenAdsManager.Ins.isShowingAd
            );
            return;
        }
        if (MaxSdk.IsAppOpenAdReady(AppOpenAdUnitId))
        {
            CanvasAllScene.Ins.panelLoading.SetActive(true);
            MaxSdk.ShowAppOpenAd(AppOpenAdUnitId);
        }
        else
        {
            Debug.Log("MAX AOA MISS => SHOW ADMOB AOA");
            ActionHelper.ShowAOA(() =>
            {
                CanvasAllScene.Ins.panelLoading.SetActive(false);
            });
            MaxSdk.LoadAppOpenAd(AppOpenAdUnitId);
        }
    }

    public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Time.timeScale = 1;

        CanvasAllScene.Ins.panelLoading.SetActive(false);
        StartCoroutine(delayIsShowingAOAAdFalse());
        MaxSdk.LoadAppOpenAd(AppOpenAdUnitId);
    }

    void HandleAdDidPresentFullScreenContent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Displayed MAX app open ad");
        isShowingAOA = true;
        Time.timeScale = 0;
    }

    void OnAppOpenAdDisplayFailedEvent(
        string adUnitId,
        MaxSdkBase.ErrorInfo errorInfo,
        MaxSdkBase.AdInfo adInfo
    )
    {
        Debug.Log("Display Failed AOA : reason : " + errorInfo.Message);
        Time.timeScale = 1;
        MaxSdk.LoadAppOpenAd(AppOpenAdUnitId);
        StartCoroutine(delayIsShowingAOAAdFalse());
    }

    IEnumerator delayIsShowingAOAAdFalse()
    {
        yield return new WaitForSeconds(8f);
        isShowingAOA = false;
    }

    #endregion
    private void TrackingFirebase(
        string adUnitId,
        MaxSdkBase.AdInfo impressionData,
        TypeAds typeAds
    )
    {
        double revenue = impressionData.Revenue;
        var impressionParameters = new[]
        {
            new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
            new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
            new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
            new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
            new Firebase.Analytics.Parameter("value", revenue),
            new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
        };
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "ad_impression_mediation",
            impressionParameters
        );

        //  AdjustTracking.TrackingEvent(TypeAds.Impression, TypeTracking.Rev, revenue);
        AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(
            GameHelper.AdjustAdRevenueSourceAppLovinMAX
        );

        adjustAdRevenue.SetRevenue(revenue, "USD");
        adjustAdRevenue.AdRevenueNetwork = impressionData.NetworkName;
        adjustAdRevenue.AdRevenueUnit = impressionData.AdUnitIdentifier;
        adjustAdRevenue.AdRevenuePlacement = impressionData.Placement;
        Adjust.TrackAdRevenue(adjustAdRevenue);
    }

    private void OnInterAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
    {
        var rev = impressionData.Revenue;
        TrackingFirebase(adUnitId, impressionData, TypeAds.Inter);
        //  AdjustTracking.TrackingEvent(TypeAds.Inter, TypeTracking.Rev, rev);
        // DataBucketEvent.Ins.AdShow(
        //     "interstitial",
        //     "Applovin_Max",
        //     impressionData.NetworkName,
        //     true,
        //     rev,
        //     "USD"
        // );
    }

    private void OnRewardAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
    {
        var rev = impressionData.Revenue;
        TrackingFirebase(adUnitId, impressionData, TypeAds.Reward);
        // AdjustTracking.TrackingEvent(TypeAds.Reward, TypeTracking.Rev, rev);
        // DataBucketEvent.Ins.AdShow(
        //     "video_rewarded",
        //     "Applovin_Max",
        //     impressionData.NetworkName,
        //     true,
        //     rev,
        //     "USD"
        // );
    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
    {
        var rev = impressionData.Revenue;
        TrackingFirebase(adUnitId, impressionData, TypeAds.Banner);
        // AdjustTracking.TrackingEvent(TypeAds.Banner, TypeTracking.Rev, rev);
        // AdjustTracking.TrackingEvent(TypeAds.Banner, TypeTracking.Close);
        //DataBucketEvent.Ins.AdShow("banner", "Applovin_Max", impressionData.NetworkName, true, rev, "USD");
    }
}
