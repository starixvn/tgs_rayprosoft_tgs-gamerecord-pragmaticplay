namespace Tgs.GameRecord.PragmaticPlay;

public record MongoDBOptions
{
	public string ConnectionString { get; set; } = "mongodb://localhost";
}
