using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using pNetworkStack.Commands;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;

namespace pNetworkStack.client
{
	public class Client
	{
	#region Variables

		private static Client m_Instance;

		public static string Username;

		private Socket m_Client;
		private UdpClient m_UdpClient;

		private IPEndPoint m_ServerEndPoint;

		public static Action OnClientConnected, OnClientDisconnected;
		public static Action<User, string> OnMessageReceived;
		public static Action<string> OnUserLeft;
		public static Action<User> OnUserJoined;

		public ConnectionType ConnectionType { get; private set; }

		internal Dictionary<string, User> ConnectedUsers = new Dictionary<string, User>();

		public User OurUser;

		internal bool m_IsReady;

		public bool IsReady => m_IsReady;

	#endregion

	#region Client Setup

		/// <summary>
		/// This will create and return a new client connection instance
		/// </summary>
		/// <param name="ip">target ip for connection</param>
		/// <param name="port">target port for connection</param>
		/// <returns>A new running instance</returns>
		public static Client CreateClient(string ip, int port, ConnectionType connectionType = ConnectionType.TCP)
		{
			if (m_Instance == null) return new Client(ip, port, connectionType);
			return m_Instance;
		}

		private Client(string ip, int port, ConnectionType connectionType)
		{
			if (m_Instance == null)
			{
				// Parse the ip
				IPAddress target = IPAddress.Parse(ip);
				m_ServerEndPoint = new IPEndPoint(target, port);

				ConnectionType = connectionType;

				if (OurUser == null) OurUser = new User();

				OurUser.Username = Username;

				try
				{
					if (connectionType == ConnectionType.TCP)
					{
						// Create the socket
						m_Client = new Socket(target.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

						// Connect to the server
						m_Client.Connect(m_ServerEndPoint);

						// Start the receive loop
						Receive(m_Client);
					}
					else
					{
						m_UdpClient = new UdpClient();
						m_UdpClient.Connect(m_ServerEndPoint);

						// Start the receive loop
						Receive(m_UdpClient);

						// Convert string to byte array
						byte[] data = new Packet("hello").SerializePacket();

						// Send the first packet to tell the server we are joining
						m_UdpClient.Send(data, data.Length);
					}

					m_Instance = this;
				}
				catch (SocketException e)
				{
					m_Instance = null;
					Debugger.Log(e.Message, LogType.Error);
					return;
				}
			}
		}

		public static Client GetCurrent()
		{
			return m_Instance;
		}

	#endregion

	#region Receive Data

		private void Receive(Socket client)
		{
			try
			{
				// Get the socket
				ClientData data = new ClientData()
				{
					WorkClient = client
				};

				// Start the receiving loop
				client.BeginReceive(data.Buffer, 0, ClientData.BufferSize, 0, ReceiveCallbackTCP, data);
			}
			catch (Exception e)
			{
				Debugger.Log(e.Message, LogType.Error);
			}
		}

		private void Receive(UdpClient client)
		{
			try
			{
				ClientData data = new ClientData()
				{
					RemoteEndPoint = m_ServerEndPoint
				};

				// Start the receiving loop
				client.BeginReceive(ReceiveCallbackUDP, data);
			}
			catch (Exception e)
			{
				Debugger.Log(e.Message, LogType.Error);
			}
		}

		private void ReceiveCallbackTCP(IAsyncResult ar)
		{
			// Retrieve our ClientData from our async object
			ClientData client = (ClientData)ar.AsyncState;

			try
			{
				Socket handler = client.WorkClient;

				// Get the amount of data
				int bytesToRead = handler.EndReceive(ar);

				Util.ProcessData(ref client, bytesToRead,
					(cmd) => { CommandHandler.GetHandler().ExecuteClientCommand(cmd); });

				// Start receiving again
				handler.BeginReceive(client.Buffer, 0, ClientData.BufferSize, 0, ReceiveCallbackTCP, client);
			}
			catch (SocketException e)
			{
				// Assume the serer has stopped
				Debugger.Log("Lost connection!", LogType.Warning);
				StopConnection();
			}
		}

		private void ReceiveCallbackUDP(IAsyncResult ar)
		{
			ClientData client = (ClientData)ar.AsyncState;

			try
			{
				UdpClient server = m_UdpClient;
				IPEndPoint remoteEndPoint = client.RemoteEndPoint;

				// Get the amount of data
				client.Buffer = server.EndReceive(ar, ref remoteEndPoint);

				Util.ProcessData(ref client, client.Buffer.Length,
					(cmd) => { CommandHandler.GetHandler().ExecuteClientCommand(cmd); });

				// Start receiving again
				server.BeginReceive(ReceiveCallbackUDP, client);
			}
			catch (SocketException e)
			{
				// TODO This will never happen as there is no stream like in TCP
				// Assume the serer has stopped
				Debugger.Log("Lost connection!", LogType.Warning);
				StopConnection();
			}
		}

	#endregion

	#region Send Data

		public void Send(Packet packet, Action onSendDone = null)
		{
			byte[] data = packet.SerializePacket();

			SendObject sendObject = new SendObject()
			{
				OnSendDone = onSendDone
			};

			if (ConnectionType == ConnectionType.TCP)
			{
				sendObject.Socket = m_Client;
				m_Client.BeginSend(data, 0, data.Length, 0, OnSendDone, sendObject);
			}
			else
			{
				m_UdpClient.Send(data, data.Length);
				onSendDone?.Invoke();
			}
		}

		void OnSendDone(IAsyncResult ar)
		{
			SendObject sobj = (SendObject)ar.AsyncState;
			Socket s = sobj.Socket;

			if (ConnectionType == ConnectionType.TCP)
				s.EndSend(ar);
			else
				m_UdpClient.EndSend(ar);

			sobj.OnSendDone?.Invoke();
		}

	#endregion

		/// <summary>
		/// Stops the current client connection and destroys the running instance
		/// </summary>
		public void StopConnection()
		{
			if (m_Client.Connected)
			{
				try
				{
					if (ConnectionType == ConnectionType.TCP)
						m_Client.Disconnect(false);
					else
						m_UdpClient.Close();
				}
				catch (Exception e)
				{
					// ignored
				}

				m_Client = null;
				m_UdpClient = null;
				m_Instance = null;
				m_IsReady = false;

				OnClientDisconnected?.Invoke();

				foreach (string uuid in ConnectedUsers.Keys)
				{
					ConnectedUsers.Remove(uuid);
					OnUserLeft?.Invoke(uuid);
				}
			}
		}

		/// <summary>
		/// Returns the connection state of the socket
		/// </summary>
		public bool IsConnected
		{
			// TODO change this to use the handshake/heartbeat
			get => ConnectionType != ConnectionType.TCP || m_Client.Connected;
		}
	}
}