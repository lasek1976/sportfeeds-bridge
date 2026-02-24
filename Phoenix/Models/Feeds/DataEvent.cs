using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataEvent
        : BaseDataTranslation
    {
        private Dictionary<string, IEnumerable<DataTranslation>> _tournamentNameTranslations =
            new Dictionary<string, IEnumerable<DataTranslation>>();

        private Dictionary<string, IEnumerable<DataTranslation>> _sportNameTranslations =
            new Dictionary<string, IEnumerable<DataTranslation>>();

        private Dictionary<string, IEnumerable<DataTranslation>> _categoryNameTranslations =
            new Dictionary<string, IEnumerable<DataTranslation>>();

        private Dictionary<string, IEnumerable<DataTranslation>> _eventNameTranslations =
            new Dictionary<string, IEnumerable<DataTranslation>>();

        [ProtoMember(1)] public int IDSport { get; set; }

        [ProtoMember(2)] public int IDCategory { get; set; }

        [ProtoMember(3)] public int IDTournament { get; set; }

        [ProtoMember(4)] public long IDEvent { get; set; }

        [ProtoMember(5)] public string EventName { get; set; }

        [ProtoMember(6)] public List<DataProviderDetails> ProviderDetails { get; set; } = [];

        [ProtoMember(10)] public DateTime EventDate { get; set; }

        [ProtoMember(11)] public DateTime StartDate { get; set; }

        [ProtoMember(12)] public DateTime EndDate { get; set; }

        [ProtoMember(13)] public string MPath { get; set; }

        [ProtoMember(17)] public List<DataScoreboard> ScoreBoards { get; set; } = [];

        [ProtoMember(18)] public List<DataMarket> Markets { get; set; } = [];

        [ProtoMember(19)] public List<DataTeam> Teams { get; set; } = [];

        [ProtoMember(24)] public TimeSpan MatchTime { get; set; }

        [ProtoMember(25)] public bool StopBetting { get; set; }

        [ProtoMember(26)] public byte IDEventType { get; set; }

        [ProtoMember(27)] public string TournamentName { get; set; }

        [ProtoMember(28)] public bool IsAntepost { get; set; }

        [ProtoMember(30)] public string SportName { get; set; }

        // Needed for filtering TopLeaguesOdds cache key events
        [ProtoMember(32)]
        public int TopLeagueEventRank { get; set; }

        // Needed for correct ordering excluding intermediate nodes on palinsest
        [ProtoMember(33)]
        public string TournamentMPath { get; set; }

        [ProtoMember(35)] public string CategoryName { get; set; }

        [ProtoMember(36)] public int SportOrder { get; set; }

        [ProtoMember(37)] public int MostPlacedRank { get; set; }

        [ProtoMember(38)] public bool HasResults { get; set; }

        [ProtoMember(39)] public int IDCalendar { get; set; }

        [ProtoMember(40)] public string AamsId { get; set; }

        [ProtoMember(41)] public int AamsIDSport { get; set; }

        [ProtoMember(42)] public bool IsSettlement { get; set; }

        [ProtoMember(43)] public string CategoryMPath { get; set; }

        [ProtoMember(44)] public int AamsIDTournament { get; set; }

        [ProtoMember(45)] public string AamsTournamentName { get; set; }

        [ProtoMember(47)] public DataStreaming Streaming { get; set; } = new();

        [ProtoMember(48)] public List<DataProposalSuperComboClient> PropSuperCombo { get; set; } = [];

        [ProtoMember(50)] public int MatchType { get; set; }

        [ProtoMember(51)] public int? StatisticValue { get; set; }

        // ID, types of market group included in current event, used for specific representation, etc...
        [ProtoMember(52)] public byte? EventMarketGroupType { get; set; }

        [ProtoMember(53)] public bool IsWizardBetBuilderEligible { get; set; }

        [ProtoMember(54)] public byte TournamentProgramStatus { get; set; }

        [ProtoMember(55)] public byte CategoryProgramStatus { get; set; }

        [ProtoMember(56)] public byte EventProgramStatus { get; set; }

        #region PROTOIGNORE

        [ProtoIgnore] public List<DataResult> Results { get; set; } = [];

        [ProtoIgnore]
        public Dictionary<string, IEnumerable<DataTranslation>> TournamentNameTranslations
        {
            get { return _tournamentNameTranslations; }
            set { _tournamentNameTranslations = value; }
        }

        [ProtoIgnore]
        public Dictionary<string, IEnumerable<DataTranslation>> CategoryNameTranslations
        {
            get { return _categoryNameTranslations; }
            set { _categoryNameTranslations = value; }
        }

        [ProtoIgnore]
        public Dictionary<string, IEnumerable<DataTranslation>> SportNameTranslations
        {
            get { return _sportNameTranslations; }
            set { _sportNameTranslations = value; }
        }

        #endregion PROTOIGNORE

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var compareObject = (obj as DataEvent);

            return compareObject.IDEvent == IDEvent;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)IDEvent;
            }
        }
    }
}
