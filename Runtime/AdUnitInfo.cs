using GameKit.Ads;

namespace GameKit.YandexAds
{
    public struct AdUnitInfo: IAdInfo
    {
        public AdUnitInfo(string name, int floor)
        {
            Name = name;
            Floor = floor;
        }

        public string Name { get; }
        public int Floor { get; }
    }
}