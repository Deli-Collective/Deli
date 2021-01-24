using System;

namespace Deli
{
	/// <summary>
	///		A disposable that calls an <see cref="Action"/> on disposal.
	/// </summary>
	public sealed class ActionDisposable : IDisposable
	{
		private readonly Action _callback;

		private bool _disposed;

		/// <summary>
		///		Creates an instance of <see cref="ActionDisposable"/>
		/// </summary>
		/// <param name="callback"></param>
		public ActionDisposable(Action callback)
		{
			_callback = callback;
		}

		/// <inheritdoc cref="IDisposable.Dispose"/>
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
