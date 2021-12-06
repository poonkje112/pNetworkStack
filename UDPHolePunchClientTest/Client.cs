using System;
using System.Collections.Generic;
using System.Linq;
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

		string m_ServerIp;
		int m_ServerPort;

		public async void Connect(string serverIp, int serverPort, bool host)
		{
			IsHost = host;

			// Create a new UdpClient with the newly mapped port.
			m_Client = new UdpClient();

			m_ServerIp = serverIp;
			m_ServerPort = serverPort;

			m_Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			m_Client.AllowNatTraversal(true);
			m_Client.Client.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);

			// Receive data without having to bind the socket
			m_ReceiveThread = new Task(Receive);

			Send($"Hello, World!");
			m_ReceiveThread.Start();
		}

		private async void Receive()
		{
			while (true)
			{
				try
				{
					IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
					UdpReceiveResult data = await m_Client.ReceiveAsync();
					remoteEP = data.RemoteEndPoint;
					string message = Encoding.ASCII.GetString(data.Buffer, 0, data.Buffer.Length);

					if (m_LobbyServer && message == "You are now the host!")
					{
						m_LobbyServer = false;
						Host();
					}

					if (m_LobbyServer && !IsHost)
					{
						string[] split = message.Split(':');
						string ip = split[0].Replace(" ", "");
						int port = int.Parse(split[1]);

						m_ServerIp = ip;
						m_ServerPort = port;

						m_LobbyServer = false;
					}

					Console.WriteLine($"[{remoteEP}] Received: " + message);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		public void StartServer()
		{
			Console.WriteLine("Started listening on " + m_Client.Client.LocalEndPoint);
			m_LobbyServer = false;
		}


		public async void Send(string message)
		{
			// Send the message to the server
			byte[] data = Encoding.ASCII.GetBytes(message);
			await m_Client.SendAsync(data, data.Length, m_ServerIp.ToString(), m_ServerPort);
		}

		public void Host()
		{
			IsHost = true;
			StartServer();
		}

		public async void Cleanup()
		{
		}
	}
}