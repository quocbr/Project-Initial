using System;
using System.Collections.Generic;
using System.Linq;
using AdjustSdk;
using quocbr.DesignPattern;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

[System.Serializable]
public class IdProduct
{
    public string idProd;
    public IAP_Product idAndroid;
    public IAP_Product idIOS;
    public ProductType type;
    public string price;
}

public class CC_IAP : Singleton<CC_IAP>
{
    public IdProduct[] idProductIAP;
    private string tokenIAPAdjust = "3lv8n7";
    StoreController m_StoreController;
    private bool IsInitComplete = false;

    // protected void Awake()
    // {
    //     Debug.Log("Awake CC_IAP");
    //     InitializeIAP();
    // }

    public IdProduct GetIdProductByIAP(IAP_Product iapProduct)
    {
        return idProductIAP.FirstOrDefault(x => x.idAndroid == iapProduct || x.idIOS == iapProduct);
    }

    //public async Task Initialize(Action onSuccess, Action<string> onError)
    //{
    //    try
    //    {
    //        if (UnityServices.State == ServicesInitializationState.Initialized)
    //        {
    //            Debug.Log("UnityServices already initialized");
    //            onSuccess?.Invoke();
    //            return;
    //        }

    //        var options = new InitializationOptions().SetEnvironmentName(adjust.environment.ToString()); // Nếu có sử dụng cloud env

    //        await UnityServices.InitializeAsync(options);

    //        Debug.Log("UnityServices initialized successfully");
    //        onSuccess?.Invoke();
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"UnityServices Initialization Failed: {ex.Message}");
    //        onError?.Invoke(ex.Message);
    //    }
    //}
    public async void InitializeIAP()
    {
        if (IsInitComplete)
            return;

        Debug.Log("Initialize CC_IAP");
        m_StoreController = UnityIAPServices.StoreController();

        m_StoreController.OnProductsFetched += InitComplete;
        m_StoreController.OnPurchasePending += OnPurchasePendingHandler;
        m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmedHandler;
        m_StoreController.OnPurchaseFailed += OnPurchaseFailedHandler;
        m_StoreController.OnPurchasesFetched += OnExistingPurchasesFetched;
        m_StoreController.OnPurchasesFetchFailed += OnExistingPurchasesFetchFailed;
        m_StoreController.OnPurchaseDeferred += OnOrderDeferred;
        await m_StoreController.Connect();
        FetchProducts();
    }

    void InitComplete(List<Product> list)
    {
        LocalizedPrice();
        IsInitComplete = true;
        Debug.Log("InitComplete");
        //RestoreTransactions();
    }

    //void InitializeIAP()
    //{
    //    Debug.Log("Initialize CC_IAP");
    //    m_StoreController = UnityIAPServices.StoreController();

    //    m_StoreController.OnPurchasePending += OnPurchasePendingHandler;
    //    m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmedHandler;
    //    m_StoreController.OnPurchaseFailed += OnPurchaseFailedHandler;
    //    m_StoreController.OnPurchasesFetched += OnExistingPurchasesFetched;
    //    m_StoreController.OnPurchasesFetchFailed += OnExistingPurchasesFetchFailed;
    //    m_StoreController.OnPurchaseDeferred += OnOrderDeferred;
    //    m_StoreController.Connect();
    //    FetchProducts();
    //    if (ActionHelper.IsAndroid())
    //    {
    //        Debug.Log("Auto Restore after 2s open game CC_IAP");
    //        Invoke(nameof(RestoreTransactions), 2);
    //    }
    //}
    public void OnOrderDeferred(DeferredOrder deferredOrder)
    {
        foreach (var cartItem in deferredOrder.CartOrdered.Items())
        {
            var productId = deferredOrder.CartOrdered.Items().First().Product.definition.id;
            Debug.Log($"Purchase deferred for approval: {productId}");
        }
    }

    public void OnExistingPurchasesFetched(Orders existingOrders)
    {
        // restore here

        RestorePurchase(existingOrders);
    }

    public void OnExistingPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
    {
        Debug.LogError($"Failed to fetch previous purchases: {failure.Message}");
    }

    private void OnPurchasePendingHandler(PendingOrder order)
    {
        Debug.Log(
            $"[IAP] Purchase Pending: {order.CartOrdered.Items().First().Product.definition.id}"
        );
        m_StoreController.ConfirmPurchase(order);
    }

    private void OnPurchaseConfirmedHandler(Order order)
    {
        Debug.Log(
            $"[IAP] Purchase Confirmed: {order.CartOrdered.Items().First().Product.definition.id}"
        );

        // Gọi callback xử lý của bạn ở đây
        switch (order)
        {
            case FailedOrder failedOrder:
                Debug.Log(
                    $"Purchase confirmation failed: {failedOrder.CartOrdered.Items().First().Product.definition.id}, {failedOrder.FailureReason.ToString()}, {failedOrder.Details}"
                );
                CC_Interface.Ins.PurchaseCallback(false);
                break;
            case ConfirmedOrder:
                Debug.Log(
                    $"Purchase completed:  {order.CartOrdered.Items().First().Product.definition.id}"
                );

                PurchaseSuccessCallback(order);

                try
                {
                    LogIAP_Adjust(order);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error logging IAP to Adjust: {ex.Message}");
                }

                break;
        }
    }

    private void OnPurchaseFailedHandler(FailedOrder failedOrder)
    {
        var reason = failedOrder.FailureReason;
        Debug.Log(
            $"[IAP] Purchase Failed: {failedOrder.CartOrdered.Items().First().Product.definition.id} | Reason: {reason} "
        );

        // Gọi callback lỗi
        CC_Interface.Ins.PurchaseCallback(false);
    }

    void FetchProducts()
    {
        var initialProductsToFetch = new List<ProductDefinition>();
        foreach (var product in idProductIAP)
        {
            var prod = new ProductDefinition(product.idProd, product.type);
            initialProductsToFetch.Add(prod);
        }
        m_StoreController.FetchProducts(initialProductsToFetch);
    }

    public void BuyIAP(IAP_Product _idProductIAP)
    {
        if (m_StoreController == null)
        {
            Debug.Log("m_StoreController == null");
            InitializeIAP();
            CC_Interface.Ins.PurchaseCallback(false);
            return;
        }
        string currentPlatformId = null;
        for (int i = 0; i < idProductIAP.Length; i++)
        {
            if (ActionHelper.IsAndroid() && _idProductIAP == idProductIAP[i].idAndroid)
                currentPlatformId = idProductIAP[i].idProd;

            if (ActionHelper.IsIOS() && _idProductIAP == idProductIAP[i].idIOS)
                currentPlatformId = idProductIAP[i].idProd;
        }

        if (!string.IsNullOrEmpty(currentPlatformId))
        {
            var product = m_StoreController
                ?.GetProducts()
                .FirstOrDefault(p => p.definition.id == currentPlatformId);
            if (product != null)
                m_StoreController.PurchaseProduct(product);
            else
                Debug.Log($"Product ID not found: {currentPlatformId}");
        }
    }

    private UnityAction<bool> callbackRestore;

    public void RestoreTransactions(UnityAction<bool> callback = null)
    {
        this.callbackRestore = callback;
        Debug.Log("RestoreTransactions");
        m_StoreController.RestoreTransactions(OnTransactionsRestored);
    }

    void OnTransactionsRestored(bool success, string error)
    {
        Debug.Log("Transactions restored: " + success);

        if (success)
            m_StoreController.FetchPurchases();
        else
            callbackRestore?.Invoke(false);
    }

    #region Localize price
    public void LocalizedPrice()
    {
        if (m_StoreController != null)
        {
            for (int i = 0; i < idProductIAP.Length; i++)
                foreach (var product in m_StoreController.GetProducts())
                    if (product.definition.id == idProductIAP[i].idProd)
                        idProductIAP[i].price = product.metadata.localizedPriceString;
        }
    }

    public string Localized(IAP_Product idProduct)
    {
        string price = "hảo hảo chua cay";
        for (int i = 0; i < idProductIAP.Length; i++)
        {
            if (ActionHelper.IsAndroid())
                if (idProduct == idProductIAP[i].idAndroid)
                {
                    price = idProductIAP[i].price;
                    break;
                }
            if (ActionHelper.IsIOS())
                if (idProduct == idProductIAP[i].idIOS)
                {
                    price = idProductIAP[i].price;
                    break;
                }
        }
        return price;
    }
    #endregion
    public void PurchaseSuccessCallback(Order order)
    {
        string productId = order.CartOrdered.Items().First().Product.definition.id;
        double price = (double)order.CartOrdered.Items().First().Product.metadata.localizedPrice;

        string currency = order.CartOrdered.Items().First().Product.metadata.isoCurrencyCode;

        Debug.Log("PurchaseSuccessCallback = " + productId);
        for (int i = 0; i < idProductIAP.Length; i++)
        {
            if (productId.Equals(idProductIAP[i].idProd))
            {
                var id = ActionHelper.IsAndroid()
                    ? idProductIAP[i].idAndroid
                    : idProductIAP[i].idIOS;
                var idProd = ActionHelper.IsAndroid()
                    ? idProductIAP[i].idProd
                    : idProductIAP[i].idProd;
                Debug.Log("PurchaseSuccessCallback iap = " + id.ToString());

                if (!ActionHelper.IsEditor())
                {
                    //   AdjustTracking.TrackingEvent(TypeAds.IAP, TypeTracking.Rev, price, currency);
                    //    AdjustTracking.TrackingEvent(TypeAds.ServerIAP, TypeTracking.Rev, price, currency);
                    CC_LogFirebase.Ins.BuyIAP(id, VariableSystem.IndexLevel);
                }

                //DataBucketEvent.Ins.PurchaseIAP(productId, price, currency);
            }
        }

        CC_Interface.Ins.PurchaseCallback(true);
    }

    private void RestorePurchase(Orders existingOrders)
    {
        if (VariableSystem.IsRestoreDone)
            return;

        Debug.Log("Restore Purchase");
        Debug.Log("existingOrders.ConfirmedOrders = " + existingOrders.ConfirmedOrders);
        Debug.Log($"Confirmed: {existingOrders.ConfirmedOrders.Count()}");
        Debug.Log($"Pending: {existingOrders.PendingOrders.Count()}");
        Debug.Log($"Deferred: {existingOrders.DeferredOrders.Count()}");

        foreach (var order in existingOrders.ConfirmedOrders)
        {
            foreach (var item in order.CartOrdered.Items())
            {
                var receipt = order.Info.Receipt;
                var productId = item.Product.definition.id;
                var productType = item.Product.definition.type;

                // Debug.Log($"[Restore] ProductID: {productId} | Receipt: {(string.IsNullOrEmpty(receipt) ? "❌ NONE" : "✅ OK")}");

                for (int i = 0; i < idProductIAP.Length; i++)
                {
                    if (productId.Equals(idProductIAP[i].idProd))
                    {
                        var id = ActionHelper.IsAndroid()
                            ? idProductIAP[i].idAndroid
                            : idProductIAP[i].idIOS;
                        Debug.Log("restore iap = " + id.ToString());

                        if (idProductIAP[i].type == ProductType.NonConsumable)
                        {
                            Debug.Log("Du ma ma");
                            CC_Interface.Ins.RestorePurchase(id);
                        }
                        else
                            Debug.Log("Khong phải gói nonConsumable => ko restore");
                    }
                }
            }
        }
        callbackRestore?.Invoke(true);
    }

    #region VerifyIapAdjust
    public void LogIAP_Adjust(Order order)
    {
        string productId = order.CartOrdered.Items().First().Product.definition.id;
        string receipt = order.Info.Receipt; //Dùng receipt từ Info
        string transactionId = order.Info.TransactionID;
        double localizedPrice = 0f;
        string isoCurrencyCode = "USD";
        foreach (var product in m_StoreController.GetProducts())
            if (product.definition.id == productId)
            {
                localizedPrice = (double)product.metadata.localizedPrice;
                isoCurrencyCode = product.metadata.isoCurrencyCode;
            }

        AdjustEvent adjustEvent = new AdjustEvent(tokenIAPAdjust);
        adjustEvent.SetRevenue(localizedPrice, isoCurrencyCode);
        Debug.Log("price = " + localizedPrice);

        if (ActionHelper.IsAndroid())
        {
            Debug.Log("Parse Android");
            Dictionary<string, object> receiptWrapper =
                (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
            string payload = (string)receiptWrapper["Payload"];
            // Parse lớp 2
            Dictionary<string, object> payloadWrapper =
                (Dictionary<string, object>)MiniJson.JsonDecode(payload);
            string json = (string)payloadWrapper["json"];
            // Parse lớp 3
            Dictionary<string, object> jsonWrapper =
                (Dictionary<string, object>)MiniJson.JsonDecode(json);
            string purchaseToken = jsonWrapper["purchaseToken"].ToString();

            adjustEvent.ProductId = productId;
            adjustEvent.PurchaseToken = purchaseToken;
            Adjust.VerifyAndTrackPlayStorePurchase(
                adjustEvent,
                verificationResult =>
                {
                    Debug.Log("Verification status: " + verificationResult.VerificationStatus);
                    Debug.Log("Code: " + verificationResult.Code);
                    Debug.Log("Message: " + verificationResult.Message);
                }
            );
        }
        else
        {
            adjustEvent.TransactionId = transactionId;
            adjustEvent.ProductId = productId;
            Adjust.VerifyAndTrackAppStorePurchase(
                adjustEvent,
                verificationResult =>
                {
                    Debug.Log("Verification status: " + verificationResult.VerificationStatus);
                    Debug.Log("Code: " + verificationResult.Code);
                    Debug.Log("Message: " + verificationResult.Message);
                }
            );
        }
    }
    #endregion
}

[Serializable]
public class VerifyIAP
{
    public string platform,
        environment,
        receipt,
        device_id,
        adjustToken,
        packageName,
        product_id;

    public VerifyIAP(
        string m_platform,
        string m_environment,
        string m_receipt,
        string m_device_id,
        string package_name,
        string adjust_Token,
        string productid
    )
    {
        platform = m_platform;
        environment = m_environment;
        receipt = m_receipt;
        packageName = package_name;
        adjustToken = adjust_Token;
        product_id = productid;

        device_id = m_device_id;
    }
}
