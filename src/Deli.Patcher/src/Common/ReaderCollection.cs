using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;

namespace Deli.Immediate
{
	/// <summary>
	///		A collection of <see cref="Reader{TOut}"/>s that can be added to and retrieved
	/// </summary>
	public class ReaderCollection : ServiceCollection
	{
		internal ReaderCollection(ManualLogSource logger) : base(logger)
		{
		}

		/// <summary>
		///		Adds a reader to the collection
		/// </summary>
		/// <param name="reader">The reader to add</param>
		/// <typeparam name="T">The type the reader is responsible for</typeparam>
		public void Add<T>(Reader<T> reader) where T : notnull
		{
			Add(typeof(T), reader);
		}

		/// <summary>
		///		Gets a reader, throwing if it does not exist
		/// </summary>
		/// <typeparam name="T">The type to get the reader for</typeparam>
		/// <exception cref="KeyNotFoundException">Reader was not present</exception>
		public Reader<T> Get<T>() where T : notnull
		{
			return (Reader<T>) Get(typeof(T));
		}

		/// <summary>
		///		Tries to get a reader, returning a success <see langword="bool"/> instead of throwing
		/// </summary>
		/// <param name="reader">The reader, if it was found</param>
		/// <typeparam name="T">The type to get the reader for</typeparam>
		public bool TryGet<T>([MaybeNullWhen(false)] out Reader<T> reader) where T : notnull
		{
			if (TryGet(typeof(T), out var obj))
			{
				reader = (Reader<T>) obj;
				return true;
			}

			reader = null;
			return false;
		}
	}
}
