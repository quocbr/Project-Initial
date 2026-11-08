using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using quocbr.DesignPattern;
using UnityEngine;
using UnityEngine.Events;
public enum TypeAdsInter
{
    None,
    Inter,
    Inter_Native,
    Native_Native
}
[Serializable]
public class DataCountTime
{
    public bool isCountTime = false;
    public float time = 10;

    public DataCountTime(bool isCountTime, float time)
    {
        this.isCountTime = isCountTime;
        this.time = time;
    }
}

// Inclusive level ranges: "1-10=2;11-30=1;31-50=1;51+=1".
// Delay 0 allows every win; -1 disables Win inter. Positive delays keep the win-count rule.
public sealed class InterWinLevelSchedule
{
    public const string DefaultConfig = "1-10=2;11-30=1;31-50=1;51+=1";

    private struct Rule
    {
        public int Min;
        public int Max;
        public int Delay;
    }

    private readonly Rule[] rules;

    private InterWinLevelSchedule(Rule[] rules) => this.rules = rules;

    public int GetDelay(int completedLevel)
    {
        foreach (Rule rule in rules)
        {
            if (completedLevel < rule.Min) break;
            if (completedLevel <= rule.Max) return rule.Delay;
        }
        // An uncovered level does not offer a Win inter.
        return -1;
    }

    public bool ShouldShow(int completedLevel, int winsSinceLastShown)
    {
        int delay = GetDelay(completedLevel);
        return delay >= 0 && (delay == 0 || winsSinceLastShown > delay);
    }

    public static bool TryParse(string value, out InterWinLevelSchedule schedule)
    {
        schedule = null;
        if (string.IsNullOrWhiteSpace(value)) return false;
        string trimmed = value.Trim();
        if (trimmed == "0" || trimmed == "-1")
        {
            schedule = new InterWinLevelSchedule(new[] {
                new Rule { Min = 1, Max = int.MaxValue, Delay = trimmed == "0" ? 0 : -1 }
            });
            return true;
        }
        List<Rule> parsed = new();
        foreach (string part in value.Split(';'))
        {
            string entry = part.Trim();
            if (entry.Length == 0) continue;
            string[] pair = entry.Split('=');
            if (pair.Length != 2 || !DelayValue(pair[1], out int delay)) return false;
            string range = pair[0].Trim();
            int min;
            int max;
            if (range.EndsWith("+", StringComparison.Ordinal))
            {
                if (!PositiveInt(range.Substring(0, range.Length - 1), out min)) return false;
                max = int.MaxValue;
            }
            else
            {
                string[] bounds = range.Split('-');
                if (bounds.Length != 2 || !PositiveInt(bounds[0], out min) ||
                    !PositiveInt(bounds[1], out max) || max < min) return false;
            }
            parsed.Add(new Rule { Min = min, Max = max, Delay = delay });
        }
        if (parsed.Count == 0) return false;
        parsed.Sort((a, b) => a.Min.CompareTo(b.Min));
        for (int i = 1; i < parsed.Count; i++)
            if (parsed[i].Min <= parsed[i - 1].Max) return false;
        schedule = new InterWinLevelSchedule(parsed.ToArray());
        return true;
    }

    private static bool PositiveInt(string value, out int number) =>
        int.TryParse(value.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out number) && number > 0;

    private static bool DelayValue(string value, out int number)
    {
        string trimmed = value.Trim();
        if (trimmed == "-1")
        {
            number = -1;
            return true;
        }
        return int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out number) && number >= 0;
    }
}

public class AdsConfig : Singleton<AdsConfig>
{
    private DataCountTime countTimeAoaInter = new DataCountTime(false, 10);
    private DataCountTime countTimeAoaReward = new DataCountTime(false, 10);
    private DataCountTime countTimeAoa = new DataCountTime(false, 10);
    private DataCountTime countTimeInter = new DataCountTime(false, 20);
    private DataCountTime countTimeInterReward = new DataCountTime(false, 10);

    private ConfigData configData = null;

    private bool firstShowInter = false;
    private string winScheduleSource;
    private InterWinLevelSchedule winSchedule;
    private LocationRemoteInter lastInterLocation = LocationRemoteInter.None;
    private LocationRemoteInter pendingInterLocation = LocationRemoteInter.None;

    private void Update()
    {
        if (configData == null)
            return;

        CountTime(countTimeAoaInter, configData.TimeIndexAoaInter);
        CountTime(countTimeAoaReward, configData.TimeIndexAoaReward);
        CountTime(countTimeAoa, configData.TimeIndexAoa);
        CountTime(countTimeInter, configData.MaxTimeBetweenInter);
        CountTime(countTimeInterReward, configData.TimeIndexInterReward);
    }
    private void CountTime(DataCountTime dataCountTime, int timeCompare)
    {
        if (dataCountTime.isCountTime)
        {
            dataCountTime.time += Time.deltaTime;
            if (dataCountTime.time >= timeCompare)
                dataCountTime.isCountTime = false;
        }
    }
    private bool isInit = false;
    public void Init()
    {
        if (isInit) return;
        isInit = true;

        configData = RemoteConfig.Ins.allConfigData;
        countTimeAoaInter.time = configData.TimeIndexAoaInter;
        countTimeAoaReward.time = configData.TimeIndexAoaReward;
        countTimeAoa.time = configData.TimeIndexAoa;
        countTimeInter.time = configData.MaxTimeBetweenInter;
        countTimeInterReward.time = configData.TimeIndexInterReward;
    }
    public void CheckShowInter(Action<bool> callback = null, LocationRemoteInter locationRemoteInter = LocationRemoteInter.None)
    {
        if (configData == null) Init();
        if (configData != null)
        {
            if (countTimeInterReward.time < configData.TimeIndexInterReward)
            {
                Debug.Log("Time Not Show Inter Reward");
                callback?.Invoke(false);
                return;
            }

            if (VariableSystem.IndexLevel < configData.LevelShowInter)
            {
                Debug.Log("Level Not Show Inter");
                callback?.Invoke(false);
                return;
            }

            if (locationRemoteInter == LocationRemoteInter.Win)
            {
                int completedLevel = VariableSystem.IndexLevel;
                InterWinLevelSchedule schedule = GetWinSchedule();
                // Disabled ranges also apply to the first inter of a new install.
                if (schedule.GetDelay(completedLevel) < 0)
                {
                    Debug.Log($"Win inter disabled at level {completedLevel}.");
                    callback?.Invoke(false);
                    return;
                }
                if (firstShowInter == false)
                {
                    if (!VariableSystem.IsFirstOpenGame && schedule.ShouldShow(completedLevel, VariableSystem.CountWinInterAds))
                    {
                        firstShowInter = true;
                    }
                    else if (VariableSystem.IsFirstOpenGame)
                    {
                        firstShowInter = true;
                    }
                    else
                    {
                        Debug.Log("Win Not Show Inter First");
                        callback?.Invoke(false);
                        return;
                    }
                }
                else
                {
                    if (!schedule.ShouldShow(completedLevel, VariableSystem.CountWinInterAds))
                    {
                        Debug.Log("Win Not Show Inter Second");
                        callback?.Invoke(false);
                        return;
                    }
                }
            }
            else
            {
                if (countTimeInter.time < configData.GetTimeBetweenInter(lastInterLocation, locationRemoteInter))
                {
                    Debug.Log($"Time Not Show Inter: {countTimeInter.time} < {configData.GetTimeBetweenInter(lastInterLocation, locationRemoteInter)}");
                    Debug.Log("Time Not Show Inter");
                    callback?.Invoke(false);
                    return;
                }
            }
        }
        pendingInterLocation = locationRemoteInter;
        CC_Interface.Ins.ShowInter(callback, locationRemoteInter);
    }

    private InterWinLevelSchedule GetWinSchedule()
    {
        string source = configData.interAds_levelDelay_WinLevel;
        if (winSchedule == null || !string.Equals(source, winScheduleSource, StringComparison.Ordinal))
        {
            winScheduleSource = source;
            if (!InterWinLevelSchedule.TryParse(source, out winSchedule))
            {
                Debug.LogWarning("Invalid interAds_levelDelay_WinLevel; using " + InterWinLevelSchedule.DefaultConfig);
                InterWinLevelSchedule.TryParse(InterWinLevelSchedule.DefaultConfig, out winSchedule);
            }
        }
        return winSchedule;
    }

    public void CheckShowAoa(UnityAction callback = null)
    {
        if (configData != null)
        {
            if (countTimeAoaInter.time < configData.TimeIndexAoaInter)
            {
                callback?.Invoke();
                return;
            }

            if (countTimeAoa.time < configData.TimeIndexAoa)
            {
                callback?.Invoke();
                return;
            }

            if (countTimeAoaReward.time < configData.TimeIndexAoaReward)
            {
                callback?.Invoke();
                return;
            }
        }

        countTimeAoa.time = 0;
        countTimeAoa.isCountTime = true;

        CC_Interface.Ins.ShowAppOpenAd(callback);
    }

    public void OnRewardShown()
    {
        countTimeInterReward.time = 0;
        countTimeInterReward.isCountTime = true;
    }

    public void OnInterShown()
    {
        countTimeInter.time = 0;
        countTimeInter.isCountTime = true;

        countTimeAoaInter.time = 0;
        countTimeAoaInter.isCountTime = true;

        lastInterLocation = pendingInterLocation;

        // Keep wins when an ad is unavailable or blocked; reset only on a displayed Win inter.
        if (pendingInterLocation == LocationRemoteInter.Win)
            VariableSystem.CountWinInterAds = 0;

        pendingInterLocation = LocationRemoteInter.None;
        //RemoveAdsPopupTrigger.OnInterShown();
    }
}
