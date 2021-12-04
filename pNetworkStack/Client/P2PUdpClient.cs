using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using pNetworkStack.Commands;
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

				m_IsReady = true;

				m_Client.BeginReceive(ReceiveCallback, null);

				// Send join message

				Send("pl_join");
			}
			catch (Exception ex)
			{
				Debugger.Log(ex.ToString());
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
			byte[] data = m_Client.EndReceive(ar, ref endPoint);

			// Decode the message
			string content = Encoding.ASCII.GetString(data);

			if (content.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
			{
				content = content.Substring(0, content.IndexOf("<EOF>", StringComparison.Ordinal));

				// Parse the command to the parser
				Util.ParseCommand(content,
					(command, parameters) =>
					{
						CommandHandler.GetHandler().ExecuteClientCommand(command, parameters);
					});
			}

			m_Client.BeginReceive(ReceiveCallback, null);
		}

		public void Send(string data)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(data + "<EOF>");
			m_Client.Send(bytes, bytes.Length);
		}
	}
}