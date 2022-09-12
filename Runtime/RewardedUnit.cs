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
        public override void Show()
        {
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is showing");
            Instance.Show();
        }

        protected override void Initialize()
        {
            Instance.OnRewardedAdDismissed += OnAdClosed;
            Instance.OnRewardedAdLoaded += OnAdLoaded;
            Instance.OnRewardedAdFailedToLoad += OnAdFailedToLoad;
            Instance.OnRewardedAdFailedToShow += OnAdFailedToShow;
            Instance.OnRewarded += OnEarnedReward;
            Instance.OnRewardedAdShown += OnAdDisplayed;
        }

        private void OnEarnedReward(object sender, Reward e)
        {
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is earned");
            IsEarned = true;
            if (Reward is null) Reward = new DefaultRewardAdInfo(e.amount, e.type);
        }

        public override void Release()
        {
            base.Release();
            if (Instance == null) return;
            Instance.OnRewardedAdDismissed -= OnAdClosed;
            Instance.OnRewardedAdLoaded -= OnAdLoaded;
            Instance.OnRewardedAdFailedToLoad -= OnAdFailedToLoad;
            Instance.OnRewardedAdFailedToShow -= OnAdFailedToShow;
            Instance.OnRewarded -= OnEarnedReward;
            Instance.OnRewardedAdShown -= OnAdDisplayed;
            Instance = null;
        }

        public override bool Load(AdRequest request)
        {
            Instance = new RewardedAd(Key);
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is loading");
            State = AdUnitState.Loading;
            Instance.LoadAd(request);
            return true;
        }

        public RewardedUnit(AdUnitConfig config) : base(config) { }
        public bool IsEarned { get; private set; }
        public IRewardAdInfo Reward { get; set; }
    }
}