using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPHolePunchTest
{
	public class Server
	{
		private UdpClient m_Client;

		private bool m_IsRunning;
		public bool IsRunning => m_IsRunning;

		private string m_Host;

		// Starts an UDP server on the specified port
		public void Start(int port)
		{
			m_Client = new UdpClient(new IPEndPoint(IPAddress.Any, port));

			m_IsRunning = true;
			
			Thread t = new Thread(Receive);
			t.Start();
		}

		private async void Receive()
		{
			while (m_IsRunning)
			{
				UdpReceiveResult result = await m_Client.ReceiveAsync();

				string message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length);
				Console.WriteLine($"[{result.RemoteEndPoint}]: " + message);

				byte[] data = Encoding.ASCII.GetBytes("You are now the host!");
				if (string.IsNullOrEmpty(m_Host))
				{
					m_Host = result.RemoteEndPoint.ToString();
					await m_Client.SendAsync(data, data.Length, result.RemoteEndPoint);
				}
				else
				{
					data = Encoding.ASCII.GetBytes(result.RemoteEndPoint.ToString());
					await m_Client.SendAsync(data, data.Length, result.RemoteEndPoint);
				}
			}
		}
	}
}