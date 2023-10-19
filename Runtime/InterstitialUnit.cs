using System;
using GameKit.Ads.Units;
using YandexMobileAds;
using YandexMobileAds.Base;

namespace GameKit.YandexAds
{
    [Serializable]
    internal class InterstitialUnit : YandexUnit<Interstitial>, IInterstitialAdUnit
    {
        private InterstitialAdLoader interstitialAdLoader = new InterstitialAdLoader();
        private readonly AdRequestConfiguration _request;

        protected override void Initialize()
        {
            Instance.OnAdDismissed += OnAdClosed;
            Instance.OnAdFailedToShow += OnAdFailedToShow;
            Instance.OnAdShown += OnAdDisplayed;
            Instance.OnAdClicked += OnAdClicked;
        }

        public override void Release()
        {
            base.Release();
            if (Instance == null) return;
            Instance.OnAdDismissed += OnAdClosed;
            Instance.OnAdFailedToShow -= OnAdFailedToShow;
            Instance.OnAdShown -= OnAdDisplayed;
            Instance.OnAdClicked -= OnAdClicked;
            Instance.OnAdClicked -= OnAdClicked;
            Instance.Destroy();
            Instance = null;
        }

        public override bool Load()
        {
            if (Logger.IsDebugAllowed) Logger.Debug($"{Name} is loading");
            State = AdUnitState.Loading;
            interstitialAdLoader.LoadAd(_request);
            return true;
        }

        public override void Show()
        {
            if (Logger.IsDebugAllowed) Logger.Debug($"{Name} is showing");
            Instance.Show();
        }

        public InterstitialUnit(AdUnitConfig config) : base(config)
        {
            interstitialAdLoader.OnAdLoaded += OnInterstitialLoaded;
            interstitialAdLoader.OnAdFailedToLoad += OnInterstitialFailedToLoad;
            _request = new AdRequestConfiguration.Builder(config.unitKey).Build();
        }

        private void OnInterstitialLoaded(object sender, InterstitialAdLoadedEventArgs e)
        {
            Instance = e.Interstitial;
            OnAdLoaded(sender, e);
        }

        private void OnInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            OnAdFailedToLoad(sender, new AdFailureEventArgs(){Message = e.Message});
        }
    }
}