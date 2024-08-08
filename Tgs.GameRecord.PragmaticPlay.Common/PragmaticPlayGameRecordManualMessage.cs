using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.PragmaticPlay.Common;

public record PragmaticPlayGameRecordManualMessage
{
	[Required]
	[DataType(System.ComponentModel.DataAnnotations.DataType.Url)]
	[DefaultValue(typeof(Uri), "https://api.prerelease-env.biz/")]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "tgs_tgstfw")]
	public string SecureLogin { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "testKey")]
	public string SecretKey { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "RNG")]
	public string DataType { get; init; } = default!;
	[Required]
	[DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-04-29T15:00:00+08:00")]
	public DateTimeOffset StartTime { get; init; }
	[Required]
	[DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-04-29T16:00:08+00:00")]
	public DateTimeOffset EndTime { get; init; }
}
