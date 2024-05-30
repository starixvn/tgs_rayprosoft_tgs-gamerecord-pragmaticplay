using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tgs.GameRecord.PragmaticPlay.Core;
public interface IGameRecordScheduleLockRepository
{
	public Task<bool> LockingAsync(string secureLogin, string dataType, CancellationToken cancellationToken = default);

	public Task<bool> UnlockAsync(string secureLogin, string dataType, CancellationToken cancellationToken = default);
}
