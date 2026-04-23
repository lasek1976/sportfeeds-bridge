using SportFeedsBridge.Phoenix.Models.Feeds.Diff;
using Sportfeeds; // Generated protobuf namespace
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace SportFeedsBridge.Services;

/// <summary>
/// Converts Phoenix domain models to Google.Protobuf generated models
/// This replaces the need for manual "Clean" DTOs
/// </summary>
public static class ProtobufConverter
{
    /// <summary>
    /// Converts a DateTime to a Google.Protobuf.WellKnownTypes.Timestamp.
    /// Ensures the DateTime is treated as UTC.
    /// </summary>
    private static Timestamp ToTimestamp(DateTime dt)
    {
        return Timestamp.FromDateTime(DateTime.SpecifyKind(dt, DateTimeKind.Utc));
    }

    public static Sportfeeds.DataFeedsDiff ToProtobuf(Phoenix.Models.Feeds.Diff.DataFeedsDiff source)
    {
        var result = new Sportfeeds.DataFeedsDiff
        {
            CreatedUTCTime = ToTimestamp(source.CreatedUTCTime),
            DiffType = (Sportfeeds.DiffType)source.DiffType
        };

        // Convert events
        if (source.Events != null)
        {
            foreach (var evt in source.Events)
            {
                result.Events.Add(ToProtobuf(evt));
            }
        }

        // Convert difference properties
        if (source.DifferenceProperties != null)
        {
            foreach (var prop in source.DifferenceProperties)
            {
                result.DifferenceProperties.Add(ToProtobuf(prop));
            }
        }

        return result;
    }

    public static Sportfeeds.DataEventDiff ToProtobuf(Phoenix.Models.Feeds.Diff.DataEventDiff source)
    {
        var result = new Sportfeeds.DataEventDiff
        {
            IDSport = source.IDSport,
            IDCategory = source.IDCategory,
            IDTournament = source.IDTournament,
            IDEvent = source.IDEvent,
            EventDate = ToTimestamp(source.EventDate),
            StartDate = ToTimestamp(source.StartDate),
            EndDate = ToTimestamp(source.EndDate),
            EventName = source.EventName ?? string.Empty,
            MPath = source.MPath ?? string.Empty,
            HasResults = source.HasResults,
            MatchTime = source.MatchTime.Ticks,
            StopBetting = source.StopBetting,
            IDEventType = source.IDEventType,
            TournamentName = source.TournamentName ?? string.Empty,
            IsAntepost = source.IsAntepost,
            SportName = source.SportName ?? string.Empty,
            TopLeagueEventRank = source.TopLeagueEventRank,
            TournamentMPath = source.TournamentMPath ?? string.Empty,
            CategoryName = source.CategoryName ?? string.Empty,
            SportOrder = source.SportOrder,
            MostPlacedRank = source.MostPlacedRank,
            IDCalendar = source.IDCalendar,
            AamsId = source.AamsId ?? string.Empty,
            AamsIDSport = source.AamsIDSport,
            IsSettlement = source.IsSettlement,
            CategoryMPath = source.CategoryMPath ?? string.Empty,
            AamsIDTournament = source.AamsIDTournament,
            AamsTournamentName = source.AamsTournamentName ?? string.Empty,
            SportDiffType = (Sportfeeds.DiffType)source.SportDiffType,
            CategoryDiffType = (Sportfeeds.DiffType)source.CategoryDiffType,
            TournamentDiffType = (Sportfeeds.DiffType)source.TournamentDiffType,
            MatchType = source.MatchType,
            EventMarketGroupType = source.EventMarketGroupType,
            IsWizardBetBuilderEligible = source.IsWizardBetBuilderEligible,
            CreatedUTCTime = ToTimestamp(source.CreatedUTCTime),
            DiffType = (Sportfeeds.DiffType)source.DiffType
        };

        // Convert nested collections
        if (source.Markets != null)
        {
            foreach (var market in source.Markets)
            {
                result.Markets.Add(ToProtobuf(market));
            }
        }

        if (source.Teams != null)
        {
            foreach (var team in source.Teams)
            {
                result.Teams.Add(ToProtobuf(team));
            }
        }

        if (source.ScoreBoards != null)
        {
            foreach (var scoreBoard in source.ScoreBoards)
            {
                result.ScoreBoards.Add(ToProtobuf(scoreBoard));
            }
        }

        if (source.ProviderDetails != null)
        {
            foreach (var provider in source.ProviderDetails)
            {
                result.ProviderDetails.Add(ToProtobuf(provider));
            }
        }

        if (source.Results != null)
        {
            foreach (var resultItem in source.Results)
            {
                result.Results.Add(ToProtobuf(resultItem));
            }
        }

        if (source.Streaming != null)
        {
            result.Streaming = ToProtobuf(source.Streaming);
        }

        if (source.PropSuperCombo != null)
        {
            foreach (var prop in source.PropSuperCombo)
            {
                result.PropSuperCombo.Add(ToProtobuf(prop));
            }
        }

        // Convert translation dictionaries
        ConvertTranslationDictionary(source.EventNameTranslations, result.EventNameTranslations);
        ConvertTranslationDictionary(source.TournamentNameTranslations, result.TournamentNameTranslations);
        ConvertTranslationDictionary(source.SportNameTranslations, result.SportNameTranslations);
        ConvertTranslationDictionary(source.CategoryNameTranslations, result.CategoryNameTranslations);

        if (source.DifferenceProperties != null)
        {
            foreach (var prop in source.DifferenceProperties)
            {
                result.DifferenceProperties.Add(ToProtobuf(prop));
            }
        }

        return result;
    }

    public static Sportfeeds.DataMarketDiff ToProtobuf(Phoenix.Models.Feeds.Diff.DataMarketDiff source)
    {
        var result = new Sportfeeds.DataMarketDiff
        {
            IDEvent = source.IDEvent,
            IDMarket = source.IDMarket,
            IDMarketType = source.IDMarketType,
            Spread = (double)source.Spread,
            GamePlay = source.GamePlay,
            Outright = source.Outright,
            MarketWay = source.MarketWay,
            SpreadMarketType = source.SpreadMarketType,
            MarketOrder = source.MarketOrder,
            ProgramStatus = source.ProgramStatus,
            MarketBook = source.MarketBook,
            AamsIDMarketType = source.AamsIDMarketType,
            AamsMarketName = source.AamsMarketName ?? string.Empty,
            AamsExtraInfo = source.AamsExtraInfo ?? string.Empty,
            AamsExtraInfoType = source.AamsExtraInfoType,
            AamsGamePlay = source.AamsGamePlay,
            AamsLinkedMarketId = source.AamsLinkedMarketId.GetValueOrDefault(),
            NameOverride = source.NameOverride ?? string.Empty,
            StatisticValue = source.StatisticValue.GetValueOrDefault(),
            CreatedUTCTime = ToTimestamp(source.CreatedUTCTime),
            DiffType = (Sportfeeds.DiffType)source.DiffType
        };

        if (source.Selections != null)
        {
            foreach (var selection in source.Selections)
            {
                result.Selections.Add(ToProtobuf(selection));
            }
        }

        // Convert translation dictionaries
        ConvertTranslationDictionary(source.MarketNameTranslations, result.MarketNameTranslations);

        if (source.DifferenceProperties != null)
        {
            foreach (var prop in source.DifferenceProperties)
            {
                result.DifferenceProperties.Add(ToProtobuf(prop));
            }
        }

        return result;
    }

    public static Sportfeeds.DataSelectionDiff ToProtobuf(Phoenix.Models.Feeds.Diff.DataSelectionDiff source)
    {
        var result = new Sportfeeds.DataSelectionDiff
        {
            IDMarket = source.IDMarket,
            IDSelection = source.IDSelection,
            IDSelectionType = source.IDSelectionType,
            IDTeam = source.IDTeam,
            SelectionName = source.SelectionName ?? string.Empty,
            IDOdd = source.IDOdd,
            OddValue = (double)source.OddValue,
            OnlineCode = source.OnlineCode,
            SSpread = (double)source.SSpread.GetValueOrDefault(),
            ResultOverride = source.ResultOverride.GetValueOrDefault(),
            ProviderIdTeam = source.ProviderIdTeam ?? string.Empty,
            SelectionStatus = (Sportfeeds.ProgramStatus)source.SelectionStatus,
            CreatedUTCTime = ToTimestamp(source.CreatedUTCTime),
            DiffType = (Sportfeeds.DiffType)source.DiffType
        };

        // Convert translation dictionaries
        ConvertTranslationDictionary(source.SelectionNameTranslations, result.SelectionNameTranslations);

        if (source.DifferenceProperties != null)
        {
            foreach (var prop in source.DifferenceProperties)
            {
                result.DifferenceProperties.Add(ToProtobuf(prop));
            }
        }

        return result;
    }

    public static Sportfeeds.DataTeamDiff ToProtobuf(Phoenix.Models.Feeds.Diff.DataTeamDiff source)
    {
        var result = new Sportfeeds.DataTeamDiff
        {
            TeamId = source.TeamId,
            ProviderTeamId = source.ProviderTeamId ?? string.Empty,
            TeamName = source.TeamName ?? string.Empty,
            ProviderTeamName = source.ProviderTeamName ?? string.Empty,
            IdTeamNumber = source.IdTeamNumber,
            IsGroup = source.IsGroup,
            IDSogei = source.IDSogei.GetValueOrDefault(),
            FullTeamName = source.FullTeamName ?? string.Empty,
            CreatedUTCTime = ToTimestamp(source.CreatedUTCTime),
            DiffType = (Sportfeeds.DiffType)source.DiffType
        };

        // Convert translation dictionaries
        ConvertTranslationDictionary(source.TeamTranslationDictionary, result.TeamTranslationDictionary);

        if (source.DifferenceProperties != null)
        {
            foreach (var prop in source.DifferenceProperties)
            {
                result.DifferenceProperties.Add(ToProtobuf(prop));
            }
        }

        return result;
    }

    public static Sportfeeds.DataScoreBoardDiff ToProtobuf(Phoenix.Models.Feeds.Diff.DataScoreBoardDiff source)
    {
        var result = new Sportfeeds.DataScoreBoardDiff
        {
            IdResultType = source.IdResultType,
            ResultValue = source.ResultValue ?? string.Empty,
            CreatedUTCTime = ToTimestamp(source.CreatedUTCTime),
            DiffType = (Sportfeeds.DiffType)source.DiffType
        };

        if (source.DifferenceProperties != null)
        {
            foreach (var prop in source.DifferenceProperties)
            {
                result.DifferenceProperties.Add(ToProtobuf(prop));
            }
        }

        return result;
    }

    public static Sportfeeds.DataProviderDetailsDiff ToProtobuf(Phoenix.Models.Feeds.Diff.DataProviderDetailsDiff source)
    {
        var result = new Sportfeeds.DataProviderDetailsDiff
        {
            ProviderIDEvent = source.ProviderIDEvent ?? string.Empty,
            ProviderIDSport = source.ProviderIDSport,
            ProviderIDCategory = source.ProviderIDCategory ?? string.Empty,
            ProviderIDTournament = source.ProviderIDTournament ?? string.Empty,
            HasStatistics = source.HasStatistics,
            LiveMultiCast = source.LiveMultiCast,
            LiveScore = source.LiveScore,
            ProviderDate = ToTimestamp(source.ProviderDate),
            CreatedUTCTime = ToTimestamp(source.CreatedUTCTime),
            DiffType = (Sportfeeds.DiffType)source.DiffType
        };

        if (source.Provider != null)
        {
            result.Provider = new Sportfeeds.DataProvider
            {
                Code = source.Provider.Code ?? string.Empty,
                ProviderName = source.Provider.ProviderName ?? string.Empty
            };
        }

        if (source.RoundInfo != null)
        {
            result.RoundInfo = new Sportfeeds.DataRoundInfo
            {
                Id = source.RoundInfo.Id,
                CupRound = source.RoundInfo.CupRound ?? string.Empty,
                MatchNumber = source.RoundInfo.MatchNumber ?? string.Empty,
                Round = source.RoundInfo.Round,
                DateInfoValue = source.RoundInfo.DateInfoValue ?? string.Empty
            };
        }

        if (source.DifferenceProperties != null)
        {
            foreach (var prop in source.DifferenceProperties)
            {
                result.DifferenceProperties.Add(ToProtobuf(prop));
            }
        }

        return result;
    }

    public static Sportfeeds.DataResultDiff ToProtobuf(Phoenix.Models.Feeds.Diff.DataResultDiff source)
    {
        var result = new Sportfeeds.DataResultDiff
        {
            ResultId = source.ResultId,
            ResultTypeId = source.ResultTypeId,
            UpdatedTime = ToTimestamp(source.UpdatedTime),
            ResultValue = source.ResultValue,
            IsTeam = source.IsTeam,
            CreatedUTCTime = ToTimestamp(source.CreatedUTCTime),
            DiffType = (Sportfeeds.DiffType)source.DiffType
        };

        if (source.DifferenceProperties != null)
        {
            foreach (var prop in source.DifferenceProperties)
            {
                result.DifferenceProperties.Add(ToProtobuf(prop));
            }
        }

        return result;
    }

    public static Sportfeeds.DataStreamingDiff ToProtobuf(Phoenix.Models.Feeds.Diff.DataStreamingDiff source)
    {
        var result = new Sportfeeds.DataStreamingDiff
        {
            StreamProvider = source.StreamProvider ?? string.Empty,
            StreamID = source.StreamID ?? string.Empty,
            CreatedUTCTime = ToTimestamp(source.CreatedUTCTime),
            DiffType = (Sportfeeds.DiffType)source.DiffType
        };

        if (source.DifferenceProperties != null)
        {
            foreach (var prop in source.DifferenceProperties)
            {
                result.DifferenceProperties.Add(ToProtobuf(prop));
            }
        }

        return result;
    }

    public static Sportfeeds.DataProposalSuperComboClientDiff ToProtobuf(Phoenix.Models.Feeds.Diff.DataProposalSuperComboClientDiff source)
    {
        var result = new Sportfeeds.DataProposalSuperComboClientDiff
        {
            IdClient = source.IdClient,
            Description = source.Description ?? string.Empty,
            CreatedUTCTime = ToTimestamp(source.CreatedUTCTime),
            DiffType = (Sportfeeds.DiffType)source.DiffType
        };

        if (source.DifferenceProperties != null)
        {
            foreach (var prop in source.DifferenceProperties)
            {
                result.DifferenceProperties.Add(ToProtobuf(prop));
            }
        }

        return result;
    }

    public static Sportfeeds.DiffKeyValue ToProtobuf(SportFeedsBridge.Phoenix.Models.Comparer.DiffKeyValue source)
    {
        return new Sportfeeds.DiffKeyValue
        {
            Key = source.Key ?? string.Empty,
            Value = source.Value ?? string.Empty
        };
    }

    /// <summary>
    /// Convert Phoenix translation dictionary to protobuf map format
    /// Phoenix: Dictionary<string, IEnumerable<DataTranslation>>
    /// Protobuf: map<string, TranslationList>
    /// </summary>
    private static void ConvertTranslationDictionary(
        Dictionary<string, IEnumerable<Phoenix.Models.Feeds.DataTranslation>>? source,
        Google.Protobuf.Collections.MapField<string, Sportfeeds.TranslationList> destination)
    {
        if (source == null || destination == null)
            return;

        foreach (var kvp in source)
        {
            var translationList = new Sportfeeds.TranslationList();

            if (kvp.Value != null)
            {
                foreach (var translation in kvp.Value)
                {
                    translationList.Items.Add(new Sportfeeds.DataTranslation
                    {
                        Language = translation.Language ?? string.Empty,
                        Value = translation.Value ?? string.Empty,
                        ProviderId = translation.ProviderId ?? string.Empty,
                        Provider = translation.Provider != null
                            ? new Sportfeeds.DataProvider
                            {
                                Code = translation.Provider.Code ?? string.Empty,
                                ProviderName = translation.Provider.ProviderName ?? string.Empty
                            }
                            : null
                    });
                }
            }

            destination[kvp.Key] = translationList;
        }
    }
}
