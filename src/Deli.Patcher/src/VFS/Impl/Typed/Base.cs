using System;
using System.IO;

namespace Deli.VFS
{
	public abstract class TypedFileHandle<TReader, TOut> : IFileHandle
	{
		private readonly IFileHandle _handle;

		private TOut? _cached;

		protected TReader Reader { get; }

		public string Path => _handle.Path;

		public string Name => _handle.Name;

		public IDirectoryHandle Directory => _handle.Directory;

		public event Action? Updated;

		public TypedFileHandle(IFileHandle handle, TReader reader)
		{
			_handle = handle;
			Reader = reader;

			SubscribeUpdate(this);
		}

		protected abstract TOut Read();

		public Stream OpenRead()
		{
			return _handle.OpenRead();
		}

		public TOut GetOrRead()
		{
			return _cached ??= Read();
		}

		// Subscribe using a weak reference. We want this to get GC'd before the handle is GC'd.
		// Use static to avoid accidental self references
		private static void SubscribeUpdate(TypedFileHandle<TReader, TOut> @this)
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

				((TypedFileHandle<TReader, TOut>) target.Target).OnUpdate();
			};

			source.Updated += handler;
		}

		private void OnUpdate()
		{
			// Invalidate cache
			_cached = default;
		}

		public override string ToString()
		{
			return $"<{typeof(TOut)}> {_handle}";
		}
	}
}
