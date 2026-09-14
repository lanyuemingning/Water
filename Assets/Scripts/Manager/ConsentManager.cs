using AnyThinkAds.Api;
using QFramework;
using UnityEngine;

/// <summary>
/// GDPR 同意管理器，通过 TopOn 内置的 UMP 接口弹出同意表单。
/// 非欧盟用户不会弹窗，自动放行个性化广告。
/// </summary>
public class ConsentManager : MonoSingleton<ConsentManager>
{
    /// <summary>用户是否允许个性化广告</summary>
    public bool IsAdsAllowed { get; private set; } = true;

    /// <summary>同意流程是否已完成</summary>
    public bool IsConsentReady { get; private set; }

    public override void OnSingletonInit()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        CheckUserLocation();
#else
        IsConsentReady = true;
#endif
    }

    /// <summary>
    /// 判断用户是否在欧盟地区，根据结果决定是否弹出同意表单。
    /// </summary>
    private void CheckUserLocation()
    {
        ATSDKAPI.getUserLocation(new ATGetUserLocationCallback(location =>
        {
            if (location == ATSDKAPI.kATUserLocationInEU)
            {
                Debug.Log("[ConsentManager] EU user, showing GDPR consent dialog");
                ShowConsentDialog();
            }
            else
            {
                // 非欧盟用户，不需要弹窗，允许个性化广告
                Debug.Log("[ConsentManager] Non-EU user, personalized ads allowed");
                IsAdsAllowed = true;
                IsConsentReady = true;
            }
        }));
    }

    /// <summary>
    /// 弹出 UMP 同意表单（由 TopOn SDK 内部调用 Google UMP SDK）。
    /// </summary>
    private void ShowConsentDialog()
    {
        ATSDKAPI.showGDPRConsentDialog(new ATConsentDismissCallback(info =>
        {
            Debug.Log($"[ConsentManager] Consent dialog dismissed: type={info.dismissType}, msg={info.infoMsg}");

            switch (info.dismissType)
            {
                case 0: // 用户同意个性化广告
                    IsAdsAllowed = true;
                    ATSDKAPI.setGDPRLevel(ATSDKAPI.PERSONALIZED);
                    break;
                case 1: // 用户拒绝，选择非个性化广告
                    IsAdsAllowed = false;
                    ATSDKAPI.setGDPRLevel(ATSDKAPI.NONPERSONALIZED);
                    break;
                default: // 关闭弹窗或其他情况，默认允许
                    IsAdsAllowed = true;
                    ATSDKAPI.setGDPRLevel(ATSDKAPI.PERSONALIZED);
                    break;
            }

            IsConsentReady = true;
        }));
    }

    /// <summary>
    /// 重新弹出同意表单（用于设置页面中用户主动修改隐私偏好）。
    /// </summary>
    public void ShowConsentDialogAgain()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        ShowConsentDialog();
#endif
    }

    #region 回调包装

    private class ATGetUserLocationCallback : ATGetUserLocationListener
    {
        private readonly System.Action<int> _callback;
        public ATGetUserLocationCallback(System.Action<int> callback) => _callback = callback;
        public void didGetUserLocation(int location) => _callback?.Invoke(location);
    }

    private class ATConsentDismissCallback : ATConsentDismissListener
    {
        private readonly System.Action<ATConsentDismissInfo> _callback;
        public ATConsentDismissCallback(System.Action<ATConsentDismissInfo> callback) => _callback = callback;
        public void onConsentDismiss(ATConsentDismissInfo info) => _callback?.Invoke(info);
    }

    #endregion
}
