using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.PragmaticPlay.Common;

public record PragmaticPlayGameRecordScheduleMessage
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
}
