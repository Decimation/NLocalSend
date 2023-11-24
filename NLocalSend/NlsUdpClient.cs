using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NLocalSend
{
	public class NlsUdpClient
	{

		public const int DEFAULT_UDP_PORT = 53317;

		public static readonly IPAddress DefaultMulticastGroup = IPAddress.Parse("224.0.0.167");

		/*
		 * LocalSend does not require a specific port or multicast address but instead provides a default configuration.

		   Everything can be configured in the app settings if the port / address is somehow unavailable.

		   The default multicast group is 224.0.0.0/24 because some Android devices reject any other multicast group.

		   Multicast (UDP)

		   Port: 53317
		   Address: 224.0.0.167

		   HTTP (TCP)

		   Port: 53317

		 */

		public UdpClient Client { get; private set; }

		public NlsUdpClient(int port = DEFAULT_UDP_PORT)
		{
			Client = new UdpClient(port);
		}

		public void Join()
		{
			Client.JoinMulticastGroup(DefaultMulticastGroup);
			
		}

		public async Task<NlsResponse> ReceiveResponseAsync()
		{
			var res = await Client.ReceiveAsync();
			var o = JsonSerializer.Deserialize<NlsResponse>(res.Buffer);
			o.Result = res;
			return o;
		}

	}

	// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
	public class NlsResponse
	{
		[JsonIgnore]
		public UdpReceiveResult Result { get; internal set; }

		[JsonPropertyName("alias")]
		public string Alias { get; set; }

		[JsonPropertyName("version")]
		public string Version { get; set; }

		[JsonPropertyName("deviceModel")]
		public string DeviceModel { get; set; }

		[JsonPropertyName("deviceType")]
		public string DeviceType { get; set; }

		[JsonPropertyName("fingerprint")]
		public string Fingerprint { get; set; }

		[JsonPropertyName("port")]
		public int Port { get; set; }

		[JsonPropertyName("protocol")]
		public string Protocol { get; set; }

		[JsonPropertyName("download")]
		public bool Download { get; set; }

		[JsonPropertyName("announcement")]
		public bool Announcement { get; set; }

		[JsonPropertyName("announce")]
		public bool Announce { get; set; }

		public override string ToString()
		{
			return
				$"{nameof(Alias)}: {Alias}, {nameof(Version)}: {Version}, {nameof(DeviceModel)}: {DeviceModel}, " +
				$"{nameof(DeviceType)}: {DeviceType}, {nameof(Fingerprint)}: {Fingerprint}, {nameof(Port)}: {Port}, " +
				$"{nameof(Protocol)}: {Protocol}, {nameof(Download)}: {Download}, {nameof(Announcement)}: {Announcement}, " +
				$"{nameof(Announce)}: {Announce}";
		}

	}
}