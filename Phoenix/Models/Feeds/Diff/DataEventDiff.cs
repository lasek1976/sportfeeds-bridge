using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SportFeedsBridge.Phoenix.Domain.Enums;
using SportFeedsBridge.Phoenix.Models.Comparer;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds.Diff
{
    [DataContract(Name = "DE"), ProtoContract]
    public class DataEventDiff :
        BaseDiffObject, IDiffMapping<DataEvent, DataEventDiff>
    {
        [DataMember(Name = "m1"), ProtoMember(1)]
        public int IDSport { get; set; }

        [DataMember(Name = "m2"), ProtoMember(2)]
        public int IDCategory { get; set; }

        [DataMember(Name = "m3"), ProtoMember(3)]
        public int IDTournament { get; set; }

        [DataMember(Name = "m4"), ProtoMember(4)]
        public long IDEvent { get; set; }

        [DataMember(Name = "m5"), ProtoMember(5)]
        public DateTime EventDate { get; set; }

        [DataMember(Name = "m6"), ProtoMember(6)]
        public DateTime StartDate { get; set; }

        [DataMember(Name = "m7"), ProtoMember(7)]
        public DateTime EndDate { get; set; }

        [DataMember(Name = "m8"), ProtoMember(8)]
        public string EventName { get; set; }

        //[IgnoreDataMember, ProtoIgnore, JsonIgnore]
        [DataMember(Name = "m9"), ProtoMember(9)]
        public Dictionary<string, IEnumerable<DataTranslation>> EventNameTranslations { get; set; }

        [DataMember(Name = "m10"), ProtoMember(10)]
        public string MPath { get; set; }

        [DataMember(Name = "m11"), ProtoMember(11)]
        public List<DataProviderDetailsDiff> ProviderDetails { get; set; }

        [DataMember(Name = "m12"), ProtoMember(12)]
        public bool HasResults { get; set; }

        [DataMember(Name = "m14"), ProtoMember(14)]
        public List<DataMarketDiff> Markets { get; set; }

        [DataMember(Name = "m15"), ProtoMember(15)]
        public List<DataTeamDiff> Teams { get; set; }

        [DataMember(Name = "m16"), ProtoMember(16)]
        public List<DataScoreBoardDiff> ScoreBoards { get; set; }

        [DataMember(Name = "m25"), ProtoMember(25)]
        public TimeSpan MatchTime { get; set; }

        [DataMember(Name = "m26"), ProtoMember(26)]
        public bool StopBetting { get; set; }

        [DataMember(Name = "m27"), ProtoMember(27)]
        public byte IDEventType { get; set; }

        [DataMember(Name = "m28"), ProtoMember(28)]
        public string TournamentName { get; set; }

        [DataMember(Name = "m29"), ProtoMember(29)]
        public bool IsAntepost { get; set; }

        [DataMember(Name = "m30"), ProtoMember(30)]
        public Dictionary<string, IEnumerable<DataTranslation>> TournamentNameTranslations { get; set; }

        [DataMember(Name = "m31"), ProtoMember(31)]
        public string SportName { get; set; }

        [DataMember(Name = "m32"), ProtoMember(32)]
        public Dictionary<string, IEnumerable<DataTranslation>> SportNameTranslations { get; set; }

        [DataMember(Name = "m33"), ProtoMember(33)]
        public int TopLeagueEventRank { get; set; }

        [DataMember(Name = "m34"), ProtoMember(34)]
        public string TournamentMPath { get; set; } //neede for ordering of the live events overview cache key tournaments

        [DataMember(Name = "m35"), ProtoMember(35)]
        public Dictionary<string, IEnumerable<DataTranslation>> CategoryNameTranslations { get; set; }

        [DataMember(Name = "m36"), ProtoMember(36)]
        public string CategoryName { get; set; }

        [DataMember(Name = "m37"), ProtoMember(37)]
        public List<DataResultDiff> Results { get; set; }

        [DataMember(Name = "m38"), ProtoMember(38)]
        public int SportOrder
        {
            get;
            set;
        }

        [DataMember(Name = "m39"), ProtoMember(39)]
        public int MostPlacedRank
        {
            get;
            set;
        }

        [DataMember(Name = "m40"), ProtoMember(40)]
        public int IDCalendar
        {
            get;
            set;
        }

        [DataMember(Name = "m41"), ProtoMember(41)]
        public string AamsId
        {
            get;
            set;
        }

        [DataMember(Name = "m42"), ProtoMember(42)]
        public int AamsIDSport
        {
            get;
            set;
        }

        [DataMember(Name = "m43"), ProtoMember(43)]
        public bool IsSettlement { get; set; }

        [DataMember(Name = "m44"), ProtoMember(44)]
        public string CategoryMPath { get; set; }

        [DataMember(Name = "m45"), ProtoMember(45)]
        public int AamsIDTournament { get; set; }

        [DataMember(Name = "m46"), ProtoMember(46)]
        public string AamsTournamentName { get; set; }

        [DataMember(Name = "m47"), ProtoMember(47)]
        public DataStreamingDiff Streaming { get; set; }

        [DataMember(Name = "m48"), ProtoMember(48)]
        public DiffType SportDiffType { get; set; }

        [DataMember(Name = "m49"), ProtoMember(49)]
        public DiffType CategoryDiffType { get; set; }

        [DataMember(Name = "m50"), ProtoMember(50)]
        public DiffType TournamentDiffType { get; set; }

        [DataMember(Name = "m51"), ProtoMember(51)]
        public List<DataProposalSuperComboClientDiff> PropSuperCombo { get; set; }

        [DataMember(Name = "m52"), ProtoMember(52)]
        public int MatchType { get; set; }

        [DataMember(Name = "m53"), ProtoMember(53)]
        public byte EventMarketGroupType { get; set; } // ID, types of market group included in current event, used for specific representation, etc...

        [DataMember(Name = "m54"), ProtoMember(54)]
        public bool IsWizardBetBuilderEligible { get; set; }

        public DataEventDiff Instance()
        {
            return new DataEventDiff() { DiffType = DiffType.Equal };
        }

        public DataEventDiff Convert(DataEvent from, int deepness, DiffType convertStatus = DiffType.Added)
        {
            DataEventDiff into = Instance();
            if (deepness != 0)
            {
                into.IDSport = from.IDSport;
                into.IDCategory = from.IDCategory;
                into.IDTournament = from.IDTournament;
                into.IDEvent = from.IDEvent;
                into.EventDate = from.EventDate;
                into.StartDate = from.StartDate;
                into.EndDate = from.EndDate;
                into.EventName = from.EventName;
                into.TournamentName = from.TournamentName;
                into.CategoryName = from.CategoryName;
                into.SportName = from.SportName;
                into.MPath = from.MPath;
                into.HasResults = from.HasResults;
                into.MostPlacedRank = from.MostPlacedRank;
                into.EventNameTranslations = from.TranslationsDictionary;
                into.TournamentNameTranslations = from.TournamentNameTranslations;
                into.SportNameTranslations = from.SportNameTranslations;
                into.CategoryNameTranslations = from.CategoryNameTranslations;
                into.MatchTime = from.MatchTime;
                into.StopBetting = from.StopBetting;
                into.IDEventType = from.IDEventType;
                into.IsAntepost = from.IsAntepost;
                into.TournamentMPath = from.TournamentMPath;
                into.TopLeagueEventRank = from.TopLeagueEventRank;
                into.SportOrder = from.SportOrder;
                into.Markets = ConvertList<DataMarket, DataMarketDiff>(from.Markets, deepness, convertStatus);
                into.Teams = ConvertList<DataTeam, DataTeamDiff>(from.Teams, deepness, convertStatus);
                into.ScoreBoards = ConvertList<DataScoreboard, DataScoreBoardDiff>(from.ScoreBoards, deepness, convertStatus);
                into.Results = ConvertList<DataResult, DataResultDiff>(from.Results, deepness, convertStatus);
                into.ProviderDetails = ConvertList<DataProviderDetails, DataProviderDetailsDiff>(from.ProviderDetails, -1, convertStatus); //add it always
                into.DiffType = convertStatus;
                into.IDCalendar = from.IDCalendar;
                into.AamsId = from.AamsId;
                into.AamsIDSport = from.AamsIDSport;
                into.IsSettlement = from.IsSettlement;
                into.CategoryMPath = from.CategoryMPath;
                into.AamsIDTournament = from.AamsIDTournament;
                into.AamsTournamentName = from.AamsTournamentName;
                into.PropSuperCombo = ConvertList<DataProposalSuperComboClient, DataProposalSuperComboClientDiff>(from.PropSuperCombo, deepness, convertStatus);
                into.Streaming = new DataStreamingDiff().Convert(from.Streaming, deepness);
                into.MatchType = from.MatchType;
                into.EventMarketGroupType = from.EventMarketGroupType ?? 0;
                into.IsWizardBetBuilderEligible = from.IsWizardBetBuilderEligible;
            }

            return into;
        }

        [IgnoreDataMember, JsonIgnore]
        public IEnumerable<Expression<Func<DataEvent, object>>> CompareProperties
        {
            get
            {
                //yield return x => x.IDSport;
                yield return x => x.IDCategory;
                yield return x => x.IDTournament;
                //yield return x => x.IDEvent;
                yield return x => x.StartDate;
                yield return x => x.EventDate;
                yield return x => x.EndDate;
                yield return x => x.MPath;
                yield return x => x.EventName;
                yield return x => x.TournamentName;
                yield return x => x.SportName;
                //yield return x => x.MatchTime;
                yield return x => x.StopBetting;
                //yield return x => x.IsAntepost;
                yield return x => x.TournamentMPath;
                //yield return x => x.SportOrder;
                //yield return x => x.TopLeagueEventRank;   //since we're taking all events from topLeague key, no need to check if something has changed on their specific properties.

                //IDeventType ???
                //providerIDEvent/Category/Tournament/ecc. ???

                yield return y => y.ScoreBoards;
                yield return y => y.Teams;
                yield return y => y.Markets;
                yield return y => y.Results;
                yield return y => y.ProviderDetails;
                yield return y => y.IDCalendar;
                yield return y => y.PropSuperCombo;
                yield return y => y.EventMarketGroupType;
                yield return y => y.IsWizardBetBuilderEligible;
            }
        }

        protected override string TypeName
        {
            get { return "DE"; }
        }
    }
}
