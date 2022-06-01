using Tgs.GameProviders.Client;

namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public record GameRecordInfo(
    string GameProviderCode,
    Guid GameProviderId,
    string PlayerCode,
    GameRecordRoundStatus Status,
    GameRecordRoundClientStatus ClientStatus,
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
