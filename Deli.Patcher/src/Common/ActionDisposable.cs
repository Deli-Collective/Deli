using System;

namespace Deli
{
	public class ActionDisposable : IDisposable
	{
		private readonly Action _callback;

		private bool _disposed;

		public ActionDisposable(Action callback)
		{
			_callback = callback;
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_callback();

			_disposed = true;
		}
	}
}
