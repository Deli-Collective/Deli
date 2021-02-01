using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Deli.Setup
{
	public class JsonRestClient
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
			UnityWebRequest? request = null;
			return (RateLimit?.Use() ?? new DummyYieldInstruction()).ContinueWith(() =>
			{
				request = new UnityWebRequest(Url + path, method);

				ApplyHeaders(request, RequestHeaders);
				if (headers is not null)
				{
					ApplyHeaders(request, headers);
				}

				return request.Send();
			}).CallbackWith(() =>
			{
				var requestNonNull = request!;

				RateLimit?.Update(requestNonNull);
				return requestNonNull;
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
