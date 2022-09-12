using System;

namespace GameKit.YandexAds
{
    [Serializable]
    internal class PlatformConfig
    {
        public AdUnitConfig[] interstitialUnits;
        public AdUnitConfig[] bannerUnits;
        public AdUnitConfig[] rewardedUnits;
    }
    
    [Serializable]
    internal class AdUnitConfig
    {
        public string name;
        public string unitKey;
        public int priceFloor;
    }
}