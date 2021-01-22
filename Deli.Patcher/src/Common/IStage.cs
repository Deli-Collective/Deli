using System;
using Deli.Patcher;

namespace Deli
{
	public interface IStage
	{
		ImmediateReaderCollection ImmediateReaders { get; }

		event Action? Started;
		event Action? Finished;
	}
}
