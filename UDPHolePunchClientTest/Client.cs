using System;
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

		private bool m_LobbyServer = true;

		public bool IsHost = false;

		Task m_ReceiveThread;

		public void Connect(string ip, int port, bool host)
		{
			IsHost = host;
			m_Client = new UdpClient();

			m_Client.Connect(ip, port);

			m_ReceiveThread = Task.Run(() => Receive());
		}

		public void StartServer(IPEndPoint endPoint)
		{
			m_Client = new UdpClient(endPoint);
			
			// Allow the client to receive data from any sender.
			m_Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			
			m_LobbyServer = false;
			m_ReceiveThread = Task.Run(() => Receive());
		}

		private async void Receive()
		{
			while (true)
			{
				try
				{
					if (m_ReceiveThread.IsCanceled) return;

					UdpReceiveResult received = await m_Client.ReceiveAsync();
					string message = Encoding.ASCII.GetString(received.Buffer, 0, received.Buffer.Length);

					if (m_LobbyServer && message == "You are now the host!")
					{
						m_LobbyServer = false;
						Host();
					}

					if (m_LobbyServer && !IsHost)
					{
						string[] split = message.Split(':');
						IPAddress ip = IPAddress.Parse(split[0]);
						int port = int.Parse(split[1]);

						// Disconnect from the server and connect to the ip and port
						m_Client.Dispose();
						m_Client = new UdpClient();
						m_Client.Connect(ip, port);

						m_LobbyServer = false;
					}

					Console.WriteLine($"[{received.RemoteEndPoint}] Received: " + message);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		public void Send(string message)
		{
			byte[] data = Encoding.ASCII.GetBytes(message);
			m_Client.Send(data, data.Length);
		}

		public void Host()
		{
			// Cancel the receive thread
			m_ReceiveThread.Dispose();

			IPEndPoint endPoint = m_Client.Client.LocalEndPoint as IPEndPoint;
			m_Client.Close();

			IsHost = true;

			StartServer(endPoint);
		}
	}
}