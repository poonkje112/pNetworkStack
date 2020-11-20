using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;

namespace pNetworkStack.client
{
	public class Client
	{
		//TODO Parse the received data to the CommandHandler

		private readonly Socket m_Client;

		public Client(string ip, int port)
		{
			// Parse the ip
			IPAddress target = IPAddress.Parse(ip);
			IPEndPoint remoteEndPoint = new IPEndPoint(target, port);

			try
			{
				// Create the socket
				m_Client = new Socket(target.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				// Connect to the server
				m_Client.Connect(remoteEndPoint);
			}
			catch (SocketException e)
			{
				Debugger.Log(e.Message, LogType.error);
				return;
			}

			// Start the receive loop
			Receive(m_Client);
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
				Debugger.Log(e.Message, LogType.error);
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			string content = string.Empty;

			// Retrieve our ClientData from our async object
			ClientData data = (ClientData) ar.AsyncState;
			Socket handler = data.WorkClient;

			// Get the amount of data
			int bytesToRead = handler.EndReceive(ar);

			// Check if there is any data to process
			if (bytesToRead > 0)
			{
				// Apply the received data to the string builder
				data.Builder.Append(Encoding.ASCII.GetString(data.Buffer, 0, bytesToRead));

				// Check if we have reached the end
				content = data.Builder.ToString();
				if (content.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
				{
					//TODO Remove this
					Debugger.Log($"Received:\n{content.Replace("<EOF>", "")}");

					// Clear the buffer and builder to prepare for new data
					data.Buffer = new byte[ClientData.BufferSize];
					data.Builder.Clear();

					// Start receiving new data
					handler.BeginReceive(data.Buffer, 0, ClientData.BufferSize, 0, ReceiveCallback, data);
				}
				else
				{
					// Not all data has been received, get more data.
					handler.BeginReceive(data.Buffer, 0, ClientData.BufferSize, 0, ReceiveCallback, data);
				}
			}
			else
			{
				// Start receiving again
				handler.BeginReceive(data.Buffer, 0, ClientData.BufferSize, 0, ReceiveCallback, data);
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
		/// Returns the connection state of the socket
		/// </summary>
		public bool IsConnected
		{
			get => m_Client.Connected;
		}
	}
}