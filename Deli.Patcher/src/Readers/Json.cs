using System;
using Deli.VFS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Deli.Patcher.Readers
{
	public class JsonImmediateReader<T> : IImmediateReader<T>
	{
		private readonly IImmediateReader<JObject> _jObject;
		private readonly JsonSerializer _serializer;

		/// <summary>
		///		Creates an instance of <see cref="JsonImmediateReader{T}"/>.
		/// </summary>
		/// <param name="jObject">The <see cref="JObject"/> reader to use.</param>
		/// <param name="serializer">The <see cref="JsonSerializer"/> for use in converting to type.</param>
		public JsonImmediateReader(IImmediateReader<JObject> jObject, JsonSerializer serializer)
		{
			_jObject = jObject;
			_serializer = serializer;
		}

		/// <inheritdoc cref="IImmediateReader{T}.Read"/>
		public T Read(IFileHandle handle)
		{
			return _jObject.Read(handle).ToObject<T>(_serializer) ?? throw new FormatException("JSON file contained a null object.");
		}
	}
}
