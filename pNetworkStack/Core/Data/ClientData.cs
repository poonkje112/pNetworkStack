using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace pNetworkStack.Core.Data
{
	internal class ClientData
	{
		public const int BufferSize = 1024;
		public byte[] Buffer = new byte[BufferSize];
		public StringBuilder Builder = new StringBuilder();
		public Socket WorkClient = null;

		public User UserData;

		Queue<byte[]> m_MessageQueue = new Queue<byte[]>();

		public void SendData(byte[] data)
		{
			m_MessageQueue.Enqueue(data);
			if (m_MessageQueue.Count < 2)
			{
				byte[] msg = m_MessageQueue.Dequeue();
				WorkClient.BeginSend(msg, 0, msg.Length, 0, OnSendDone, WorkClient);
			}
		}

		void OnSendDone(IAsyncResult ar)
		{
			Socket s = (Socket) ar.AsyncState;
			s.EndSend(ar);

			if (m_MessageQueue.Count > 0)
			{
				byte[] data = m_MessageQueue.Dequeue();
				s.BeginSend(data, 0, data.Length, 0, OnSendDone, s);
			}
		}
	}
}