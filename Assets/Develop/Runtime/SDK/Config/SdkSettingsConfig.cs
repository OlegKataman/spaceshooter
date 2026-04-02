using UnityEngine;

namespace Develop.Runtime.SDK.Config
{
    [CreateAssetMenu(menuName = "SDK/Settings")]
    public sealed class SdkSettingsConfig : ScriptableObject
    {
        [field: SerializeField] public string AdjustAppToken { get; private set; } = "placeholder_adjust_token";
        [field: SerializeField] public string MetaAppId { get; private set; } = "placeholder_meta_app_id";
        [field: SerializeField] public string AdMobInterstitialId { get; private set; } = "ca-app-pub-3940256099942544/1033173712";
    }
}