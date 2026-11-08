using System;
using System.Collections;
using System.Collections.Generic;
using AdjustSdk;
using UnityEngine;

public enum TypeMode
{
    Release,
    Development,
    Marketing,
}

public class KeepObject : MonoBehaviour
{
    public static KeepObject Instance;
    private static bool isReady = false;
    public TypeMode mode;
    public Adjust adjust;
    public GameObject objDebug;
    public GameObject FPSDebug;
    public GameObject CheatObj;

    //public string playerPosition => UIManager.Ins.GetTrackPosition();

    public List<NativeInterstitial> listNativeInter = new List<NativeInterstitial>();

    //public BannerMrec bannerMrec;
    private void OnValidate()
    {
        if (mode == TypeMode.Release)
        {
            if (adjust != null)
                adjust.environment = AdjustEnvironment.Production;
            if (CheatObj != null)
                CheatObj.SetActive(false);
            if (FPSDebug != null)
                FPSDebug.SetActive(false);
            if (objDebug != null)
                objDebug.SetActive(false);
            //GameManager.Ins.isTestMode = false;
            //DataBucketEvent.Ins.isServerSidePush = true;
        }
        else
        {
            //GameManager.Ins.isTestMode = true;
            if (CheatObj != null)
                CheatObj.SetActive(true);
            if (objDebug != null)
                objDebug.SetActive(true);
            if (FPSDebug != null)
                FPSDebug.SetActive(true);
            if (adjust != null)
                adjust.environment = AdjustEnvironment.Sandbox;
        }

        if (mode == TypeMode.Marketing)
        {
            //GameManager.Ins.isTestMode = true;
            adjust.environment = AdjustEnvironment.Sandbox;
            if (CheatObj != null)
                CheatObj.SetActive(true);
            if (objDebug != null)
                objDebug.SetActive(false);
            if (FPSDebug != null)
                FPSDebug.SetActive(false);
        }
    }

    private void Awake()
    {
        if (!isReady)
        {
            isReady = true;
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        StartCoroutine(IE_InitSDKs());
    }

    private IEnumerator IE_InitSDKs()
    {
        yield return null;
        //DataBucketEvent.Ins.StartInitDatabuckets();
        yield return null;
        CC_LogFirebase.Ins.InitFirebase();
        yield return null;
        AppOpenAdsManager.Ins.InitAdmob();
        yield return null;
        CC_IAP.Ins.InitializeIAP();
    }

    #region [ Custom Ads ]
    public void ShowNativeFull(Action<bool> callback)
    {
        var isShow = false;
        // foreach (var na in listNativeInter)
        //     if (na.IsNativeFullLoaded())
        //     {
        //         na.ShowNativeFull(callback);
        //         isShow = true;
        //         break;
        //     }
        if (!isShow)
            callback?.Invoke(true);
    }

    public void SetControlParram(
        bool v1,
        bool v2,
        bool v3,
        string ctaPos,
        string setCloseButtonSize
    )
    {
        // foreach (var inter in listNativeInter)
        //     inter.SetControlParram(v1, v2, v3, ctaPos, setCloseButtonSize);
    }

    // private List<BannerMrec> listMrecShow = new List<BannerMrec>();

    public void ShowBannerMrec(Action<bool> callback, string where)
    {
        //if (VariableSystem.RemoveAds || VariableSystem.RemoveAdsHack) return;

        //Debug.Log("Show Banner Mrec");
        //bool show = false;
        //if (bannerMrec.IsLoadBannerMrec())
        //{
        //    bannerMrec.ShowMrec(where);
        //    show = true;
        //    listMrecShow.Add(bannerMrec);
        //}

        //if (!show)
        callback?.Invoke(true);
    }

    public void HideBannerMrec(Action<bool> callback)
    {
        callback?.Invoke(true);
        //if (listMrecShow.Count <= 1)
        //{
        //    callback?.Invoke(true);
        //    bannerMrec?.HideMrec();
        //    listMrecShow.Clear();
        //}
        //else
        //{
        //    callback?.Invoke(false);
        //    listMrecShow.RemoveAt(0);
        //}
    }
    #endregion
}
