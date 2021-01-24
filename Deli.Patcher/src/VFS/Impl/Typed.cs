using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Deli.Patcher;

namespace Deli.VFS
{
	public class ImmediateTypedFileHandle<T> : IFileHandle
	{
		private readonly IFileHandle _handle;
		private readonly IImmediateReader<T> _reader;

		private bool _read;
		[AllowNull]
		[MaybeNull]
		private T _cached;

		public string Name => _handle.Name;

		public IDirectoryHandle Directory => _handle.Directory;

		public event Action? Updated;

		public ImmediateTypedFileHandle(IFileHandle handle, IImmediateReader<T> reader)
		{
			_handle = handle;
			_reader = reader;

			SubscribeUpdate(this);
		}

		public Stream OpenRead()
		{
			return _handle.OpenRead();
		}

		public T GetOrRead()
		{
			if (!_read)
			{
				_cached = _reader.Read(this);
				_read = true;
			}

			// We just read the value into '_cached'; ignore nullability warning.
			return _cached!;
		}

		// Subscribe using a weak reference. We want this to get GC'd before the handle is GC'd.
		// Use static to avoid accidental self references
		private static void SubscribeUpdate(ImmediateTypedFileHandle<T> @this)
		{
			var source = @this._handle;
			var target = new WeakReference(@this);

			Action? handler = null;
			handler = () =>
			{
				if (!target.IsAlive)
				{
					source.Updated -= handler;
					return;
				}

				((ImmediateTypedFileHandle<T>) target.Target).OnUpdate();
			};

			source.Updated += handler;
		}

		private void OnUpdate()
		{
			// Invalidate cache
			_read = false;
			_cached = default;
		}

		public override string ToString()
		{
			return $"<{typeof(T)}> {_handle}";
		}
	}
}
