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

		public JsonImmediateReader(IImmediateReader<JObject> jObject, JsonSerializer serializer)
		{
			_jObject = jObject;
			_serializer = serializer;
		}

		public T Read(IFileHandle handle)
		{
			return _jObject.Read(handle).ToObject<T>() ?? throw new FormatException("JSON file contained a null object.");
		}
	}
}
