using System;
using System.Collections.Generic;
using DG.Tweening;
using quocbr.DesignPattern;
using UnityEngine;

public class IAPManager : Singleton<IAPManager>
{
    [SerializeField] private IAPConfig iapConfig;

    [Header("Fake store (chưa có SDK)")]
    [SerializeField] private float fakeDelay = 0.5f;
    [SerializeField] private bool fakeFail;

    public bool IsPurchasing { get; private set; }

    // Bắn sau khi đã cộng quà. ShopUI / MainUI nghe để tự refresh.
    public static event Action<IAPItem> OnPurchased;

    // CC_IAP lấy danh sách sản phẩm (id store, loại) từ đây.
    public IReadOnlyList<IAPItem> Items => iapConfig != null ? iapConfig.iapItems : null;

    public IAPItem GetIAPItemByProductId(IAP_Product iapProduct)
    {
        return iapConfig != null ? iapConfig.GetItem(iapProduct) : null;
    }

    /// <summary>Giá hiển thị: ưu tiên giá localize từ store, chưa có thì dùng giá trong IAPConfig.</summary>
    public string GetPrice(IAP_Product iapProduct)
    {
#if !UNITY_EDITOR
        string storePrice = CC_IAP.Ins.Localized(iapProduct);
        if (!string.IsNullOrEmpty(storePrice))
            return storePrice;
#endif
        IAPItem item = GetIAPItemByProductId(iapProduct);
        return item != null ? item.price : string.Empty;
    }

    public bool IsOwned(IAP_Product iapProduct)
    {
        return PlayerPrefs.GetInt("IAP_Owned_" + iapProduct, 0) == 1;
    }

    public void BuyIAP(IAP_Product iAP_Product, Action<bool> onResult)
    {
        if (IsPurchasing)
        {
            onResult?.Invoke(false);
            return;
        }

        IAPItem item = GetIAPItemByProductId(iAP_Product);
        if (item == null || (item.isNonConsumable && IsOwned(iAP_Product)))
        {
            onResult?.Invoke(false);
            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //EventManager.Trigger(new OnShowMessage { message = "No Internet Connection" });
            onResult?.Invoke(false);
            return;
        }

        IsPurchasing = true;
        UIManager.Ins.BlockInputUI(true);

        RequestStorePurchase(item, success =>
        {
            IsPurchasing = false;
            UIManager.Ins.BlockInputUI(false);

            if (success)
                HandlePurchase(item);

            onResult?.Invoke(success);
        });
    }

    // Mọi nhánh phải gọi callback đúng 1 lần.
    // Editor không có store thật nên dùng fake; build thật đi qua SDK (CC_Interface → CC_IAP).
    private void RequestStorePurchase(IAPItem item, Action<bool> callback)
    {
#if UNITY_EDITOR
        DOVirtual.DelayedCall(fakeDelay, () => callback?.Invoke(!fakeFail), ignoreTimeScale: true);
#else
        CC_Interface.Ins.BuyIAP(item.iAP_Product, callback);
#endif
    }

    private void HandlePurchase(IAPItem item)
    {
        //SoundManager.Ins.PlaySFX(SoundFX.Coin);

        // Mua trong Shop Ingame: dong shop truoc de toast qua hien tren man ingame,
        // coin bay ve CoinUI goc tren trai thay vi coin cua shop.
        //UIManager.Ins.CloseUI<ShopIngameUI>();

        // Man OutOfTime / OutOfSpace: van show toast nhung khong dien bay nhan thuong.
        //bool playFly = !UIManager.Ins.IsOpened<OutOfSpaceUI>() && !UIManager.Ins.IsOpened<OutOfTimeUI>();

        // Hold the old display before GrantItem broadcasts OnUpdateCoin.
        // CoinUI rewardCoinUI = playFly && item.coinAmount > 0 && ItemCollection.Ins != null
        //     ? ItemCollection.Ins.GetCoinUI()
        //     : null;
        // if (rewardCoinUI != null)
        //     rewardCoinUI.LockDisplay(VariableSystem.StoreCoin);

        GrantItem(item);
        LogEarnFromPack(item.ToRewards());

        // PurchaseSuccessfulUI successUI = UIManager.Ins.OpenUI<PurchaseSuccessfulUI>();
        // if (successUI != null)
        //     successUI.Init(item, rewardCoinUI, playFly);
        // else if (rewardCoinUI != null)
        // {
        //     rewardCoinUI.UnlockDisplay();
        //     rewardCoinUI.UpdateCoinText(VariableSystem.StoreCoin, 0f);
        // }
    }

    private void LogEarnFromPack(Rewards item, string source = "purchase")
    {
        var names = new List<string>();
        var amounts = new List<string>();

        if (item.coinAmount > 0)
        {
            names.Add("Coin");
            amounts.Add(item.coinAmount.ToString());
        }
        foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
        {
            int amount = item.GetBoosterAmount(type);
            if (amount <= 0) continue;
            names.Add(type.ToString());
            amounts.Add(amount.ToString());
        }
        if (names.Count == 0) return; // gói chỉ có RemoveAds

        string resourceType = names.Count > 1 ? "pack"
                            : names[0] == "Coin" ? "currency"
                            : "booster";
        //DataBucketEvent.Ins.EarnResource(resourceType, string.Join(",", names), string.Join(",", amounts), source);
    }

    /// <summary>
    /// Khôi phục gói mua 1 lần (RemoveAds...) khi cài lại game. Gọi từ SDK (CC_Interface.RestorePurchase).
    /// Gói đã sở hữu hoặc gói Consumable thì bỏ qua để không cộng quà 2 lần.
    /// </summary>
    public void RestorePurchase(IAP_Product iapProduct)
    {
        IAPItem item = GetIAPItemByProductId(iapProduct);
        if (item == null || !item.isNonConsumable || IsOwned(iapProduct))
            return;

        Debug.Log($"[IAP] Restore {iapProduct}");
        GrantItem(item);
    }

    /// <summary>
    /// Đơn mua treo từ phiên trước (app tắt giữa lúc mua) được store trả về khi mở lại game.
    /// Không có ShopItemUI nào chờ kết quả nên cộng quà + hiện popup trực tiếp.
    /// </summary>
    public void GrantPendingPurchase(IAP_Product iapProduct)
    {
        IAPItem item = GetIAPItemByProductId(iapProduct);
        if (item == null || (item.isNonConsumable && IsOwned(iapProduct)))
            return;

        Debug.Log($"[IAP] Grant pending purchase {iapProduct}");
        HandlePurchase(item);
    }

    /// <summary>
    /// Cộng phần thưởng (coin + booster) của một gói Rewards mà không đụng tới cờ mua hàng (RemoveAds, StarterPack...).
    /// Dùng cho quà ngoài shop như Journey. Chỉ cộng tài nguyên, animation bay do PurchaseSuccessfulUI xử lý khi mở popup nhận thưởng.
    /// </summary>
    public void GrantRewards(Rewards item, string source)
    {
        if (item.IsEmpty) return;
        BuyCoin(item.coinAmount);
        foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
        {
            BuyBooster(type, item.GetBoosterAmount(type));
        }
        BuyUnlimitedLife(item.unlimitedLifeMinutes);
        PlayerPrefs.Save();
        LogEarnFromPack(item, source);
    }

    // Cộng quà của gói. Dùng chung cho mua mới và restore để hai luồng luôn đồng bộ.
    private void GrantItem(IAPItem item)
    {
        if (item.isNonConsumable)
            PlayerPrefs.SetInt("IAP_Owned_" + item.iAP_Product, 1);

        if (item.removeAds || item.iAP_Product == IAP_Product.RemoveAds)
        {
            VariableSystem.RemoveAds = true;
            ActionHelper.HideBanner();
            if (!ActionHelper.IsEditor())
                ActionHelper.DestroyBanner();
        }

        if (item.iAP_Product == IAP_Product.StarterPack)
        {
            VariableSystem.StarterPack = true;
        }

        if (item.iAP_Product == IAP_Product.LimitedPack)
        {
            VariableSystem.LimitedPack = true;
        }
        BuyCoin(item.coinAmount);
        foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
        {
            BuyBooster(type, item.GetBoosterAmount(type));
        }
        BuyUnlimitedLife(item.unlimitedLifeMinutes);
        PlayerPrefs.Save();

        OnPurchased?.Invoke(item);
    }

    private void BuyCoin(int amount)
    {
        if (amount == 0) return;
        VariableSystem.StoreCoin += amount;
        //EventManager.Trigger(new OnUpdateCoin { coinAmount = VariableSystem.StoreCoin });
    }

    private void BuyBooster(BoosterType boosterType, int amount)
    {
        if (amount == 0) return;
        BoosterManager.Ins.HandleBuyBooster(boosterType, amount);
    }

    private void BuyUnlimitedLife(int minutes)
    {
        if (minutes == 0) return;
        Debug.LogWarning($"[IAP] Chưa có hệ thống mạng vô hạn, bỏ qua {minutes} phút.");
    }
}
