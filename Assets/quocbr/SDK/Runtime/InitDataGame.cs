using System.Collections;
using System.Collections.Generic;
using quocbr.DesignPattern;
using UnityEngine;

public class InitDataGame : Singleton<InitDataGame>
{
    private bool isInit = false;
    public void InitData()
    {
        if (isInit) return;
        isInit = true;

        TimeManager.Ins.InitData();
        VariableSystem.IsCanShowInterAds = TimeManager.Ins.CheckDayShowInter();
        VariableSystem.CountWinInterAds = 0;

        if (VariableSystem.IsFirstOpenGame)
        {
            VariableSystem.IsFirstOpenGame = false;

        }

        if (!PlayerPrefs.HasKey("FirstOpenDate"))
        {
            PlayerPrefs.SetString("FirstOpenDate", System.DateTime.Now.ToString());
        }

        if (KeepObject.Instance.mode != TypeMode.Marketing)
        {
            if (!VariableSystem.IsFirstInitData)
            {
                VariableSystem.IsFirstInitData = true;
                VariableSystem.IsFirstOpenGame = true;
                PlayerPrefs.SetInt("ShowedNewLevel", 0);

                VariableSystem.StoreCoin = RemoteConfig.Ins.allConfigData.CoinStartGame;
                VariableSystem.IndexLevel = 0;
            }


        }

    }
}
