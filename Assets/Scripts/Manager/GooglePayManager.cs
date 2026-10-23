using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public class GooglePayManager : MonoSingleton<GooglePayManager>
{
    static readonly string[] ProductIds =
    {
        "battlepass_vip",
        "gift_1",
        "gift_2",
        "gift_3",
        "gift_4",
        "gift_5",
        "gift_6",
        "gold_1",
        "gold_2",
        "gold_3",
        "gold_4",
        "gold_5",
        "gold_6",
        "pg_gift1",
        "pg_gift2",
        "pg_gift3",
        "pg_gift4",
        "pg_gift5",
        "pg_gift6",
        "pg_gift7",
        "pg_gift8",
        "pg_gift9",
        "privilegepack_1",
        "privilegepack_2",
        "privilegepack_3",
        "privilegepack_4",
        "privilegepack_5",
        "privilegepack_6",
        "removeadpack_1",
        "sepecialoffer_1",
        "dgpack_vip",
    };

    StoreController m_StoreController;
    bool m_Initialized;
    string m_InitError;

    public override void OnSingletonInit()
    {
        InitializePurchasing();
    }

    async void InitializePurchasing()
    {
        Debug.Log("[GooglePay] InitializePurchasing");
        m_StoreController = UnityIAPServices.StoreController();
        m_StoreController.OnStoreDisconnected += OnStoreDisconnected;
        m_StoreController.OnProductsFetched += OnProductsFetched;
        m_StoreController.OnProductsFetchFailed += OnProductsFetchFailed;
        m_StoreController.OnPurchasePending += OnPurchasePending;
        m_StoreController.OnPurchaseFailed += OnPurchaseFailed;

        try
        {
            await m_StoreController.Connect();
        }
        catch (Exception e)
        {
            m_InitError = e.Message;
            Debug.LogWarning($"[GooglePay] Connect threw: {e.Message}");
            return;
        }

        if (this == null)
        {
            return;
        }

        var products = new List<ProductDefinition>(ProductIds.Length);
        foreach (var id in ProductIds)
        {
            products.Add(new ProductDefinition(id, ProductType.Consumable));
        }

        m_StoreController.FetchProducts(products);
    }

    void OnStoreDisconnected(StoreConnectionFailureDescription description)
    {
        m_InitError = description.Message;
        Debug.LogWarning($"[GooglePay] Store disconnected: {description.Message}");
    }

    void OnProductsFetched(List<Product> products)
    {
        m_Initialized = true;
        m_InitError = null;
        Debug.Log($"[GooglePay] In-App Purchasing successfully initialized, products: {products?.Count ?? 0}");
        m_StoreController.FetchPurchases();
    }

    void OnProductsFetchFailed(ProductFetchFailed failed)
    {
        m_Initialized = false;
        m_InitError = failed.FailureReason;
        Debug.LogWarning($"[GooglePay] Products fetch failed: {failed.FailureReason}");
    }

    public void BuyProduct(string pruductid)
    {
        if (!m_Initialized || m_StoreController == null)
        {
            Debug.LogWarning($"[GooglePay] Store not initialized, cannot buy: {pruductid}. {m_InitError}");
            return;
        }

        var product = m_StoreController.GetProductById(pruductid);
        if (product == null)
        {
            Debug.LogWarning($"[GooglePay] Product not found: {pruductid}");
            return;
        }

        m_StoreController.PurchaseProduct(product);
    }

    void OnPurchasePending(PendingOrder order)
    {
        var product = order.CartOrdered.Items().FirstOrDefault()?.Product;
        if (product == null)
        {
            Debug.LogWarning("[GooglePay] Pending order has no product");
            return;
        }

        var productId = product.definition.id;
        StringEventSystem.Global.Send(productId);
        StringEventSystem.Global.Send(GameDefine.GameConst.GIFT_PACK_ENTRY_STATE_CHANGED);

        TenjinManager.Instance.ReportIAPToTenjin(
            productId,
            product.metadata.isoCurrencyCode,
            product.metadata.localizedPrice,
            order.Info.Receipt);

        m_StoreController.ConfirmPurchase(order);
    }

    void OnPurchaseFailed(FailedOrder failedOrder)
    {
        var productId = failedOrder.CartOrdered.Items().FirstOrDefault()?.Product.definition.id;
        Debug.Log($"Purchase failed - Product: '{productId}'," +
            $" Purchase failure reason: {failedOrder.FailureReason}," +
            $" Purchase failure details: {failedOrder.Details}");
    }
}
