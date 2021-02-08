using System.Collections.Generic;
using System.Text;
using Deli.Newtonsoft.Json.Linq;
using Deli.Runtime.Yielding;
using UnityEngine.Networking;

namespace Deli.Runtime
{
	internal class JsonRestClient
	{
		public string Url { get; }

		public Dictionary<string, string> RequestHeaders { get; set; }

		public XRateLimit? RateLimit { get; set; }

		public JsonRestClient(string url)
		{
			Url = url;
			RequestHeaders = new Dictionary<string, string>();
		}

		private static void ApplyHeaders(UnityWebRequest request, Dictionary<string, string> headers)
		{
			foreach (var header in headers)
			{
				request.SetRequestHeader(header.Key, header.Value);
			}
		}

		private ResultYieldInstruction<UnityWebRequest> Request(string path, string method, Dictionary<string, string>? headers = null)
		{
			return (RateLimit?.Use() ?? new DummyYieldInstruction()).CallbackWith(() =>
			{
				var request = new UnityWebRequest(Url + path, method)
				{
					downloadHandler = new DownloadHandlerBuffer()
				};

				ApplyHeaders(request, RequestHeaders);
				if (headers is not null)
				{
					ApplyHeaders(request, headers);
				}

				return request;
			}).ContinueWith(request => request.Send()).CallbackWith(request =>
			{
				RateLimit?.Update(request);
				return request;
			});
		}

		public ResultYieldInstruction<JObject?> Get(string path, Dictionary<string, string>? headers = null)
		{
			return Request(path, UnityWebRequest.kHttpVerbGET, headers).CallbackWith(request =>
			{
				var data = request.downloadHandler.data;
				return data is null ? null : JObject.Parse(Encoding.UTF8.GetString(data));
			});
		}
	}
}
