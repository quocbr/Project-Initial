using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using quocbr.DesignPattern;
using System;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_IOS
using UnityEngine.iOS;
#endif
public enum LocationRemoteInter
{
    None,
    Win,        // Win level
    Lose,       // Fail level → tap Try Again
    Replay,     // tap Replay
    QuitHome,   // tap Quit (thoát màn chơi ra Home)
    LoseHome    // Fail level → tap Home
}

public class CC_Interface : Singleton<CC_Interface>
{

    public bool HasBanner { get; set; }
    public bool IsShowingAd { get; set; }
    public bool IsLaunchAoa = false;
    protected override void Awake()
    {
        base.Awake();
        IsShowingAd = false;
    }
    private void Start()
    {
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;


    }
    #region AOA
    private void OnAppStateChanged(AppState state)
    {
        if (state == AppState.Foreground)
        {
            if (IsLaunchAoa && !ActionHelper.IsEditor())
            {
                if (RemoteConfig.Ins != null)
                    if (RemoteConfig.Ins.allConfigData.IsShowAoaResume)
                    {
                        ActionHelper.ShowAOA(() =>
                        {
                            CanvasAllScene.Ins.panelLoading.SetActive(false);
                        });
                    }
            }
        }
    }
    public void ShowAppOpenAd(UnityAction callback)
    {
        if (IsShowingAd)
        {
            callback?.Invoke();
            return;
        }

        if (AppOpenAdsManager.Ins.IsAdAvailable)
            AppOpenAdsManager.Ins.ShowAdIfAvailable(callback);
        else
        {
            AppOpenAdsManager.Ins.LoadAppOpenAd();
            callback?.Invoke();
        }

    }
    #endregion

    #region Reward
    public void ShowReward(string where = "", Action<bool> callback = null)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            callback?.Invoke(false);
            // OnShowMessage onShowMessage = new OnShowMessage();
            // onShowMessage.message = "No Connection";
            // EventManager.Trigger(onShowMessage);

            //GameEventBus.FireShowMessage("No Internet");
            //   CanvasAllScene.Ins.ShowNoti("No Internet");
        }
        else
        {
            CC_Max.Ins.ShowRewardedAd(callback);
        }


    }
    #endregion

    #region Inter


    public void ShowInter(Action<bool> callback, LocationRemoteInter locationRemoteInter)
    {
        var abTest = RemoteConfig.Ins.allConfigData.IndexAbTestNativeInter;
        //  if (KeepObject.Ins.LoadNativeFullLoaded())
        {
            if (abTest == 1)
            {
                ShowInterNative(callback, locationRemoteInter);
            }
            else if (abTest == 2)
            {
                ShowNativeNative(callback, locationRemoteInter);
            }
            else
            {
                ShowInterNormal(callback, locationRemoteInter);
            }

        }
        //else
        //{
        //    ShowInterNormal(callback, locationRemoteInter);
        //}


    }
    private void ShowInterNormal(Action<bool> callback, LocationRemoteInter locationRemoteInter)
    {
        var allConfig = RemoteConfig.Ins.allConfigData;
        var isShow = false;
        Debug.Log("ShowInterNormal");
        switch (locationRemoteInter)
        {
            case LocationRemoteInter.Win:
                if (allConfig.InterWin)
                    isShow = true;
                break;
            case LocationRemoteInter.Lose:
                if (allConfig.InterLose)
                    isShow = true;
                break;
            case LocationRemoteInter.LoseHome:
                if (allConfig.InterLoseHome)
                    isShow = true;
                break;
            case LocationRemoteInter.Replay:
                if (allConfig.InterReplay)
                    isShow = true;
                break;
            case LocationRemoteInter.QuitHome:
                if (allConfig.InterQuitHome)
                    isShow = true;
                break;
        }

        if (isShow)
            CC_Max.Ins.ShowInterstitial(callback, TypeAdsInter.Inter);
        else
            callback?.Invoke(true);
    }
    private void ShowInterNative(Action<bool> callback, LocationRemoteInter locationRemoteInter)
    {
        var allConfig = RemoteConfig.Ins.allConfigData;
        var isShow = false;

        switch (locationRemoteInter)
        {
            case LocationRemoteInter.Win:
                if (allConfig.IsNativeInterWin)
                    isShow = true;
                break;
            case LocationRemoteInter.Lose:
            case LocationRemoteInter.LoseHome:
                if (allConfig.IsNativeInterLose)
                    isShow = true;
                break;
            case LocationRemoteInter.Replay:
                if (allConfig.IsNativeInterReplay)
                    isShow = true;
                break;
            case LocationRemoteInter.QuitHome:
                if (allConfig.IsNativeInterQuitHome)
                    isShow = true;
                break;

        }

        if (isShow)
            CC_Max.Ins.ShowInterstitial(callback, TypeAdsInter.Inter_Native);
        else
            ShowInterNormal(callback, locationRemoteInter);
    }
    private void ShowNativeNative(Action<bool> callback, LocationRemoteInter locationRemoteInter)
    {
        var allConfig = RemoteConfig.Ins.allConfigData;
        var isShow = false;
        Debug.Log("ShowNativeNative");
        switch (locationRemoteInter)
        {
            case LocationRemoteInter.Win:
                if (allConfig.IsNativeInterWin)
                    isShow = true;
                break;
            case LocationRemoteInter.Lose:
            case LocationRemoteInter.LoseHome:
                if (allConfig.IsNativeInterLose)
                    isShow = true;
                break;
            case LocationRemoteInter.Replay:
                if (allConfig.IsNativeInterReplay)
                    isShow = true;
                break;
            case LocationRemoteInter.QuitHome:
                if (allConfig.IsNativeInterQuitHome)
                    isShow = true;
                break;
        }

        if (isShow)
            CC_Max.Ins.ShowInterstitial(callback, TypeAdsInter.Native_Native);
        else
            ShowInterNormal(callback, locationRemoteInter);
    }
    #endregion

    #region Banner

    public void ShowBanner()
    {
        //if (ActionHelper.IsNotVietnam())
        //    KeepObject.Ins.nativeBanner.ShowNativeBanner();
        //else
        CC_Max.Ins.ShowBanner();
    }
    public void HideBanner()
    {
        //if (ActionHelper.IsNotVietnam())
        //    KeepObject.Ins.nativeBanner.Hide();
        //else
        CC_Max.Ins.HideBanner();
    }
    public void DestroyBanner()
    {
        //if (ActionHelper.IsNotVietnam())
        //    KeepObject.Ins.nativeBanner.DestroyNativeBanner();
        //else
        CC_Max.Ins.DestroyBanner();
    }
    #endregion

    #region IAP
    private Action<bool> callback = null;

    // true khi IAPManager đang chờ kết quả một lần mua do người chơi bấm.
    public bool IsWaitingPurchase => callback != null;
    public void BuyIAP(IAP_Product iAP_Product, Action<bool> callback = null)
    {
        IsShowingAd = true;
        this.callback = callback;
        CC_IAP.Ins.BuyIAP(iAP_Product);
    }
    // Cộng quà (coin, booster, RemoveAds...) do IAPManager lo, dùng chung với luồng mua mới.
    public void RestorePurchase(IAP_Product iAP_Product)
    {
        IAPManager.Ins.RestorePurchase(iAP_Product);
        VariableSystem.IsRestoreDone = true;
    }

    // Kết quả mua từ CC_IAP. Cộng quà + popup do IAPManager xử lý qua callback.
    public void PurchaseCallback(bool isSuccess)
    {
        IsShowingAd = false;
        Debug.Log("Call back purchase " + isSuccess);

        // CanvasAllScene không nằm sẵn trong scene → panelLoading có thể null.
        if (CanvasAllScene.Ins.panelLoading != null)
            CanvasAllScene.Ins.panelLoading.SetActive(false);

        // if (!isSuccess)
        //     EventManager.Trigger(new OnShowMessage { message = "Purchase Failed" });

        Action<bool> cb = callback;
        callback = null;
        cb?.Invoke(isSuccess);
    }
    #endregion
}


