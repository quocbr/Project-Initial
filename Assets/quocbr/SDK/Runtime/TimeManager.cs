using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using System.Globalization;
using quocbr.DesignPattern;

[Serializable]
public class CountdownTimer
{
    public string timerId;
    public double endTimestamp;
    public float remainingTime;
    public bool isRunning;
    public List<TextMeshProUGUI> listTextCountTime = new List<TextMeshProUGUI>();
    public Action onTimerComplete; // callback khi timer kết thúc

    public CountdownTimer(string id, double endTime)
    {
        timerId = id;
        endTimestamp = endTime;
        isRunning = true;
    }
}

public class TimeID
{
    public const string ForestCall = "QueenGift";
    public const string DelayForestCall = "DelayQueenGift";
    public const string X2TimeBooster = "X2TimeBooster";
}

public enum TypeTimer
{
    None,
    ForestCall,
    DelayForestCall
}

[Serializable]
public class DataTimer
{
    public TypeTimer timerType;
    public int totalTime; // second
}

public class TimeManager : Singleton<TimeManager>
{
    public List<DataTimer> listDataTimer = new List<DataTimer>();

    private Dictionary<string, CountdownTimer> timers = new Dictionary<string, CountdownTimer>();
    private const string SAVE_PREFIX = "MultiTimer_";
    private const string SAVE_LIST_KEY = SAVE_PREFIX + "LIST";

    public void InitData()
    {
        LoadAllTimers();
        CheckInstallDate();
        IncreaseSessionCount();
        CheckFirstSessionOfDay(); 
    }

    #region [ Count Day / Interstitial Logic ]
    private const string INSTALL_DATE_KEY = "InstallDate";
    private const string SESSION_COUNT_KEY = "TM_SessionCount";
    private float hoursToShowInter = 24;
    private float hoursToShowBanner = 24;
    private bool IsShowInter = false;
    private bool IsShowBanner = false;
    private const string LAST_SESSION_DAY_KEY = "TM_LastSessionDay";
    private bool isFirstSessionOfDay;
    public bool IsFirstSessionOfDay => isFirstSessionOfDay;
    private int levelsFinishedThisSession;

    private void CheckFirstSessionOfDay()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        isFirstSessionOfDay = PlayerPrefs.GetString(LAST_SESSION_DAY_KEY, "") != today;
        PlayerPrefs.SetString(LAST_SESSION_DAY_KEY, today);
        PlayerPrefs.Save();

        levelsFinishedThisSession = 0;
    }

    /// <summary>Gọi mỗi khi chơi xong 1 level (Win hoặc Lose).</summary>
    public void OnLevelFinished()
    {
        levelsFinishedThisSession++;
    }

    // Đang ở session đầu ngày và chưa chơi quá N level → chưa được hiện Inter.
    private bool IsBlockedByFirstSessionOfDay()
    {
        if (!isFirstSessionOfDay) return false;

        int delayLevels = RemoteConfig.Ins.allConfigData.DelaySessionShow;
        return levelsFinishedThisSession <= delayLevels;
    }

    // Số session (lần mở app) đã trải qua, tính cả session hiện tại. Dùng để
    // chờ đủ DelaySessionShow session trong ngày D0 trước khi cho hiện Inter
    // khi DayShowInter == 0 (xem CheckShowInterAds).
    private void IncreaseSessionCount()
    {
        int count = PlayerPrefs.GetInt(SESSION_COUNT_KEY, 0) + 1;
        PlayerPrefs.SetInt(SESSION_COUNT_KEY, count);
        PlayerPrefs.Save();
    }

    public int GetSessionCount()
    {
        return PlayerPrefs.GetInt(SESSION_COUNT_KEY, 0);
    }

    // D0 = còn trong vòng 24h kể từ lúc cài đặt.
    private bool IsInFirstDaySinceInstall()
    {
        if (!PlayerPrefs.HasKey(INSTALL_DATE_KEY))
        {
            string nowStr = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            PlayerPrefs.SetString(INSTALL_DATE_KEY, nowStr);
            PlayerPrefs.Save();
            return true; // vừa cài -> chắc chắn đang ở D0
        }

        string installDateStr = PlayerPrefs.GetString(INSTALL_DATE_KEY);

        if (DateTime.TryParse(installDateStr, null, DateTimeStyles.RoundtripKind, out DateTime installDate))
        {
            TimeSpan diff = DateTime.UtcNow - installDate;
            return diff.TotalHours < 24;
        }

        string now2 = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        PlayerPrefs.SetString(INSTALL_DATE_KEY, now2);
        PlayerPrefs.Save();
        return true;
    }

    private Coroutine updateTimersCoroutine;

    public bool CheckDayShowInter()
    {
        CheckShowInterAds();
        return IsShowInter;
    }

    public bool CheckDayShowBanner()
    {
        CheckShowBannerAds();
        return IsShowBanner;
    }

    private void CheckShowInterAds()
    {
        IsShowInter = false;

        if (RemoteConfig.Ins == null) return;

        string val = RemoteConfig.Ins.allConfigData.DayShowInter.Replace(',', '.');
        if (!float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float day))
            day = 0;

        IsShowInter = GetDaysSinceInstall() >= Mathf.FloorToInt(day);
    }

    public int GetDaysSinceInstall()
    {
        DateTime installLocal = GetInstallDateUtc().ToLocalTime().Date;
        return Mathf.Max(0, (DateTime.Now.Date - installLocal).Days);
    }

    private DateTime GetInstallDateUtc()
    {
        if (PlayerPrefs.HasKey(INSTALL_DATE_KEY) &&
            DateTime.TryParse(PlayerPrefs.GetString(INSTALL_DATE_KEY), null,
                            DateTimeStyles.RoundtripKind, out DateTime installDate))
        {
            return installDate;
        }

        DateTime now = DateTime.UtcNow;
        PlayerPrefs.SetString(INSTALL_DATE_KEY, now.ToString("o", CultureInfo.InvariantCulture));
        PlayerPrefs.Save();
        return now;
    }

    private void CheckShowBannerAds()
    {
        IsShowBanner = false;

        if (RemoteConfig.Ins == null) return;

        string bannerVal = RemoteConfig.Ins.allConfigData.DayShowBanner;
        bannerVal = bannerVal.Replace(',', '.');

        if (!float.TryParse(bannerVal, NumberStyles.Float, CultureInfo.InvariantCulture, out float bannerTime))
            bannerTime = 0;

        hoursToShowBanner = bannerTime;


        if (hoursToShowBanner <= 0)
        {
            IsShowBanner = true;
            return;
        }

        if (!PlayerPrefs.HasKey(INSTALL_DATE_KEY))
        {
            string nowStr = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            PlayerPrefs.SetString(INSTALL_DATE_KEY, nowStr);
            PlayerPrefs.Save();
            return;
        }

        string installDateStr = PlayerPrefs.GetString(INSTALL_DATE_KEY);

        if (DateTime.TryParse(installDateStr, null, DateTimeStyles.RoundtripKind, out DateTime installDate))
        {
            TimeSpan diff = DateTime.UtcNow - installDate;
            if (diff.TotalHours >= hoursToShowBanner)
            {
                IsShowBanner = true;
            }

        }
        else
        {
            string nowStr = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            PlayerPrefs.SetString(INSTALL_DATE_KEY, nowStr);
            PlayerPrefs.Save();
        }
    }

    private void CheckInstallDate()
    {
        IsShowInter = false;
        IsShowBanner = false;

        if (RemoteConfig.Ins == null) return;

        string val = RemoteConfig.Ins.allConfigData.DayShowInter;
        val = val.Replace(',', '.');

        if (!float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float time))
            time = 0;

        hoursToShowInter = time;

        string bannerVal = RemoteConfig.Ins.allConfigData.DayShowBanner;
        bannerVal = bannerVal.Replace(',', '.');

        if (!float.TryParse(bannerVal, NumberStyles.Float, CultureInfo.InvariantCulture, out float bannerTime))
            bannerTime = 0;

        hoursToShowBanner = bannerTime;


        if (hoursToShowInter <= 0)
        {
            IsShowInter = true;
            return;
        }

        if (hoursToShowBanner <= 0)
        {
            IsShowBanner = true;
            return;
        }

        if (!PlayerPrefs.HasKey(INSTALL_DATE_KEY))
        {
            string nowStr = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            PlayerPrefs.SetString(INSTALL_DATE_KEY, nowStr);
            PlayerPrefs.Save();
            return;
        }

        string installDateStr = PlayerPrefs.GetString(INSTALL_DATE_KEY);

        if (DateTime.TryParse(installDateStr, null, DateTimeStyles.RoundtripKind, out DateTime installDate))
        {
            TimeSpan diff = DateTime.UtcNow - installDate;
            if (diff.TotalHours >= hoursToShowInter)
            {
                IsShowInter = true;
            }
            if (diff.TotalHours >= hoursToShowBanner)
            {
                IsShowBanner = true;
            }

        }
        else
        {
            string nowStr = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            PlayerPrefs.SetString(INSTALL_DATE_KEY, nowStr);
            PlayerPrefs.Save();
        }
    }

    #endregion

    #region [ Check New Day ]

    private const string LAST_CHECK_DAY_PREFIX = "LastCheckDay_";

    /// <summary>
    /// Kiểm tra xem đã sang ngày mới hay chưa (so với lần gọi gần nhất cùng key).
    /// Nếu đã sang ngày mới, tự động cập nhật lại mốc ngày lưu trữ.
    /// </summary>
    /// <param name="key">Định danh riêng cho từng mục đích check (VD: "DailyReward", "DailyQuest"...)</param>
    public bool CheckIsNewDay(string key)
    {
        string prefKey = LAST_CHECK_DAY_PREFIX + key;
        DateTime now = DateTime.Now;

        if (!PlayerPrefs.HasKey(prefKey))
        {
            PlayerPrefs.SetString(prefKey, now.ToString("o", CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
            return true;
        }

        string lastStr = PlayerPrefs.GetString(prefKey);
        bool isNewDay = true;

        if (DateTime.TryParse(lastStr, null, DateTimeStyles.RoundtripKind, out DateTime lastDate))
        {
            isNewDay = now.Date > lastDate.Date;
        }

        if (isNewDay)
        {
            PlayerPrefs.SetString(prefKey, now.ToString("o", CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }

        return isNewDay;
    }

    #endregion

    public DataTimer GetDataTimer(TypeTimer type)
    {
        return listDataTimer.FirstOrDefault(t => t.timerType == type);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveAllTimers();
        else
            LoadAllTimers();
    }

    public  void OnApplicationQuit()
    {
        SaveAllTimers();
    }

    // ====== PUBLIC API ======

    public void StartTimer(string id, float duration, Action onComplete = null)
    {
        double endTime = GetUnixTimeNow() + duration;
        var timer = new CountdownTimer(id, endTime);
        timer.onTimerComplete = onComplete;
        timers[id] = timer;
        SaveTimer(timer);
        if (updateTimersCoroutine == null)
        {
            updateTimersCoroutine = StartCoroutine(UpdateRoutine());
        }
    }

    public void SetCallback(string id, Action onComplete)
    {
        if (timers.TryGetValue(id, out var timer))
            timer.onTimerComplete = onComplete;
    }

    public void SetTextTarget(string id, TextMeshProUGUI text)
    {
        if (timers.ContainsKey(id))
        {
            if (!timers[id].listTextCountTime.Contains(text))
                timers[id].listTextCountTime.Add(text);
            timers[id].listTextCountTime.RemoveAll(t => t == null);
            UpdateText(timers[id]);
        }
    }

    public void RemoveTextTarget(string id, TextMeshProUGUI text)
    {
        if (timers.ContainsKey(id))
        {
            if (timers[id].listTextCountTime.Contains(text))
                timers[id].listTextCountTime.Remove(text);
            timers[id].listTextCountTime.RemoveAll(t => t == null);
            UpdateText(timers[id]);
        }
    }

    public float GetRemaining(string id)
    {
        if (!timers.ContainsKey(id)) return 0;
        return timers[id].remainingTime;
    }

    public bool IsRunning(string id)
    {
        return timers.ContainsKey(id) && timers[id].isRunning;
    }

    // ====== INTERNAL ======

    private IEnumerator UpdateRoutine()
    {
        while (true)
        {
            double now = GetUnixTimeNow();

            foreach (var pair in timers.Values.ToList())
            {
                if (!pair.isRunning) continue;


                pair.remainingTime = Mathf.Max(0, (float)(pair.endTimestamp - now));

                if (pair.remainingTime <= 0)
                {
                    pair.isRunning = false;
                    PlayerPrefs.DeleteKey(SAVE_PREFIX + pair.timerId);
                    pair.onTimerComplete?.Invoke();
                }

                UpdateText(pair);
            }

            yield return new WaitForSeconds(1);
        }
    }

    private void UpdateText(CountdownTimer timer)
    {
        if (timer.listTextCountTime == null || timer.listTextCountTime.Count == 0)
            return;

        string formatted = Ultils.ReturnSecondsToDays(Mathf.FloorToInt(timer.remainingTime));

        foreach (var text in timer.listTextCountTime)
        {
            if (text != null)
                text.text = formatted;
        }
    }

    // ====== SAVE / LOAD ======

    private void SaveTimer(CountdownTimer timer)
    {
        PlayerPrefs.SetString(SAVE_PREFIX + timer.timerId, timer.endTimestamp.ToString(CultureInfo.InvariantCulture));

        var keyList = timers.Keys.ToList();
        PlayerPrefs.SetString(SAVE_LIST_KEY, string.Join(",", keyList));

        PlayerPrefs.Save();
    }

    private void SaveAllTimers()
    {
        foreach (var timer in timers.Values)
        {
            if (timer.isRunning)
                SaveTimer(timer);
        }
    }

    public void LoadAllTimers()
    {
        if (timers == null)
            timers = new Dictionary<string, CountdownTimer>();

        string keyList = PlayerPrefs.GetString(SAVE_LIST_KEY, "");
        if (string.IsNullOrEmpty(keyList)) return;

        double now = GetUnixTimeNow();

        foreach (string id in keyList.Split(','))
        {
            if (string.IsNullOrEmpty(id)) continue;

            string key = SAVE_PREFIX + id;
            if (!PlayerPrefs.HasKey(key)) continue;

            string raw = PlayerPrefs.GetString(key);
            if (!double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out double endTime))
                continue;

            // Timer bình thường
            if (!timers.ContainsKey(id))
            {
                var timer = new CountdownTimer(id, endTime);
                timers[id] = timer;
            }
            else
            {
                timers[id].endTimestamp = endTime;
                timers[id].isRunning = true;
            }

            if (id == TimeID.DelayForestCall)
            {
                timers[id].onTimerComplete = () =>
                {
                    StartTimer(TimeID.ForestCall, RemoteConfig.Ins.allConfigData.TimeQueenGift);
                };
            }
            if (id == TimeID.ForestCall)
            {
                timers[id].onTimerComplete = () =>
                {
                    StartTimer(TimeID.DelayForestCall, RemoteConfig.Ins.allConfigData.TimeQueenGift);
                };
            }
        }

        // Đảm bảo không start coroutine nhiều lần
        if (updateTimersCoroutine != null)
        {
            StopCoroutine(updateTimersCoroutine);
        }
        updateTimersCoroutine = StartCoroutine(UpdateRoutine());
    }

    private double GetUnixTimeNow()
    {
        return (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public void AddTime(TypeTimer type, float extraSeconds)
    {
        string id = type.ToString();

        if (timers.TryGetValue(id, out var timer))
        {
            if (timer.isRunning)
            {
                timer.endTimestamp += extraSeconds;
                SaveTimer(timer);
                Debug.Log($"⏱ Đã cộng thêm {extraSeconds} giây cho timer {id}. Thời gian còn lại: {timer.remainingTime:F1}s");
            }
            else
            {
                Debug.LogWarning($"Timer {id} không chạy, không thể cộng thêm thời gian.");
            }
        }
        else
        {
            Debug.LogWarning($"Timer {id} chưa tồn tại, không thể cộng thêm thời gian.");
        }
    }

    public void StopTimer(TypeTimer type)
    {
        string id = type.ToString();

        if (timers.ContainsKey(id))
        {
            var timer = timers[id];
            timer.isRunning = false;
            timer.remainingTime = 0;
            timer.listTextCountTime.ForEach(t =>
            {
                if (t != null)
                    t.text = "00:00";
            });

            PlayerPrefs.DeleteKey(SAVE_PREFIX + id);
            PlayerPrefs.Save();

            Debug.Log($"⏹ Timer {id} đã được dừng ngay lập tức.");
        }
    }
}