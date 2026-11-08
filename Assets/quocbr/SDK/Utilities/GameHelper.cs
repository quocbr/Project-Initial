using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameHelper
{
    #region [Remote config TMP]
    public static int LevelAutoBackHome = 8;
    #endregion

    public const string AdjustUrlStrategyChina = "china";
    public const string AdjustUrlStrategyIndia = "india";
    public const string AdjustUrlStrategyCn = "cn";

    public const string AdjustDataResidencyEU = "data-residency-eu";
    public const string AdjustDataResidencyTR = "data-residency-tr";
    public const string AdjustDataResidencyUS = "data-residency-us";

    public const string AdjustAdRevenueSourceAppLovinMAX = "applovin_max_sdk";
    public const string AdjustAdRevenueSourceMopub = "mopub";
    public const string AdjustAdRevenueSourceAdMob = "admob_sdk";
    public const string AdjustAdRevenueSourceIronSource = "ironsource_sdk";
    public const string AdjustAdRevenueSourceAdmost = "admost_sdk";
    public const string AdjustAdRevenueSourceUnity = "unity_sdk";
    public const string AdjustAdRevenueSourceHeliumChartboost = "helium_chartboost_sdk";
    public const string AdjustAdRevenueSourcePublisher = "publisher_sdk";

    private static List<GameObject> activePopups = new List<GameObject>();
    public static void RegisterPopup(GameObject popup)
    {
        if (!activePopups.Contains(popup))
            activePopups.Add(popup);
    }

    public static void UnregisterPopup(GameObject popup)
    {
        if (activePopups.Contains(popup))
            activePopups.Remove(popup);
    }

    public static bool HasPopupOpen()
    {
        return activePopups.Count > 0;
    }
}

public enum TypeAds
{
    Inter,
    Reward,
    Banner,
    Aoa,
    Impression,
    NativeInter,
    NativeBanner,
    BannerMrec,
    IAP,
    ServerIAP
}
