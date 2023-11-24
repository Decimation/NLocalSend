using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Nodes;
using Flurl.Http.Content;

namespace NLocalSend.Main
{
	internal class Program
	{

		public static async Task Main(string[] args)
		{
			var u = new NlsUdpClient();
			u.Join();
			var o = await u.ReceiveResponseAsync();
			Console.WriteLine(o);

			// HttpListener listener = new HttpListener();
			// listener.Prefixes.Add($"http://*:{NlsUdpClient.DEFAULT_UDP_PORT}/");
			// listener.Prefixes.Add($"https://*:{NlsUdpClient.DEFAULT_UDP_PORT}/");

			// listener.Start();
			/*var listen = new HttpListener()
			{
				Prefixes = { "http://localhost:53317/", "https://localhost:53317/" }
			};
			listen.Start();*/
			ServicePointManager.DefaultConnectionLimit = int.MaxValue;

			ServicePointManager.SecurityProtocol =
				SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			HttpClientHandler clientHandler = new HttpClientHandler();

			clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
			{
				Console.WriteLine($"{sender} {cert} {sslPolicyErrors}");
				return true;
			};

			IPAddress address = o.Result.RemoteEndPoint.Address;
			Console.WriteLine(address);
			var s = address.ToString();
			var b = new UriBuilder($"https://{s}");

			b.Port = o.Port;
			Console.WriteLine(b.Uri);

			var cl = new HttpClient(clientHandler);
			cl.BaseAddress = b.Uri;
			
			/*var tcp = new TcpClient();
			await tcp.ConnectAsync(o.Result.RemoteEndPoint.Address, NlsUdpClient.DEFAULT_UDP_PORT);
			var str = tcp.GetStream();

			var sw = new StreamWriter(str);

			await sw.WriteAsync("""
			                    POST /api/localsend/v2/register
			                    {
			                      "alias": "Secret Banana",
			                      "version": "2.0",
			                      "deviceModel": "Windows",
			                      "deviceType": "desktop",
			                      "fingerprint": "waifu",
			                      "port": 53317,
			                      "protocol": "https",
			                      "download": false,
			                    }
			                    """);*/

			do {
				var request = new HttpRequestMessage(HttpMethod.Post, "/api/localsend/v2/register")
				{
					Content = new StringContent("""
					                            {
					                              "alias": "Secret Banana",
					                              "version": "2.0",
					                              "deviceModel": "Windows",
					                              "deviceType": "desktop",
					                              "fingerprint": "waifu",
					                              "port": 53317,
					                              "protocol": "https",
					                              "download": false,
					                              "announce":true
					                            }
					                            """, Encoding.UTF8)
				};
				var req = await cl.SendAsync(request);
				Console.WriteLine(req);
				var res  =await  u.Client.ReceiveAsync();
				Console.WriteLine();
				new HttpListener()
			} while (Console.ReadKey(true).Key != ConsoleKey.Escape);

			Console.ReadKey();
		}

	}
	public static class TcpClientForHttp
	{
		public static Task<MemoryStream> SendHttpRequestAsync(
			Uri uri,
			HttpMethod httpMethod,
			Version httpVersion,
			CancellationToken cancellationToken)
		{
			string strHttpRequest = $"{httpMethod} {uri.PathAndQuery} HTTP/{httpVersion}\r\n";
			strHttpRequest += $"Host: {uri.Host}:{uri.Port}\r\n";
			// Any other HTTP headers can be added here ....
			strHttpRequest += "\r\n";

			return SendRequestAsync(uri, strHttpRequest, cancellationToken);
		}

		private static async Task<MemoryStream> SendRequestAsync(Uri uri, string request, CancellationToken token)
		{
			bool isHttps = uri.Scheme == Uri.UriSchemeHttps;

			using var tcpClient = new TcpClient();
			await tcpClient.ConnectAsync(uri.Host, uri.Port, token);
			await using NetworkStream ns = tcpClient.GetStream();

			var resultStream = new MemoryStream();

			if (isHttps)
			{
				await using var ssl = new SslStream(ns, false, ValidateServerCertificate, null);
				await ssl.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
				{
					TargetHost = uri.Host,
					ClientCertificates = null,
					EnabledSslProtocols = SslProtocols.None,
					CertificateRevocationCheckMode = X509RevocationMode.NoCheck
				},
					token);

				await using var sslWriter = new StreamWriter(ssl);
				await sslWriter.WriteAsync(request);
				await sslWriter.FlushAsync();

				await ssl.CopyToAsync(resultStream, token);
			}
			else
			{
				// Normal HTTP
				await using var nsWriter = new StreamWriter(ns);
				await nsWriter.WriteAsync(request);
				await nsWriter.FlushAsync();

				await ns.CopyToAsync(resultStream, token);
			}

			resultStream.Position = 0;
			return resultStream;
		}

		private static bool ValidateServerCertificate(
			object sender,
			X509Certificate? certificate,
			X509Chain? chain,
			SslPolicyErrors sslPolicyErrors)
		{
			return true; // Accept all certs
		}
	}
}