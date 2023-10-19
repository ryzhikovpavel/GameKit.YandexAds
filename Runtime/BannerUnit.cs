using System;
using GameKit.Ads.Units;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

namespace GameKit.YandexAds
{
    [Serializable]
    internal class BannerUnit : YandexUnit<Banner>, ITopSmartBannerAdUnit, IBottomSmartBannerAdUnit
    {
        private readonly AdPosition _position;
        private readonly BannerAdSize _size;
        private readonly AdRequest _request;

        public event Action EventClicked;

        public BannerUnit(AdUnitConfig config, AdPosition position) : base(config)
        {
            _position = position;
            _size = BannerAdSize.StickySize(GetScreenWidthDp());
            _request = new AdRequest.Builder().Build();
        }

        protected override void Initialize()
        {
            Instance.OnAdLoaded += OnAdLoaded;
            Instance.OnAdLoaded += OnInternalLoaded;
            Instance.OnAdFailedToLoad += OnAdFailedToLoad;
            Instance.OnAdClicked += OnAdClicked;
        }

        protected override void OnAdClicked(object sender, EventArgs eventArgs)
        {
            base.OnAdClicked(sender, eventArgs);
            EventClicked?.Invoke();
        }

        public override void Release()
        {
            base.Release();
            if (Instance is null) return;
            Instance.OnAdLoaded -= OnAdLoaded;
            Instance.OnAdLoaded += OnInternalLoaded;
            Instance.OnAdFailedToLoad -= OnAdFailedToLoad;
            Instance.OnImpression -= OnImpression;
            Instance.OnAdClicked -= OnAdClicked;
            Instance.Destroy();
            Instance = null;
        }

        public override bool Load()
        {
            if (Logger.IsDebugAllowed) Logger.Debug($"{Name} is loading");
            Instance = new Banner(Key, _size, _position);
            State = AdUnitState.Loading;
            Instance.LoadAd(_request);
            Instance.Hide();
            return true;
        }

        public override void Show()
        {
            Instance.Show();
            base.Show();
        }

        public void Hide()
        {
            Instance?.Hide();
            OnAdClosed(Instance, null);
        }

        private int GetScreenWidthDp()
        {
            int screenWidth = (int)Screen.safeArea.width;
            return ScreenUtils.ConvertPixelsToDp(screenWidth);
        }

        private void OnInternalLoaded(object sender, EventArgs e)
        {
            Instance.Hide();
            Instance.OnImpression += OnImpression;
        }

        private void OnImpression(object sender, ImpressionData e)
        {
            OnAdDisplayed(Instance, null);
        }
    }
}