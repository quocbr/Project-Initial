using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IAPConfig", menuName = "SO/IAPConfig")]
public class IAPConfig : ScriptableObject
{
    public List<IAPItem> iapItems;

    public IAPItem GetItem(IAP_Product product)
    {
        return iapItems.Find(item => item.iAP_Product == product);
    }
}

[Serializable]
public class IAPItem
{
    public string id;                 // id trên store, dùng khi gắn SDK
    public IAP_Product iAP_Product;
    public string title;
    public string price = "$0.99";    // giá hiển thị tạm khi chưa có store
    public bool isNonConsumable;      // mua 1 lần (RemoveAds, StarterPack...)
    public bool removeAds;            // gói có kèm gỡ quảng cáo

    public int coinAmount;
    public int clearAmount;           // BoosterType.Clear
    public int netAmount;             // BoosterType.Net (Magnet)
    public int shuffleAmount;         // BoosterType.Shuffle
    public int unlimitedLifeMinutes;  // phút

    public bool IsCoinPack =>
        coinAmount > 0 && clearAmount == 0 && netAmount == 0
        && shuffleAmount == 0 && unlimitedLifeMinutes == 0 && !removeAds;

    public int GetBoosterAmount(BoosterType type)
    {
        switch (type)
        {
            case BoosterType.Clear: return clearAmount;
            case BoosterType.Net: return netAmount;
            case BoosterType.Shuffle: return shuffleAmount;
            default: return 0;
        }
    }

    public Rewards ToRewards()
    {
        return new Rewards
        {
            coinAmount = coinAmount,
            clearAmount = clearAmount,
            netAmount = netAmount,
            shuffleAmount = shuffleAmount,
            unlimitedLifeMinutes = unlimitedLifeMinutes,
        };
    }
}
