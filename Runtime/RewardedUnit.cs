using System;
using GameKit.Ads;
using GameKit.Ads.Units;
using YandexMobileAds;
using YandexMobileAds.Base;

namespace GameKit.YandexAds
{
    [Serializable]
    internal class RewardedUnit : YandexUnit<RewardedAd>, IRewardedVideoAdUnit
    {
        private readonly RewardedAdLoader _loader = new RewardedAdLoader();
        private readonly AdRequestConfiguration _request;

        public override void Show()
        {
            if (Logger.IsDebugAllowed) Logger.Debug($"{Name} is showing");
            Instance.Show();
        }

        protected override void Initialize()
        {
            Instance.OnAdDismissed += OnAdClosed;
            Instance.OnAdFailedToShow += OnAdFailedToShow;
            Instance.OnRewarded += OnEarnedReward;
            Instance.OnAdShown += OnAdDisplayed;
        }

        private void OnEarnedReward(object sender, Reward e)
        {
            if (Logger.IsDebugAllowed) Logger.Debug($"{Name} is earned");
            IsEarned = true;
            if (Reward is null) Reward = new DefaultRewardAdInfo(e.amount, e.type);
        }

        public override void Release()
        {
            base.Release();
            if (Instance == null) return;
            Instance.OnAdDismissed -= OnAdClosed;
            Instance.OnAdFailedToShow -= OnAdFailedToShow;
            Instance.OnRewarded -= OnEarnedReward;
            Instance.OnAdShown -= OnAdDisplayed;
            Instance = null;
        }

        public override bool Load()
        {
            if (Logger.IsDebugAllowed) Logger.Debug($"{Name} is loading");
            State = AdUnitState.Loading;
            _loader.LoadAd(_request);
            return true;
        }

        public bool IsEarned { get; private set; }
        public IRewardAdInfo Reward { get; set; }

        public RewardedUnit(AdUnitConfig config) : base(config)
        {
            _request = new AdRequestConfiguration.Builder(config.unitKey).Build();
            _loader.OnAdLoaded += OnRewardedLoaded;
            _loader.OnAdFailedToLoad += OnRewardedFailedToLoad;
        }

        private void OnRewardedFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            if (e.AdUnitId != Config.unitKey) return;
            OnAdFailedToLoad(sender, new AdFailureEventArgs(){Message = e.Message});
        }

        private void OnRewardedLoaded(object sender, RewardedAdLoadedEventArgs e)
        {
            if (e.RewardedAd?.GetInfo()?.AdUnitId != Config.unitKey) return;
            Instance = e.RewardedAd;
            OnAdLoaded(sender, e);
        }
    }
}