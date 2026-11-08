using Firebase.Extensions;
using Firebase.RemoteConfig;
using quocbr.DesignPattern;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Giá trị mặc định = cột Value trong bảng Remote Config.
/// Key trên Firebase Remote Config đặt TRÙNG tên field (vd "CoinStartGame").
/// </summary>
[Serializable]
public class ConfigData
{
    // ===== Gameplay =====
    public int CoinStartGame = 300;         // #1 Coin start game
    public int CoinWinLevel = 30;           // #2 Coin win level
    public int LevelShowRate = 4;           // #3 Lần đầu show popup rate (sau khi win level 4)
    public int LevelShowRateAgain = 10;     // #4 Sau bao nhiêu level show lại popup rate nếu chưa rate
    public int LevelSetIndex = 0;           // #5 Bộ level dùng để AB Test (mặc định bộ 0)
    public int LevelBackHome = 15;          // #6 Từ level này, bấm Next ở Win sẽ về Home

    public string LevelDifficultyConfig;

    // ===== Ads =====
    public int LevelShowInter = 10;         // Minimum level for placements other than Win.
    public string interAds_levelDelay_WinLevel = InterWinLevelSchedule.DefaultConfig;
    public int LevelShowRewarded = 3;       // #8 Level bắt đầu show reward x2 coin ở màn Win
    public int TimeIndexInterReward = 15;   // #9 Time between Reward vs Inter (giây)
    public int TimeIndexInter = 60;         // #10 Time between 2 Inter (tối thiểu, giây)
    public int TimeInterWin = 90;           // #11 Time between 2 Inter khi Win level
    public int TimeInterRetry = 90;         // #12 Time between 2 Inter khi bấm chơi lại (retry)
    public int TimeInterQuitHome = 60;      // #13 Time between 2 Inter khi thoát màn chơi ra Home

    public bool InterWin = true;            // #14 Inter khi Win level
    public bool InterLose = true;           // #15 Inter khi tap Try Again (fail level)
    public bool InterLoseHome = true;       // #16 Inter khi tap Home (fail level)
    public bool InterReplay = false;        // #17 Inter khi tap Replay
    public bool InterQuitHome = true;       // #18 Inter khi tap Quit

    public string DayShowInter = "0";       // #19 Day start show inter (giờ, 0 = ngày đầu chỉ chặn theo session)
    public int DelaySessionShow = 1;        // #20 Số level không show Inter trong session đầu tiên trong ngày
    public bool IsAllowOffline = true;      // #21 Cho phép tắt mạng khi chơi (false = hiện popup No Internet)

    // ===== SDK (không có trong bảng, giữ cho các script SDK dùng) =====
    public int TimeIndexAoaInter = 20;      // sau Inter bao lâu mới được show AOA
    public int TimeIndexAoaReward = 10;     // sau Reward bao lâu mới được show AOA
    public int TimeIndexAoa = 20;
    public int SecondWinShowInter = 3;      // Legacy key; Win now uses interAds_levelDelay_WinLevel.
    public int CountOpenShowAoa = 2;
    public int LevelShowBanner = 10;
    public string DayShowBanner = "1";
    public int TimeQueenGift = 86400;       // TimeManager còn dùng

    /// <summary>1: inter native || 2: 2 native full || 3: inter normal</summary>
    public int IndexAbTestNativeInter = 3;

    public bool IsDebug = false;
    public bool IsShowBanner = false;
    public bool IsShowAoaSplash = false;
    public bool IsShowAoaResume = false;

    public bool IsNativeInterWin = true;
    public bool IsNativeInterLose = true;
    public bool IsNativeInterReplay = true;
    public bool IsNativeInterQuitHome = true;

    public bool IsArea_1 = false;
    public bool IsArea_2 = false;
    public bool IsArea_3 = false;

    public string TimeReloadNativeBanner = "5000";
    public string PosCta = "LEFT";
    public string CloseButtonSize = "TINY";
    public string UrlIapServer = "https://validate.bzkiap.com/transaction-buy";

    public int CoinReviveOutOfSpace = 600;
    public int CoinReviveOutOfTime = 600;
    public int CoinBuyNest = 900;

    public bool UsePreWin = true;

    /// <summary>AdsConfig đếm thời gian tới mốc lớn nhất để mọi vị trí đều so được.</summary>
    public int MaxTimeBetweenInter =>
        Mathf.Max(TimeIndexInter, Mathf.Max(TimeInterRetry, TimeInterQuitHome));

    public int GetTimeBetweenInter(LocationRemoteInter last, LocationRemoteInter next)
    {
        LocationRemoteInter lastGroup = GetInterGroup(last);
        LocationRemoteInter nextGroup = GetInterGroup(next);

        if (lastGroup == LocationRemoteInter.None || lastGroup != nextGroup)
            return TimeIndexInter;

        switch (nextGroup)
        {
            //case LocationRemoteInter.Win: return TimeInterWin;
            case LocationRemoteInter.Lose: return TimeInterRetry;
            case LocationRemoteInter.QuitHome: return TimeInterQuitHome;
            default: return TimeIndexInter;
        }
    }

    private LocationRemoteInter GetInterGroup(LocationRemoteInter location)
    {
        switch (location)
        {
            case LocationRemoteInter.Replay: return LocationRemoteInter.Lose;
            case LocationRemoteInter.LoseHome: return LocationRemoteInter.QuitHome;
            default: return location;
        }
    }
}

public class RemoteConfig : Singleton<RemoteConfig>
{
    public bool isFetchDone { get; private set; }
    public bool isFetchSuccess { get; private set; }
    public ConfigData allConfigData = new ConfigData();

    private void Start()
    {
        if (PlayerPrefs.HasKey("Remote_IsAllowOffline"))
            allConfigData.IsAllowOffline = PlayerPrefs.GetInt("Remote_IsAllowOffline") == 1;

        ActionHelper.ConfigDebug();
    }

    private void SetValue(FirebaseRemoteConfig remoteConfig)
    {
        var keys = new HashSet<string>(remoteConfig.Keys);
        var c = allConfigData;

        int GetInt(string key, int current) =>
            keys.Contains(key) ? (int)remoteConfig.GetValue(key).LongValue : current;
        bool GetBool(string key, bool current) =>
            keys.Contains(key) ? remoteConfig.GetValue(key).BooleanValue : current;
        string GetString(string key, string current) =>
            keys.Contains(key) ? remoteConfig.GetValue(key).StringValue : current;

        c.LevelDifficultyConfig = GetString(nameof(c.LevelDifficultyConfig), c.LevelDifficultyConfig);

        // ===== Gameplay =====
        c.CoinStartGame = GetInt(nameof(c.CoinStartGame), c.CoinStartGame);
        c.CoinWinLevel = GetInt(nameof(c.CoinWinLevel), c.CoinWinLevel);
        c.LevelShowRate = GetInt(nameof(c.LevelShowRate), c.LevelShowRate);
        c.LevelShowRateAgain = GetInt(nameof(c.LevelShowRateAgain), c.LevelShowRateAgain);
        c.LevelSetIndex = GetInt(nameof(c.LevelSetIndex), c.LevelSetIndex);
        c.LevelBackHome = GetInt(nameof(c.LevelBackHome), c.LevelBackHome);

        // ===== Ads =====
        c.LevelShowInter = GetInt(nameof(c.LevelShowInter), c.LevelShowInter);
        c.interAds_levelDelay_WinLevel = GetString(nameof(c.interAds_levelDelay_WinLevel), c.interAds_levelDelay_WinLevel);
        c.LevelShowRewarded = GetInt(nameof(c.LevelShowRewarded), c.LevelShowRewarded);
        c.TimeIndexInterReward = GetInt(nameof(c.TimeIndexInterReward), c.TimeIndexInterReward);
        c.TimeIndexInter = GetInt(nameof(c.TimeIndexInter), c.TimeIndexInter);
        c.TimeInterWin = GetInt(nameof(c.TimeInterWin), c.TimeInterWin);
        c.TimeInterRetry = GetInt(nameof(c.TimeInterRetry), c.TimeInterRetry);
        c.TimeInterQuitHome = GetInt(nameof(c.TimeInterQuitHome), c.TimeInterQuitHome);

        c.InterWin = GetBool(nameof(c.InterWin), c.InterWin);
        c.InterLose = GetBool(nameof(c.InterLose), c.InterLose);
        c.InterLoseHome = GetBool(nameof(c.InterLoseHome), c.InterLoseHome);
        c.InterReplay = GetBool(nameof(c.InterReplay), c.InterReplay);
        c.InterQuitHome = GetBool(nameof(c.InterQuitHome), c.InterQuitHome);

        c.DayShowInter = GetString(nameof(c.DayShowInter), c.DayShowInter);
        c.DelaySessionShow = GetInt(nameof(c.DelaySessionShow), c.DelaySessionShow);
        c.IsAllowOffline = GetBool(nameof(c.IsAllowOffline), c.IsAllowOffline);

        // ===== SDK =====
        c.TimeIndexAoaInter = GetInt(nameof(c.TimeIndexAoaInter), c.TimeIndexAoaInter);
        c.TimeIndexAoaReward = GetInt(nameof(c.TimeIndexAoaReward), c.TimeIndexAoaReward);
        c.TimeIndexAoa = GetInt(nameof(c.TimeIndexAoa), c.TimeIndexAoa);
        c.SecondWinShowInter = GetInt(nameof(c.SecondWinShowInter), c.SecondWinShowInter);
        c.CountOpenShowAoa = GetInt(nameof(c.CountOpenShowAoa), c.CountOpenShowAoa);
        c.LevelShowBanner = GetInt(nameof(c.LevelShowBanner), c.LevelShowBanner);
        c.DayShowBanner = GetString(nameof(c.DayShowBanner), c.DayShowBanner);
        c.TimeQueenGift = GetInt(nameof(c.TimeQueenGift), c.TimeQueenGift);
        c.IndexAbTestNativeInter = GetInt(nameof(c.IndexAbTestNativeInter), c.IndexAbTestNativeInter);

        c.IsDebug = GetBool(nameof(c.IsDebug), c.IsDebug);
        c.IsShowBanner = GetBool(nameof(c.IsShowBanner), c.IsShowBanner);
        c.IsShowAoaSplash = GetBool(nameof(c.IsShowAoaSplash), c.IsShowAoaSplash);
        c.IsShowAoaResume = GetBool(nameof(c.IsShowAoaResume), c.IsShowAoaResume);

        c.IsNativeInterWin = GetBool(nameof(c.IsNativeInterWin), c.IsNativeInterWin);
        c.IsNativeInterLose = GetBool(nameof(c.IsNativeInterLose), c.IsNativeInterLose);
        c.IsNativeInterReplay = GetBool(nameof(c.IsNativeInterReplay), c.IsNativeInterReplay);
        c.IsNativeInterQuitHome = GetBool(nameof(c.IsNativeInterQuitHome), c.IsNativeInterQuitHome);

        c.IsArea_1 = GetBool(nameof(c.IsArea_1), c.IsArea_1);
        c.IsArea_2 = GetBool(nameof(c.IsArea_2), c.IsArea_2);
        c.IsArea_3 = GetBool(nameof(c.IsArea_3), c.IsArea_3);

        c.UsePreWin = GetBool(nameof(c.UsePreWin), c.UsePreWin);

        c.TimeReloadNativeBanner = GetString(nameof(c.TimeReloadNativeBanner), c.TimeReloadNativeBanner);
        c.PosCta = GetString(nameof(c.PosCta), c.PosCta);
        c.CloseButtonSize = GetString(nameof(c.CloseButtonSize), c.CloseButtonSize);
        c.UrlIapServer = GetString(nameof(c.UrlIapServer), c.UrlIapServer);

        PlayerPrefs.SetInt("Remote_IsAllowOffline", c.IsAllowOffline ? 1 : 0);
        PlayerPrefs.SetInt("Remote_LevelSetIndex", c.LevelSetIndex);
        PlayerPrefs.Save();
        if (c.IsDebug)
            ShowDebug();
    }

    public void ShowDebug()
    {
        Debug.Log("===== Remote Config =====\n" + JsonUtility.ToJson(allConfigData, true));
    }

    public Task CheckRemoteConfigValues()
    {
        Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }

    private void FetchComplete(Task fetchTask)
    {
        if (!fetchTask.IsCompleted)
        {
            InitDataGame.Ins.InitData();
            isFetchDone = true;
            return;
        }

        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        var info = remoteConfig.Info;
        if (info.LastFetchStatus != LastFetchStatus.Success)
        {
            InitDataGame.Ins.InitData();
            isFetchDone = true;
            return;
        }

        // Fetch successful. Parameter values must be activated to use.
        remoteConfig.ActivateAsync()
          .ContinueWithOnMainThread(
            task =>
            {
                Debug.Log("Fetch Remote");
                SetValue(remoteConfig);
                AdsConfig.Ins.Init();
                InitDataGame.Ins.InitData();
                KeepObject.Instance.SetControlParram(allConfigData.IsArea_1, allConfigData.IsArea_2, allConfigData.IsArea_3, allConfigData.PosCta, allConfigData.CloseButtonSize);
                ActionHelper.ConfigDebug();
                isFetchSuccess = true;
                isFetchDone = true;
            });
    }
}
