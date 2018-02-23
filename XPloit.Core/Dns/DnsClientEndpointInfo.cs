using System.Net;

namespace XPloit.Core.Dns
{
	internal class DnsClientEndpointInfo
	{
		public bool IsMulticast;
		public IPAddress LocalAddress;
		public IPAddress ServerAddress;
	}
}