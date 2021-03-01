using System;
using System.IO;

namespace Deli.VFS
{
	/// <summary>
	///		A file handle which supports deserialization to a generic type
	/// </summary>
	/// <typeparam name="TReader">The reader to deserialize the raw file to <typeparamref name="TOut"/></typeparam>
	/// <typeparam name="TOut">The type to deserialize to</typeparam>
	public abstract class TypedFileHandle<TReader, TOut> : IFileHandle, IDisposable
	{
		private readonly IFileHandle _handle;

		private TOut? _cached;

		/// <summary>
		///		The reader responsible for deserializing the underlying file
		/// </summary>
		protected TReader Reader { get; }

		/// <inheritdoc cref="IHandle.IsAlive"/>
		public bool IsAlive { get; private set; }

		/// <inheritdoc cref="IHandle.Path"/>
		public string Path => _handle.Path;

		/// <inheritdoc cref="IChildHandle.Directory"/>
		public IDirectoryHandle Directory => _handle.Directory;

		/// <inheritdoc cref="IChildHandle.Name"/>
		public string Name => _handle.Name;

		/// <inheritdoc cref="IHandle.Updated"/>
		public event Action? Updated;

		/// <inheritdoc cref="IHandle.Deleted"/>
		public event Action? Deleted;

		/// <summary>
		///		Creates an instance of <see cref="TypedFileHandle{TReader,TOut}"/>
		/// </summary>
		/// <param name="handle">The raw handle to deserialize</param>
		/// <param name="reader">The reader responsible for deserialization</param>
		public TypedFileHandle(IFileHandle handle, TReader reader)
		{
			_handle = handle;

			Reader = reader;
			IsAlive = handle.IsAlive;

			if (IsAlive)
			{
				handle.Updated += OnUpdate;
				handle.Deleted += OnDelete;
			}
		}

		private void OnUpdate()
		{
			// Invalidate cache
			_cached = default;
		}

		private void OnDelete()
		{
			Dispose();
		}

		/// <summary>
		///		Deserializes this file to <typeparamref name="TOut"/>
		/// </summary>
		protected abstract TOut Read();

		/// <inheritdoc cref="IFileHandle.OpenRead"/>
		public Stream OpenRead()
		{
			this.ThrowIfDead();

			return _handle.OpenRead();
		}

		/// <summary>
		///		Returns the cached value, if it exists. Otherwise, inserts the result of the reader into the cache, and returns it
		/// </summary>
		public TOut GetOrRead()
		{
			this.ThrowIfDead();

			return _cached ??= Read();
		}

		/// <inheritdoc cref="IDisposable.Dispose"/>
		public void Dispose()
		{
			if (!IsAlive)
			{
				return;
			}

			_handle.Updated -= OnUpdate;
			_handle.Deleted -= OnDelete;

			_cached = default;
			IsAlive = false;
		}

		/// <inheritdoc cref="object.ToString"/>
		public override string ToString()
		{
			return $"<{typeof(TOut)}> {_handle}";
		}
	}
}
