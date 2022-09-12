using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameKit.Ads;
using GameKit.Ads.Networks;
using GameKit.Ads.Units;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

namespace GameKit.YandexAds
{
    [CreateAssetMenu(fileName = "YandexConfig", menuName = "GameKit/Ads/Yandex")]
    public class YandexNetwork: ScriptableObject, IAdsNetwork
    {
        internal static int PauseDelay;

        [SerializeField] 
        private bool autoRegister = true;
        
        [SerializeField, Range(1, 3)]
        [Tooltip("Required number of simultaneously loaded banners")]
        private int targetBannerLoaded = 2;

        [SerializeField, Range(2, 10)]
        [Tooltip("Delay between two request load ad banners")]
        private int delayBetweenRequest = 2;

        [SerializeField, Range(15, 300)]
        [Tooltip("Pause between two request load ad instance")]
        private int pauseAfterFailedRequest = 30;
        
        [SerializeField, Range(15, 300)]
        [Tooltip("Pause after click to banner")]
        private int pauseAfterBannerClicked = 30;
        
        [Header("Extras")]
        [SerializeField]
        private bool testMode;
        [SerializeField] 
        private bool enableInterstitial = true;
        [SerializeField] 
        private bool enableRewarded = true;
        [SerializeField] 
        private bool enableBannersTopPosition = false;
        [SerializeField]
        private bool enableBannersBottomPosition = true;

        [Header("Platforms")]
        [SerializeField]
        private PlatformConfig android;
        [SerializeField]
        private PlatformConfig iOS;

        private readonly Dictionary<Type, IAdUnit[]> _units = new Dictionary<Type,  IAdUnit[]>();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Registration()
        {
            Logger<YandexNetwork>.Info("Registered");
            var config = Resources.Load<YandexNetwork>("YandexConfig");
            if (config != null && config.autoRegister && Application.isEditor == false)
            {
                Service<AdsMediator>.Instance.RegisterNetwork(config);
                Logger<YandexNetwork>.Info("Registered");
            }
        }
        
        public TaskRoutine Initialize(bool trackingConsent, bool purchasedDisableUnits)
        {
            PauseDelay = pauseAfterFailedRequest;
            MobileAds.SetLocationConsent(false);
            MobileAds.SetUserConsent(trackingConsent);

#if UNITY_ANDROID
            PlatformConfig units = android;
#elif UNITY_IOS
            PlatformConfig units = ios;
#else
            PlatformConfig units = null;
#endif
            
            if (testMode)
            {
                InitializeTestUnits();
            }
            else
            {
                if (units is null) return TaskRoutine.FromCanceled();

                if (units.interstitialUnits.Length > 0 && enableInterstitial && purchasedDisableUnits == false)
                    _units.Add(typeof(IInterstitialAdUnit), InitializeUnits<InterstitialUnit>(units.interstitialUnits));

                if (units.bannerUnits.Length > 0 && purchasedDisableUnits == false)
                {
                    if (enableBannersTopPosition)
                        _units.Add(typeof(ITopSmartBannerAdUnit), InitializeUnits<BannerUnit>(units.bannerUnits, AdPosition.TopCenter));
                    if (enableBannersBottomPosition)
                        _units.Add(typeof(IBottomSmartBannerAdUnit), InitializeUnits<BannerUnit>(units.bannerUnits, AdPosition.BottomCenter));
                }
                
                if (units.rewardedUnits.Length > 0 && enableRewarded)
                    _units.Add(typeof(IRewardedVideoAdUnit), InitializeUnits<RewardedUnit>(units.rewardedUnits));
            }

            foreach (var banners in _units.Values)
            {
                Loop.StartCoroutine(DownloadHandler(new List<YandexUnit>(banners.Cast<YandexUnit>())));
            }
            
            if (_units.TryGetValue(typeof(ITopSmartBannerAdUnit), out var bannerUnits))
                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (BannerUnit u in bannerUnits) u.EventClicked += StartPauseAfterClick;
            if (_units.TryGetValue(typeof(IBottomSmartBannerAdUnit), out bannerUnits))
                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (BannerUnit u in bannerUnits) u.EventClicked += StartPauseAfterClick;
            
            return TaskRoutine.FromCompleted();
        }
        
        private void StartPauseAfterClick()
        {
            void AppendPause(IAdUnit[] units)
            {
                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (BannerUnit u in units)
                {
                    u.PauseUntilTime = DateTime.Now.AddSeconds(pauseAfterBannerClicked);
                    if (u.State is AdUnitState.Loaded or AdUnitState.Loading)
                        u.Release();
                }
            }
            
            if (_units.TryGetValue(typeof(ITopSmartBannerAdUnit), out var units)) AppendPause(units);
            if (_units.TryGetValue(typeof(IBottomSmartBannerAdUnit), out units)) AppendPause(units);
        }
        
        private void InitializeTestUnits()
        {
            string bannerAdUnit = "unexpected_platform";
            string interstitialAdUnit = "unexpected_platform";
            string rewardedAdUnit = "unexpected_platform";
            
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    bannerAdUnit = "R-M-DEMO-320x50";
                    interstitialAdUnit = "R-M-DEMO-interstitial";
                    rewardedAdUnit = "R-M-DEMO-rewarded-client-side-rtb";
                    break;
            }
            
            _units.Add(typeof(ITopSmartBannerAdUnit), new IAdUnit[]{
                new BannerUnit(new AdUnitConfig() { name = "Test Top Banner 1", unitKey = bannerAdUnit }, AdPosition.TopCenter),
                new BannerUnit(new AdUnitConfig() { name = "Test Top Banner 2", unitKey = bannerAdUnit }, AdPosition.TopCenter)
            });
            
            _units.Add(typeof(IBottomSmartBannerAdUnit), new IAdUnit[]{
                new BannerUnit(new AdUnitConfig() { name = "Test Bottom Banner 1", unitKey = bannerAdUnit }, AdPosition.BottomCenter),
                new BannerUnit(new AdUnitConfig() { name = "Test Bottom Banner 2", unitKey = bannerAdUnit }, AdPosition.BottomCenter)
            });
            
            _units.Add(typeof(IInterstitialAdUnit), new IAdUnit[]{new InterstitialUnit(new AdUnitConfig()
            {
                name = "Test Interstitial",
                unitKey = interstitialAdUnit
            })});
            
            _units.Add(typeof(IRewardedVideoAdUnit), new IAdUnit[]{new RewardedUnit(new AdUnitConfig()
            {
                name = "Test Rewarded",
                unitKey = rewardedAdUnit
            })});
        }
        
        private IAdUnit[] InitializeUnits<TUnit>(AdUnitConfig[] configs) where TUnit: YandexUnit
        {
            List<IAdUnit> units = new List<IAdUnit>();
            foreach (var config in configs)
            {
                if (string.IsNullOrEmpty(config.name)) config.name = typeof(TUnit).Name;
                var unit = (TUnit)Activator.CreateInstance(typeof(TUnit), config);
                units.Add(unit);
            }
            
            return units.ToArray();
        }
        
        private IAdUnit[] InitializeUnits<TUnit>(AdUnitConfig[] configs, AdPosition position) where TUnit: BannerUnit
        {
            List<IAdUnit> units = new List<IAdUnit>();
            foreach (var config in configs)
            {
                if (string.IsNullOrEmpty(config.name)) config.name = typeof(TUnit).Name;
                var unit = (TUnit)Activator.CreateInstance(typeof(TUnit), config, position);
                units.Add(unit);
            }
            
            return units.ToArray();
        }
        
        public void DisableUnits()
        {

        }

        public bool IsSupported(Type type) => _units.ContainsKey(type);
        public IAdUnit[] GetUnits(Type type) => _units[type];
        
        private AdRequest GetRequest()
        {
            return new AdRequest.Builder().Build();
        }
        
        private IEnumerator DownloadHandler(List<YandexUnit> units)
        {
            units.Sort((a,b)=>a.Config.priceFloor.CompareTo(b.Config.priceFloor));
            
            int attempt = 0;

            var request = GetRequest();
            var delay = new WaitForSecondsRealtime(delayBetweenRequest);

            var last = units.Last();
            last.Load(request);

            yield return null;

            while (Loop.IsQuitting == false)
            {
                var count = 0;
                foreach (var u in units)
                {
                    if (u.State == AdUnitState.Loaded) count++;
                    if (count >= targetBannerLoaded)
                    {
                        attempt++;
                        break;
                    }
                    
                    if (u.State == AdUnitState.Empty && attempt > u.Attempt && u.PauseUntilTime < DateTime.Now)
                    {
                        u.Load(request);
                        u.Attempt = attempt;
                        break;
                    }

                    if (last == u) attempt++;
                }

                yield return delay;
            }
        }
    }
}