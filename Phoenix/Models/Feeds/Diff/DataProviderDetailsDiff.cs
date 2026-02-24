using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SportFeedsBridge.Phoenix.Domain.Enums;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds.Diff
{
    using SportFeedsBridge.Phoenix.Models.Comparer;

    [DataContract(Name = "DPD"), ProtoContract]
    public class DataProviderDetailsDiff
        : BaseDiffObject, IDiffMapping<DataProviderDetails, DataProviderDetailsDiff>
    {
        [DataMember(Name = "m1"), ProtoMember(1)]
        public DataProvider Provider { get; set; }

        [DataMember(Name = "m2"), ProtoMember(2)]
        public string ProviderIDEvent { get; set; }

        [DataMember(Name = "m3"), ProtoMember(3)]
        public int ProviderIDSport { get; set; }

        [DataMember(Name = "m4"), ProtoMember(4)]
        public string ProviderIDCategory { get; set; }

        [DataMember(Name = "m5"), ProtoMember(5)]
        public string ProviderIDTournament { get; set; }

        [DataMember(Name = "m7"), ProtoMember(7)]
        public bool HasStatistics { get; set; }

        [DataMember(Name = "m8"), ProtoMember(8)]
        public bool LiveMultiCast { get; set; }

        [DataMember(Name = "m9"), ProtoMember(9)]
        public bool LiveScore { get; set; }

        [DataMember(Name = "m10"), ProtoMember(10)]
        public DataRoundInfo RoundInfo { get; set; }

        [DataMember, ProtoMember(11)]
        public DateTime ProviderDate { get; set; }

        public DataProviderDetailsDiff Instance()
        {
            return new DataProviderDetailsDiff() { DiffType = DiffType.Equal };
        }

        public DataProviderDetailsDiff Convert(DataProviderDetails from, int deepness, DiffType convertStatus = DiffType.Added)
        {
            DataProviderDetailsDiff into = Instance();
            if (deepness != 0)
            {
                into.Provider = from.Provider;
                into.HasStatistics = from.HasStatistics;
                into.ProviderIDEvent = from.ProviderIDEvent;
                into.ProviderIDSport = from.ProviderIDSport;
                into.ProviderIDCategory = from.ProviderIDCategory;
                into.ProviderIDTournament = from.ProviderIDTournament;
                into.LiveMultiCast = from.LiveMultiCast;
                into.LiveScore = from.LiveScore;
                into.RoundInfo = from.RoundInfo;
                into.DiffType = convertStatus;
                into.ProviderDate = from.ProviderDate;
            }

            return into;
        }

        [IgnoreDataMember, JsonIgnore]
        public IEnumerable<Expression<Func<DataProviderDetails, object>>> CompareProperties
        {
            get
            {
                //yield return x => x.Provider;
                //yield return x => x.LiveMultiCast;
                //yield return x => x.LiveScore;
                yield return x => x.RoundInfo;
                yield return x => x.ProviderDate;
            }
        }

        protected override string TypeName
        {
            get { return "DPD"; }
        }
    }

    public static class DataProviderDetailsDiffExtensions
    {
		/// <summary>
		/// Returns DataProviderDetails for first match from preferred providers or default
		/// </summary>
		/// <param name="providersDetails">list of provider details</param>
		/// <param name="preferredProviders">Ordered list of preferred providers</param>
		/// <returns></returns>
		public static DataProviderDetailsDiff GetOrDefault(this IEnumerable<DataProviderDetailsDiff> providersDetails, params DataProvider[] preferredProviders)
        {
            if (preferredProviders == null || preferredProviders.Length == 0)
                return providersDetails.FirstOrDefault();

            DataProviderDetailsDiff result = null;
            int index = 0;

            while (result == null && index < preferredProviders.Length)
            {
                result = providersDetails.FirstOrDefault(x => x.Provider == preferredProviders[index]);
                index++;
            }

            if (result == null)
                return providersDetails.FirstOrDefault();

            return result;
        }

		/// <summary>
		/// Returns DataProviderDetails for first match from preferred providers or default
		/// </summary>
		/// <param name="providersDetails">list of provider details</param>
		/// <param name="preferredProviders">Ordered list of preferred providers</param>
		/// <returns></returns>
		public static DataProviderDetailsDiff Get(this IEnumerable<DataProviderDetailsDiff> providersDetails, params DataProvider[] preferredProviders)
        {
            if (preferredProviders == null || preferredProviders.Length == 0)
                return providersDetails.FirstOrDefault();

            DataProviderDetailsDiff result = null;
            int index = 0;

            while (result == null && index < preferredProviders.Length)
            {
                result = providersDetails.FirstOrDefault(x => x.Provider == preferredProviders[index]);
                index++;
            }
            return result;
        }
    }
}
