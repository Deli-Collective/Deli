using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;

namespace Deli.Runtime
{
	/// <summary>
	///		A collection of <see cref="DelayedReader{T}"/>s that can be added to and retrieved
	/// </summary>
	public class DelayedReaderCollection : ServiceCollection
	{
		/// <summary>
		///		Creates an instance of <see cref="DelayedReaderCollection"/>
		/// </summary>
		/// <param name="logger">The logger to use, in case a warning needs to be displayed</param>
		public DelayedReaderCollection(ManualLogSource logger) : base(logger)
		{
		}

		/// <summary>
		///		Adds a reader to the collection
		/// </summary>
		/// <param name="reader">The reader to add</param>
		/// <typeparam name="T">The type the reader is responsible for</typeparam>
		public void Add<T>(DelayedReader<T> reader) where T : notnull
		{
			Add(typeof(T), reader);
		}

		/// <summary>
		///		Gets a reader, throwing if it does not exist
		/// </summary>
		/// <typeparam name="T">The type to get the reader for</typeparam>
		/// <exception cref="KeyNotFoundException">Reader was not present</exception>
		public DelayedReader<T> Get<T>() where T : notnull
		{
			return (DelayedReader<T>) Get(typeof(T));
		}

		/// <summary>
		///		Tries to get a reader, returning a success <see langword="bool"/> instead of throwing
		/// </summary>
		/// <param name="reader">The reader, if it was found</param>
		/// <typeparam name="T">The type to get the reader for</typeparam>
		public bool TryGet<T>([MaybeNullWhen(false)] out DelayedReader<T> reader) where T : notnull
		{
			if (TryGet(typeof(T), out var obj))
			{
				reader = (DelayedReader<T>) obj;
				return true;
			}

			reader = null;
			return false;
		}
	}
}
