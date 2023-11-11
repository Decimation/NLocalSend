using System.Net;
using System.Net.Sockets;
using System.Text.Json.Nodes;

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
			HttpListener listener = new HttpListener();
			listener.Start();
			var cl=new HttpClient();
			cl.SendAsync(new HttpRequestMessage(HttpMethod.Post, "POST /api/localsend/v2/register"))
			var          c        =await listener.GetContextAsync();
			Console.WriteLine(c);
		}

	}
}