using System.Net;
using System.Net.Sockets;

namespace pNetworkStack.Core
{
	internal struct ConnectionInfo
	{
		public TcpListener Listener;
		public UdpClient UdpClient;
		public IPEndPoint EndPoint;
		public ConnectionType Type;
	}
}