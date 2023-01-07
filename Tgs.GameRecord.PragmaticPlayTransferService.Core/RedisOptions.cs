using Tgs.GameRecord.PragmaticPlayTransferService;
namespace Tgs.GameRecord.PragmaticPlayTransferService.Core;

public record RedisOptions
{
    public string ConnectionString { get; set; } = "localhost";
}
