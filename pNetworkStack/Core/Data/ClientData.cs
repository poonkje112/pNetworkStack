using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace pNetworkStack.Core.Data
{
	public class ClientData
	{
		public const int BufferSize = 1024;
		public byte[] Buffer = new byte[BufferSize];
		public StringBuilder Builder = new StringBuilder();
		public Socket WorkClient = null;

		public User UserData;

		public void SendData(byte[] data)
		{
			SendData(data, null);
		}
		
		public void SendData(byte[] data, User ignoreSender)
		{
			if (ignoreSender != null && ignoreSender == UserData) return;
			WorkClient.BeginSend(data, 0, data.Length, 0, OnSendDone, WorkClient);
		}

		void OnSendDone(IAsyncResult ar)
		{
			Socket s = (Socket) ar.AsyncState;
			s.EndSend(ar);
		}
	}
}