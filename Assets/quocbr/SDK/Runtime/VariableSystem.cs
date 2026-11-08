using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class VariableSystem
{

    #region [ Setting ]
    public static bool Music
    {
        get => PlayerPrefs.GetFloat("Music", 1) == 1;
        set
        {
            PlayerPrefs.SetFloat("Music", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    public static bool Sound
    {
        get => PlayerPrefs.GetFloat("Sound", 1) == 1;
        set
        {
            PlayerPrefs.SetFloat("Sound", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    public static bool Vibration
    {
        get => PlayerPrefs.GetInt("Vibration", 1) == 1;
        set
        {
            PlayerPrefs.SetInt("Vibration", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    public static string Language
    {
        get => PlayerPrefs.GetString("Language", "en");
        set
        {
            PlayerPrefs.SetString("Language", value);
            PlayerPrefs.Save();
        }
    }

    public static string NameUser
    {
        get
        {
            // Lần đầu chưa có tên thì random 1 lần rồi lưu lại, tránh mỗi lần đọc ra 1 tên khác
            if (!PlayerPrefs.HasKey("NameUser"))
            {
                PlayerPrefs.SetString("NameUser", string.Format("Player{0:000000}", Random.Range(0, 999999)));
                PlayerPrefs.Save();
            }
            return PlayerPrefs.GetString("NameUser");
        }
        set
        {
            PlayerPrefs.SetString("NameUser", value);
            PlayerPrefs.Save();
        }
    }

    #endregion

    #region [ Currency ]
    public static int StoreCoin
    {
        get => PlayerPrefs.GetInt("StoreCoin", 300);
        set => PlayerPrefs.SetInt("StoreCoin", value);
    }
    public static int StoreHeart
    {
        get => PlayerPrefs.GetInt("StoreHeart", 5);
        set => PlayerPrefs.SetInt("StoreHeart", value);
    }

    public static int StoreStar
    {
        get => PlayerPrefs.GetInt("StoreStar", 0);
        set => PlayerPrefs.SetInt("StoreStar", value);
    }

    public static int StoreAchievementCurrency
    {
        get => PlayerPrefs.GetInt("StoreAchievementCurrency", 0);
        set => PlayerPrefs.SetInt("StoreAchievementCurrency", value);
    }

    #endregion

    #region [ Remote ]
    public static bool IsDebug
    {
        get => PlayerPrefs.GetInt("IsDebug", 0) == 1;
        set => PlayerPrefs.SetInt("IsDebug", value ? 1 : 0);
    }

    public static int CountOpenShowAoa
    {
        get => PlayerPrefs.GetInt("CountOpenShowAoa", 0);
        set => PlayerPrefs.SetInt("CountOpenShowAoa", value);
    }
    #endregion

    #region [ Hack ]
    public static bool OnOffBG
    {
        get => PlayerPrefs.GetInt("OnOffBG", 0) == 1;
        set => PlayerPrefs.SetInt("OnOffBG", value ? 1 : 0);
    }
    public static bool OnOffUI
    {
        get => PlayerPrefs.GetInt("OnOffUI", 0) == 1;
        set => PlayerPrefs.SetInt("OnOffUI", value ? 1 : 0);
    }
    public static bool RemoveAdsHack
    {
        get => PlayerPrefs.GetInt("RemoveAdsHack", 0) == 1;
        set => PlayerPrefs.SetInt("RemoveAdsHack", value ? 1 : 0);
    }

    #endregion

    #region [ IAP ]
    public static bool IsHasUserConsent
    {
        get => PlayerPrefs.GetInt("IsHasUserConsent", 0) == 1;
        set => PlayerPrefs.SetInt("IsHasUserConsent", value ? 1 : 0);
    }
    public static bool IsRestoreDone
    {
        get => PlayerPrefs.GetInt("IsRestoreDone", 0) == 1;
        set => PlayerPrefs.SetInt("IsRestoreDone", value ? 1 : 0);
    }
    public static bool IsShowBtnRestore
    {
        get => PlayerPrefs.GetInt("IsShowBtnRestore", 1) == 1;
        set => PlayerPrefs.SetInt("IsShowBtnRestore", value ? 1 : 0);
    }
    public static bool RemoveAds
    {
        get => PlayerPrefs.GetInt("RemoveAds", 0) == 1;
        set => PlayerPrefs.SetInt("RemoveAds", value ? 1 : 0);
    }
    public static bool RemoveAdsBundle
    {
        get => PlayerPrefs.GetInt("RemoveAdsBundle", 0) == 1;
        set => PlayerPrefs.SetInt("RemoveAdsBundle", value ? 1 : 0);
    }
    public static bool StarterPack
    {
        get => PlayerPrefs.GetInt("StarterPack", 0) == 1;
        set => PlayerPrefs.SetInt("StarterPack", value ? 1 : 0);
    }

    public static bool LimitedPack
    {
        get => PlayerPrefs.GetInt("LimitedPack", 0) == 1;
        set => PlayerPrefs.SetInt("LimitedPack", value ? 1 : 0);
    }

    public static bool ForestCallBundle
    {
        get => PlayerPrefs.GetInt("ForestCallBundle", 0) == 1;
        set => PlayerPrefs.SetInt("ForestCallBundle", value ? 1 : 0);
    }
    public static int CountShowRemoveAds
    {
        get => PlayerPrefs.GetInt("CountShowRemoveAds", 0);
        set => PlayerPrefs.SetInt("CountShowRemoveAds", value);
    }
    public static bool IsFirstShowRemoveAds
    {
        get => PlayerPrefs.GetInt("IsFirstShowRemoveAds", 0) == 1;
        set => PlayerPrefs.SetInt("IsFirstShowRemoveAds", value ? 1 : 0);
    }
    public static bool CanShowRemoveAds
    {
        get => PlayerPrefs.GetInt("CanShowRemoveAds", 0) == 1;
        set => PlayerPrefs.SetInt("CanShowRemoveAds", value ? 1 : 0);
    }

    public static int CountShowForestCall
    {
        get => PlayerPrefs.GetInt("CountShowForestCall", 0);
        set => PlayerPrefs.SetInt("CountShowForestCall", value);
    }
    public static bool UniverseBundle
    {
        get => PlayerPrefs.GetInt("UniverseBundle", 0) == 1;
        set => PlayerPrefs.SetInt("UniverseBundle", value ? 1 : 0);
    }
    public static bool GalaxyBundle
    {
        get => PlayerPrefs.GetInt("GalaxyBundle", 0) == 1;
        set => PlayerPrefs.SetInt("GalaxyBundle", value ? 1 : 0);
    }
    public static bool InfinityBundle
    {
        get => PlayerPrefs.GetInt("InfinityBundle", 0) == 1;
        set => PlayerPrefs.SetInt("InfinityBundle", value ? 1 : 0);
    }
    #endregion

    #region [ Booster ]

    public static int CountBoosterUndo
    {
        get => PlayerPrefs.GetInt("CountBoosterUndo", 1);
        set => PlayerPrefs.SetInt("CountBoosterUndo", value);
    }
    public static int CountBoosterShuffle
    {
        get => PlayerPrefs.GetInt("CountBoosterShuffle", 1);
        set => PlayerPrefs.SetInt("CountBoosterShuffle", value);
    }

    public static int CountBoosterMagnet
    {
        get => PlayerPrefs.GetInt("CountBoosterMagnet", 1);
        set => PlayerPrefs.SetInt("CountBoosterMagnet", value);
    }
    public static int CountBoosterExtraSlot
    {
        get => PlayerPrefs.GetInt("CountBoosterExtraSlot", 1);
        set => PlayerPrefs.SetInt("CountBoosterExtraSlot", value);
    }

    public static bool IsTutorialFirstLevelDone
    {
        get => PlayerPrefs.GetInt("IsTutorialFirstLevelDone", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("IsTutorialFirstLevelDone", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    /// <summary>Đã hiện tut "chim không khớp sẽ vào hàng chờ" (lần đầu có chim vào hàng chờ, mọi level).</summary>
    public static bool IsTutorialHoldingDone
    {
        get => PlayerPrefs.GetInt("IsTutorialHoldingDone", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("IsTutorialHoldingDone", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    /// <summary>Đã hiện tut "chỉ còn 1 slot ở hàng chờ" + mở nest miễn phí.</summary>
    public static bool IsTutorialOneSlotDone
    {
        get => PlayerPrefs.GetInt("IsTutorialOneSlotDone", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("IsTutorialOneSlotDone", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    public static bool IsTutorialExtraTime
    {
        get => PlayerPrefs.GetInt("IsTutorialExtraTime", 0) == 1;
        set => PlayerPrefs.SetInt("IsTutorialExtraTime", value ? 1 : 0);
    }
    public static bool IsTutorialMagicWand
    {
        get => PlayerPrefs.GetInt("IsTutorialMagicWand", 0) == 1;
        set => PlayerPrefs.SetInt("IsTutorialMagicWand", value ? 1 : 0);
    }

    public static int TimeX2Free
    {
        get => PlayerPrefs.GetInt("TimeX2Free", 0);
        set => PlayerPrefs.SetInt("TimeX2Free", value);
    }
    #endregion

    #region [ Gameplay ]
    public static bool IsFirstInitData
    {
        get => PlayerPrefs.GetInt("IsFirstInitData", 0) == 1;
        set => PlayerPrefs.SetInt("IsFirstInitData", value ? 1 : 0);
    }
    public static bool IsFirstOpenGame
    {
        get => PlayerPrefs.GetInt("IsFirstOpenGame", 0) == 1;
        set => PlayerPrefs.SetInt("IsFirstOpenGame", value ? 1 : 0);
    }
    public static float ValueDragMultiplier
    {
        get => PlayerPrefs.GetFloat("ValueDragMultiplier", 2);
        set => PlayerPrefs.SetFloat("ValueDragMultiplier", value);
    }
    public static bool IsDoneTutorial
    {
        get => PlayerPrefs.GetInt("IsDoneTutorial", 0) == 1;
        set => PlayerPrefs.SetInt("IsDoneTutorial", value ? 1 : 0);
    }
    public static bool IsFirstLogPlayLevel
    {
        get => PlayerPrefs.GetInt("IsFirstLogPlayLevel", 0) == 1;
        set => PlayerPrefs.SetInt("IsFirstLogPlayLevel", value ? 1 : 0);
    }
    public static int IndexLevel
    {
        get => PlayerPrefs.GetInt("IndexLevel", 0);
        set
        {
            PlayerPrefs.SetInt("IndexLevel", value);
            PlayerPrefs.Save();
        }
    }
    public static bool IsQuitGameWhenLose
    {
        get => PlayerPrefs.GetInt("IsQuitGameWhenLose", 0) == 1;
        set => PlayerPrefs.SetInt("IsQuitGameWhenLose", value ? 1 : 0);
    }
    public static bool IsProgressScore_1
    {
        get => PlayerPrefs.GetInt("IsProgressScore_1", 0) == 1;
        set => PlayerPrefs.SetInt("IsProgressScore_1", value ? 1 : 0);
    }
    public static bool IsProgressScore_2
    {
        get => PlayerPrefs.GetInt("IsProgressScore_2", 0) == 1;
        set => PlayerPrefs.SetInt("IsProgressScore_2", value ? 1 : 0);
    }
    public static int CountTime
    {
        get => PlayerPrefs.GetInt("CountTime", 0);
        set
        {
            PlayerPrefs.SetInt("CountTime", value);
            PlayerPrefs.Save();
        }
    }

    public static float TimePlaying
    {
        get => PlayerPrefs.GetFloat("TimePlaying", 0f);
        set
        {
            PlayerPrefs.SetFloat("TimePlaying", value);
        }
    }
    public static int ScoreOld
    {
        get => PlayerPrefs.GetInt("ScoreOld", 0);
        set
        {
            PlayerPrefs.SetInt("ScoreOld", value);
            PlayerPrefs.Save();
        }
    }

    public static int ScoreTarget
    {
        get => PlayerPrefs.GetInt("ScoreTarget", 25000);
        set
        {
            PlayerPrefs.SetInt("ScoreTarget", value);
            PlayerPrefs.Save();
        }
    }
    public static int HighScore
    {
        get => PlayerPrefs.GetInt("HighScore", 0);
        set
        {
            PlayerPrefs.SetInt("HighScore", value);
            PlayerPrefs.Save();
        }
    }
    public static int Score
    {
        get => PlayerPrefs.GetInt("Score", 0);
        set
        {
            PlayerPrefs.SetInt("Score", value);
            PlayerPrefs.Save();
        }
    }
    public static int CountWin
    {
        get => PlayerPrefs.GetInt("CountWin", 0);
        set
        {
            PlayerPrefs.SetInt("CountWin", value);
            PlayerPrefs.Save();
        }
    }
    public static int CountLose
    {
        get => PlayerPrefs.GetInt("CountLose", 0);
        set
        {
            PlayerPrefs.SetInt("CountLose", value);
            PlayerPrefs.Save();
        }
    }

    public static int CountWatchedReward
    {
        get => PlayerPrefs.GetInt("CountWatchedReward", 0);
        set
        {
            PlayerPrefs.SetInt("CountWatchedReward", value);
            PlayerPrefs.Save();
        }
    }

    public static int CountGoldenTile
    {
        get => PlayerPrefs.GetInt("CountGoldenTile", 0);
        set
        {
            PlayerPrefs.SetInt("CountGoldenTile", value);
            PlayerPrefs.Save();
        }
    }

    public static int CountWinStreak
    {
        get => PlayerPrefs.GetInt("CountWinStreak", 0);
        set
        {
            PlayerPrefs.SetInt("CountWinStreak", value);
            PlayerPrefs.Save();
        }
    }

    #endregion

    #region [ RATE ]
    public static int CountShowRate
    {
        get => PlayerPrefs.GetInt("CountShowRate", 0);
        set => PlayerPrefs.SetInt("CountShowRate", value);
    }
    public static bool IsRateDone
    {
        get => PlayerPrefs.GetInt("IsRateDone", 0) == 1;
        set => PlayerPrefs.SetInt("IsRateDone", value ? 1 : 0);
    }
    public static bool IsFirstShowRate
    {
        get => PlayerPrefs.GetInt("IsFirstShowRate", 0) == 1;
        set => PlayerPrefs.SetInt("IsFirstShowRate", value ? 1 : 0);
    }
    #endregion

    #region [ ADS ]
    public static bool IsCanShowInterAds
    {
        get => PlayerPrefs.GetInt("IsCanShowInterAds", 0) == 1;
        set => PlayerPrefs.SetInt("IsCanShowInterAds", value ? 1 : 0);
    }

    public static int CountWinInterAds
    {
        get => PlayerPrefs.GetInt("CountWinInterAds", 0);
        set => PlayerPrefs.SetInt("CountWinInterAds", value);
    }

    #endregion

    #region [ Timer ]
    public static int CountTimeThroughout
    {
        get => PlayerPrefs.GetInt("CountTimeThroughout", 0);
        set => PlayerPrefs.SetInt("CountTimeThroughout", value);
    }
    #endregion

    #region [ Avatar & Frame ]
    public static int CurrentAvatarId
    {
        get => PlayerPrefs.GetInt("CurrentAvatarId", 0);
        set => PlayerPrefs.SetInt("CurrentAvatarId", value);
    }
    public static int CurrentFrameId
    {
        get => PlayerPrefs.GetInt("CurrentFrameId", 0);
        set => PlayerPrefs.SetInt("CurrentFrameId", value);
    }
    #endregion

    public static int LoseStreakLevelId
    {
        get => PlayerPrefs.GetInt("LoseStreakLevelId", 0);
        set => PlayerPrefs.SetInt("LoseStreakLevelId", value);
    }
    public static int LoseStreakCount
    {
        get => PlayerPrefs.GetInt("LoseStreakCount", 0);
        set => PlayerPrefs.SetInt("LoseStreakCount", value);
    }
}
