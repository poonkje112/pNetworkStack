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
	public class Client
	{
		private static Client m_Instance;

		public static string Username;

		private Socket m_Client;

		public static Action OnClientConnected, OnClientDisconnected;
		public static Action<User, string> OnMessageReceived;
		public static Action<string> OnUserLeft;
		public static Action<User> OnUserJoined;

		internal Dictionary<string, User> ConnectedUsers = new Dictionary<string, User>();

		public User OurUser;

		/// <summary>
		/// This will create and return a new client connection instance
		/// </summary>
		/// <param name="ip">target ip for connection</param>
		/// <param name="port">target port for connection</param>
		/// <returns>A new running instance</returns>
		public static Client CreateClient(string ip, int port)
		{ 
			if(m_Instance == null) return new Client(ip, port);
			return m_Instance;
		}

		private Client(string ip, int port)
		{
			if (m_Instance == null)
			{
				// Parse the ip
				IPAddress target = IPAddress.Parse(ip);
				IPEndPoint remoteEndPoint = new IPEndPoint(target, port);

				if (OurUser == null) OurUser = new User();

				OurUser.Username = Username;

				try
				{
					// Create the socket
					m_Client = new Socket(target.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
					m_Instance = this;

					// Connect to the server
					m_Client.Connect(remoteEndPoint);
					OnClientConnected?.Invoke();
				}
				catch (SocketException e)
				{
					m_Instance = null;
					Debugger.Log(e.Message, LogType.Error);
					return;
				}

				// Start the receive loop
				Receive(m_Client);
			}
		}

		public static Client GetCurrent()
		{
			return m_Instance;
		}

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
				client.BeginReceive(data.Buffer, 0, ClientData.BufferSize, 0, ReceiveCallback, data);
			}
			catch (Exception e)
			{
				Debugger.Log(e.Message, LogType.Error);
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			// Retrieve our ClientData from our async object
			ClientData data = (ClientData) ar.AsyncState;

			try
			{
				Socket handler = data.WorkClient;

				// Get the amount of data
				int bytesToRead = handler.EndReceive(ar);

				// Check if there is any data to process
				if (bytesToRead > 0)
				{
					// Apply the received data to the string builder
					data.Builder.Append(Encoding.ASCII.GetString(data.Buffer, 0, bytesToRead));

					// Check if we have reached the end
					string content = data.Builder.ToString();
					if (content.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
					{
						// Parse the command to the parser
						Util.ParseCommand(content.Replace("<EOF>", ""),
							(command, parameters) =>
							{
								CommandHandler.GetHandler().ExecuteClientCommand(command, parameters);
							});

						// Clear the buffer and builder to prepare for new data
						data.Buffer = new byte[ClientData.BufferSize];
						data.Builder.Clear();
					}
				}

				// Start receiving again
				handler.BeginReceive(data.Buffer, 0, ClientData.BufferSize, 0, ReceiveCallback, data);
			}
			catch (SocketException e)
			{
				// Assume the serer has stopped
				Debugger.Log("Lost connection!", LogType.Warning);
				StopConnection();
			}
		}

		/// <summary>
		/// Send a message/command to the server
		/// </summary>
		/// <param name="message">The message that needs to get sent</param>
		public void Send(string message)
		{
			// If the message already contains <EOF> then remove it.
			if (message.Contains("<EOF>")) message = message.Replace("<EOF>", "");
			byte[] data = Encoding.ASCII.GetBytes(message + "<EOF>");

			m_Client.Send(data, 0, data.Length, 0);
		}

		/// <summary>
		/// Stops the current client connection and destroys the running instance
		/// </summary>
		public void StopConnection()
		{
			if (m_Client.Connected)
			{
				try
				{
					m_Client.Disconnect(false);
				}
				catch (Exception e)
				{
					// ignored
				}

				m_Client = null;
				m_Instance = null;

				OnClientDisconnected?.Invoke();
			}
		}

		/// <summary>
		/// Returns the connection state of the socket
		/// </summary>
		public bool IsConnected
		{
			get => m_Client.Connected;
		}
	}
}