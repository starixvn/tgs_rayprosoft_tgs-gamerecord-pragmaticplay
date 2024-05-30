using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.PragmaticPlay.Common;

public record PragmaticPlayGameRecordManualMessage
{
	[Required]
	[DataType(System.ComponentModel.DataAnnotations.DataType.Url)]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	public string SecureLogin { get; init; } = default!;
	[Required]
	public string SecretKey { get; init; } = default!;
	[Required]
	public string DataType { get; init; } = default!;
	[Required]
	[DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
	public DateTimeOffset StartTime { get; init; }
	[Required]
	[DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
	public DateTimeOffset EndTime { get; init; }
}
