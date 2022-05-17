namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public record PlayerInfo(
    Guid BrandId,
    string PlayerCode,
    string DefaultWalletCurrencyCode,
    Guid DefaultWalletId,
    Guid GameProviderConfigurationId);
