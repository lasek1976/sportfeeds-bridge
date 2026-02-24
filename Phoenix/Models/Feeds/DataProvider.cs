using System.Runtime.Serialization;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public struct DataProvider
        : IComparable, IComparable<DataProvider>
    {
        [ProtoMember(1)]
        public string Code
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public string ProviderName
        {
            get;
            set;
        }

        public DataProvider(string code, string providerName)
            : this()
        {
            Code = code;
            ProviderName = providerName;
        }

        #region Defined providers

        public static DataProvider Goldbet = new DataProvider(null, "Goldbet");
        public static DataProvider BetRadarFixed = new DataProvider("BRF", "BetRadarFixed");
        public static DataProvider BetRadarLive = new DataProvider("BRO", "BetRadarLive");
        public static DataProvider RunningBallLive = new DataProvider("RBL", "RunningBallLive");
        public static DataProvider SportingSolutionLive = new DataProvider("SSL", "SportingSolutionLive");
        public static DataProvider BetGenius = new DataProvider("BGN", "BetGenius");
        public static DataProvider BetRadarUnifiedOddsPrematch = new DataProvider("UOP", "BetRadarUnifiedOddsPrematch");
        public static DataProvider BetRadarUnifiedOddsLive = new DataProvider("UOL", "BetRadarUnifiedOddsLive");
        public static DataProvider InnBetsUnifiedOddsPrematch = new DataProvider("IBF", "InnBetsUnifiedOddsPrematch");
        public static DataProvider BIPOddsPrematch = new DataProvider("BIP", "BIPOddsPrematch");
        public static DataProvider BIPLive = new DataProvider("BIL", "BIPLive");
		private static DataProvider[] _dataProviders = new DataProvider[] {
            Goldbet,
            BetRadarFixed,
            BetRadarLive,
            SportingSolutionLive,
            RunningBallLive,
            BetGenius,
            BetRadarUnifiedOddsPrematch,
            BetRadarUnifiedOddsLive,
            InnBetsUnifiedOddsPrematch,
            BIPOddsPrematch,
            BIPLive
        };

        #endregion

        public int CompareTo(DataProvider other)
        {
            if (string.IsNullOrEmpty(this.Code) && string.IsNullOrEmpty(other.Code))
                return 0;

            if (string.IsNullOrEmpty(this.Code))
                return 1;

            return this.Code.CompareTo(other.Code);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (obj.GetType() != typeof(DataProvider))
                return 1;

            return CompareTo((DataProvider)obj);
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(Code) ? 0 : Code.GetHashCode();
        }

        public static bool operator ==(DataProvider a, DataProvider b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DataProvider a, DataProvider b)
        {
            return !a.Equals(b);
        }

        public static DataProvider FromCode(string code)
        {
            var provider = _dataProviders.FirstOrDefault(x => x.Code == code);
            return string.IsNullOrEmpty(provider.ProviderName) ? DataProvider.Goldbet : provider;
        }
    }
}
