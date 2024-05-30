using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tgs.GameRecord.PragmaticPlay.Core.Models;
public record GameRecordScheduleInfo(
	string SecureLogin,
	string DataType,
	DateTimeOffset NextTime,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt);
