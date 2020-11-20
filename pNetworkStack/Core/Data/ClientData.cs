using System.Net.Sockets;
using System.Text;

namespace pNetworkStack.Core.Data
{
	internal class ClientData
	{
		public const int BufferSize = 1024;
		public byte[] Buffer = new byte[BufferSize];
		public StringBuilder Builder = new StringBuilder();
		public Socket WorkClient = null;

		public User UserData;
	}
}