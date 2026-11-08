using System;

/// <summary>
/// Gói phần thưởng dùng chung (coin, booster, mạng vô hạn). Số 0 nghĩa là không có loại đó.
/// Dùng cho Journey và có thể thay các field phần thưởng rời trong IAPItem.
/// </summary>
[Serializable]
public struct Rewards
{
    public int coinAmount;
    public int clearAmount;           // BoosterType.Clear
    public int netAmount;             // BoosterType.Net (Magnet)
    public int shuffleAmount;         // BoosterType.Shuffle
    public int unlimitedLifeMinutes;  // phút

    public readonly bool IsEmpty =>
        coinAmount <= 0 && clearAmount <= 0 && netAmount <= 0 && shuffleAmount <= 0 && unlimitedLifeMinutes <= 0;

    public readonly bool IsCoinOnly =>
        coinAmount > 0 && clearAmount <= 0 && netAmount <= 0 && shuffleAmount <= 0 && unlimitedLifeMinutes <= 0;

    public static Rewards operator +(Rewards a, Rewards b)
    {
        return new Rewards
        {
            coinAmount = a.coinAmount + b.coinAmount,
            clearAmount = a.clearAmount + b.clearAmount,
            netAmount = a.netAmount + b.netAmount,
            shuffleAmount = a.shuffleAmount + b.shuffleAmount,
            unlimitedLifeMinutes = a.unlimitedLifeMinutes + b.unlimitedLifeMinutes,
        };
    }

    public readonly int GetBoosterAmount(BoosterType type)
    {
        switch (type)
        {
            case BoosterType.Clear: return clearAmount;
            case BoosterType.Net: return netAmount;
            case BoosterType.Shuffle: return shuffleAmount;
            default: return 0;
        }
    }
}
