using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public record GameRecordInfo(
    string GameProviderCode,
    Guid GameProviderId,
    string PlayerCode,
    RoundStatus Status,
    RoundClientStatus ClientStatus,
    string ExternalRoundId,
    string ExternalGameId,
    DateTimeOffset? ExternalCreatedOn,
    DateTimeOffset? ExternalClosedOn,
    decimal? BetAmount,
    decimal? ValidBetAmount,
    decimal? PlayerWinLoss,
    decimal? Commission,
    decimal? Turnover,
    decimal? SettleAmount,
    decimal? TieBetAdjustment,
    string AdditionalData);
