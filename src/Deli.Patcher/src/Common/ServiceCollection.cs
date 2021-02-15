using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;

namespace Deli
{
	/// <summary>
	///		A collection of objects, each keyed by a type
	/// </summary>
	public abstract class ServiceCollection : IEnumerable
	{
		private readonly ManualLogSource _logger;
		private readonly Dictionary<Type, object> _services = new();

		/// <summary>
		///		Creates an instance of <see cref="ServiceCollection"/>
		/// </summary>
		/// <param name="logger">The logger to use, in case a warning needs to be displayed</param>
		protected ServiceCollection(ManualLogSource logger)
		{
			_logger = logger;
		}

		/// <summary>
		///		Adds a service to this collection
		/// </summary>
		/// <param name="type">The type to key the service by</param>
		/// <param name="service">The service itself</param>
		protected void Add(Type type, object service)
		{
			if (_services.ContainsKey(type))
			{
				_logger.LogWarning($"A reader for that type ({type}) already exists.");
				return;
			}

			_services.Add(type, service);
		}

		/// <summary>
		///		Gets a service from this collection
		/// </summary>
		/// <param name="type">The type the service is keyed by</param>
		/// <exception cref="KeyNotFoundException">The service was not found</exception>
		protected object Get(Type type)
		{
			if (!_services.TryGetValue(type, out var obj))
			{
				throw new KeyNotFoundException($"The reader for that type ({type}) was not found.");
			}

			return obj;
		}

		/// <summary>
		///		Gets a service from this collection. If not found, returns <see langword="false"/>.
		/// </summary>
		/// <param name="type">The type the service is keyed by</param>
		/// <param name="service">The service, if it was found</param>
		protected bool TryGet(Type type, [NotNullWhen(true)] out object? service)
		{
			return _services.TryGetValue(type, out service);
		}

		/// <summary>
		///		Enumerates over all of the services, with their corresponding type, in this collection
		/// </summary>
		public Dictionary<Type,object>.ValueCollection.Enumerator GetEnumerator()
		{
			return _services.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
