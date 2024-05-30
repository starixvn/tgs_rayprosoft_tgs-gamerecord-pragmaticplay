using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tgs.GameRecord.PragmaticPlay.Repositories.Models;

[Table("game_record_schedules")]
public record GameRecordSchedule
{
	/// <summary>
	/// The identifier
	/// </summary>
	[BsonId]
	public ObjectId Id { get; set; } = default!;
	/// <summary>
	/// The secure login
	/// </summary>
	[BsonElement("secure_login")]
	public string SecureLogin { get; init; } = default!;
	/// <summary>
	/// The data type
	/// </summary>
	[BsonElement("data_type")]
	public string DataType { get; init; } = default!;
	/// <summary>
	/// The next time
	/// </summary>
	[BsonElement("next_time")]
	public DateTime NextTime { get; init; } = default!;
	/// <summary>
	/// The created at
	/// </summary>
	[BsonElement("created_at")]
	public DateTime CreatedAt { get; init; } = default!;
	/// <summary>
	/// The updated at
	/// </summary>
	[BsonElement("updated_at")]
	public DateTime UpdatedAt { get; init; } = default!;
}
