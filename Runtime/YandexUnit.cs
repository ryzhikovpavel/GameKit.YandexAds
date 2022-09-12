using System;
using GameKit.Ads;
using GameKit.Ads.Units;
using YandexMobileAds.Base;

namespace GameKit.YandexAds
{
    [Serializable]
    internal abstract class YandexUnit : IAdUnit
    {
        public readonly AdUnitConfig Config;
        public string Name => Config.name;
        public string Key => Config.unitKey;
        public int Attempt { get; set; }
        public abstract bool Load(AdRequest request);
        public DateTime PauseUntilTime;
        public DateTime BestBeforeDate;
        
        public AdUnitState State { get; set; }
        public string Error { get; protected set;}
        public IAdInfo Info { get; }

        public virtual void Show()
        {
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is Displayed");
            State = AdUnitState.Displayed;
        }

        public virtual void Release()
        {
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is Released");
            State = AdUnitState.Empty;
        }

        protected YandexUnit(AdUnitConfig config)
        {
            this.Config = config;
            Info = new AdUnitInfo(config.name, config.priceFloor);
        }
    }
    
    [Serializable]
    internal abstract class YandexUnit<T> : YandexUnit
    {
        public T Instance
        {
            get => _instance;
            set
            {
                _instance = value;
                if (_instance is null == false) Initialize();
            }
        }

        private T _instance;

        protected abstract void Initialize();
        
        protected virtual void OnAdClosed(object sender, EventArgs eventArgs)
        {
            State = AdUnitState.Closed;
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is closed");
        }

        protected virtual void OnAdLoaded(object sender, EventArgs eventArgs)
        {
            State = AdUnitState.Loaded;
            Attempt = -1;
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is loaded");
        }

        protected virtual void OnAdClicked(object sender, EventArgs eventArgs)
        {
            State = AdUnitState.Clicked;
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is clicked");
        }
        
        protected virtual void OnAdDisplayed(object sender, EventArgs eventArgs)
        {
            State = AdUnitState.Displayed;
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is Displayed");
        }
        
        protected virtual void OnAdFailedToLoad(object sender, AdFailureEventArgs e)
        {
            Error = e.Message;
            State = AdUnitState.Error;
            PauseUntilTime = DateTime.Now.AddSeconds(YandexNetwork.PauseDelay);
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} load failed");
        }
        
        protected virtual void OnAdFailedToShow(object sender, AdFailureEventArgs e)
        {
            Error = e.Message;
            State = AdUnitState.Error;
            if (Logger<YandexNetwork>.IsDebugAllowed) Logger<YandexNetwork>.Debug($"{Name} is show failed");
        }

        protected YandexUnit(AdUnitConfig config) : base(config) { }
    }
}