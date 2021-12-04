using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;

namespace pNetworkStack.client
{
	public class P2PUdpClient
	{
		private static P2PUdpClient m_Instance;

		public static string Username;

		private UdpClient m_Client;

		public static Action OnClientConnected, OnClientDisconnected;
		public static Action<User, string> OnMessageReceived;
		public static Action<string> OnUserLeft;
		public static Action<User> OnUserJoined;

		internal Dictionary<string, User> ConnectedUsers = new Dictionary<string, User>();

		public User OurUser;

		internal bool m_IsReady;

		public bool IsReady => m_IsReady;

		/// <summary>
		/// This will create and return a new client connection instance
		/// </summary>
		/// <param name="ip">target ip for connection</param>
		/// <param name="port">target port for connection</param>
		/// <returns>A new running instance</returns>
		public static P2PUdpClient CreateClient()
		{
			return m_Instance ?? (m_Instance = new P2PUdpClient());
		}

		private P2PUdpClient()
		{
			m_Client = new UdpClient();

			try
			{
				Debugger.Log("Connecting to Lobby...");
				// Connect to our lobby server
				m_Client.Connect("127.0.0.1", 2117);
				
				Debugger.Log($"Connected to Lobby!");


				m_IsReady = true;

				m_Client.BeginReceive(ReceiveCallback, null);

				// Convert string to byte array
				byte[] data = Encoding.ASCII.GetBytes("Hello, World!");
				m_Client.Send(data, data.Length);
			}
			catch (Exception ex)
			{
				Debugger.Log(ex.ToString());
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
		}

		public void Send(string data)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(data);
			var result = m_Client.SendAsync(bytes, bytes.Length);
			result.Wait();
		}
	}
}