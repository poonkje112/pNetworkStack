using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPHolePunchTest
{
	public class Client
	{
		private UdpClient m_Client;

		Task m_ReceiveThread;

		private bool m_isPeerConnected = false;

		private string m_TargetIp;
		private int m_TargetPort;

		public void Connect(string ip, int port)
		{
			m_TargetIp = ip;
			m_TargetPort = port;

			m_Client = new UdpClient(new IPEndPoint(IPAddress.Parse(GetExternalIp()), 0));
			Send(ip, port, "Hello");

			m_ReceiveThread = Task.Run(() => ServerReceive());
		}

		private async void ServerReceive()
		{
			while (!m_isPeerConnected)
			{
				try
				{
					if (m_ReceiveThread.IsCanceled) return;

					UdpReceiveResult received = await m_Client.ReceiveAsync();
					string message = Encoding.ASCII.GetString(received.Buffer, 0, received.Buffer.Length);

					// Check if we have received an IP address and port from the server.
					if (message.Contains(":"))
					{
						string[] split = message.Split(':');
						string ip = split[0];
						int port = int.Parse(split[1]);

						m_TargetIp = ip;
						m_TargetPort = port;

						// Send a message to the new target.
						Send(ip, port, "Hello!");
						m_isPeerConnected = true;
						m_ReceiveThread = Task.Run(() => PeerReceive());
					}

					Console.WriteLine($"[{received.RemoteEndPoint}] Received: " + message);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		private async void PeerReceive()
		{
			while (m_isPeerConnected)
			{
				try
				{
					if (m_ReceiveThread.IsCanceled) return;

					UdpReceiveResult received = await m_Client.ReceiveAsync();
					string message = Encoding.ASCII.GetString(received.Buffer, 0, received.Buffer.Length);

					Console.WriteLine($"[{received.RemoteEndPoint}] Received: " + message);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		public void Send(string ip, int port, string message)
		{
			byte[] data = Encoding.ASCII.GetBytes(message);
			m_Client.Send(data, data.Length, ip, port);
		}

		public void Send(string message)
		{
			Send(m_TargetIp, m_TargetPort, message);
		}
	}
}