using System;
using System.Net.Sockets;

namespace pNetworkStack.Core
{
	internal struct SendObject
	{
		public Socket Socket;
		public Action OnSendDone;
	}
}