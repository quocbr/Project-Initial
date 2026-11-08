using Firebase;
using Firebase.Analytics;
using quocbr.DesignPattern;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Unity.Notifications.Android;
using UnityEngine;

public class CC_LogFirebase : Singleton<CC_LogFirebase>
{
    private DependencyStatus dependencyStatus;
    private bool firebaseInitialized = false;

    public bool FirebaseInitialized => firebaseInitialized;

    public void InitFirebase()
    {
        StartCoroutine(InitializeFirebaseCoroutine());
    }
    public void CheckLaunchGame()
    {
        //if (GameHelper.IsOpenFromNotification())
        //{
        //    Debug.Log("[Tracking] App opened from Notification");
        //    ActionHelper.LogEvent("open_from_notify");
        //}
        //else
        //{
        //    Debug.Log("[Tracking] App opened from Launcher");
        //    ActionHelper.LogEvent("open_from_game");
        //}
    }

    private IEnumerator InitializeFirebaseCoroutine()
    {
        // Đợi đến khi kiểm tra và sửa lỗi dependencies hoàn tất
        var checkAndFixTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => checkAndFixTask.IsCompleted);

        dependencyStatus = checkAndFixTask.Result;

        if (dependencyStatus == DependencyStatus.Available)
        {
            InitializeFirebaseLogEvent();
        }
    }


    private void InitializeFirebaseLogEvent()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        FirebaseAnalytics.SetUserId(SystemInfo.deviceUniqueIdentifier);
        FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));

        firebaseInitialized = true;

        // Kiểm tra và lấy dữ liệu từ Remote Config nếu có
        RemoteConfig.Ins.CheckRemoteConfigValues();
    }
    public void LogEventWithString(string eventName)
    {
        if (firebaseInitialized)
            FirebaseAnalytics.LogEvent(eventName);
    }



    #region [ Key Log ]

    private string lv_win = "lv_win_";
    private string lv_lose = "lv_lose_";
    private string lv_play = "lv_play_";
    private string use_booster = "use_booster_";
    private string buy_iap = "buy_iap_";


    #endregion

    #region [ Funct Log ]

    public void WinLevel(int level)
    {
        ActionHelper.LogEvent(lv_win + level);
    }
    public void LoseLevel(int level)
    {
        ActionHelper.LogEvent(lv_lose + level);
    }
    public void PlayLevel(int level)
    {
        ActionHelper.LogEvent(lv_play + level);
    }

    public void BuyIAP(IAP_Product idProduct, int level)
    {
        ActionHelper.LogEvent(buy_iap + idProduct.ToString() + "_lv_" + level);
    }
    #endregion
}
