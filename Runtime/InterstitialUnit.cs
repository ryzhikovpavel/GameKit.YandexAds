using System;
using GameKit.Ads.Units;
using YandexMobileAds;
using YandexMobileAds.Base;

namespace GameKit.YandexAds
{
    [Serializable]
    internal class InterstitialUnit : YandexUnit<Interstitial>, IInterstitialAdUnit
    {
        protected override void Initialize()
        {
            Instance.OnInterstitialDismissed += OnAdClosed;
            Instance.OnInterstitialLoaded += OnAdLoaded;
            Instance.OnInterstitialFailedToLoad += OnAdFailedToLoad;
            Instance.OnInterstitialFailedToShow += OnAdFailedToShow;
            Instance.OnInterstitialShown += OnAdDisplayed;
            Instance.OnAdClicked += OnAdClicked;
        }
        
        public override void Release()
        {
            base.Release();
            if (Instance == null) return;
            Instance.OnInterstitialDismissed -= OnAdClosed;
            Instance.OnInterstitialLoaded -= OnAdLoaded;
            Instance.OnInterstitialFailedToLoad -= OnAdFailedToLoad;
            Instance.OnInterstitialFailedToShow -= OnAdFailedToShow;
            Instance.OnInterstitialShown -= OnAdDisplayed;
            Instance.OnAdClicked -= OnAdClicked;
            Instance.Destroy();
            Instance = null;
        }

        public override bool Load(AdRequest request)
        {
            Instance = new Interstitial(Key);
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is loading");
            State = AdUnitState.Loading;
            Instance.LoadAd(request);
            return true;
        }
        
        public override void Show()
        {
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is showing");
            Instance.Show();
        }

        public InterstitialUnit(AdUnitConfig config) : base(config) { }
    }
}