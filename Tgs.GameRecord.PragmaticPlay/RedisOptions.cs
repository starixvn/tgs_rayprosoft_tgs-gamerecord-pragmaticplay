using Tgs.GameRecord.PragmaticPlay.Core;
namespace Tgs.GameRecord.PragmaticPlay;

public record RedisOptions
{
    public string ConnectionString { get; set; } = "localhost";
}
