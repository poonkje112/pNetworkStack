using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using pNetworkStack.Commands;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;

namespace pNetworkStack.Server
{
	public class Server
	{
		//TODO Parse the received data to the CommandHandler

		private static Server Instance;

		// All server/client commands gets handled here
		private CommandHandler m_CommandHandler;

		// Our state to check if the server is running or not
		private bool m_IsRunning;

		private List<ClientData> m_Clients = new List<ClientData>();

		/// <summary>
		/// Creates and starts a server on the specified port
		/// </summary>
		/// <param name="port">the port the server should listen on</param>
		/// <returns>An instance of the server</returns>
		public static Server CreateServer(int port)
		{
			if (Instance == null) Instance = new Server(port);
			return Instance;
		}

		/// <summary>
		/// Gets the current active and running server instance
		/// </summary>
		/// <returns>Server instance</returns>
		public static Server GetCurrent()
		{
			return Instance;
		}

		private Server(int port)
		{
			if (Instance == null)
			{
				Debugger.Log("Starting server...");

				// Start listening
				TcpListener listener = new TcpListener(System.Net.IPAddress.Any, port);
				listener.Start();

				// Set the running state to true
				m_IsRunning = true;

				// Get an instance of the CommandHandler
				m_CommandHandler = CommandHandler.GetHandler();

				Debugger.Log($"Server started on: {System.Net.IPAddress.Any}:{port}");

				// Wait for a new TcpClient
				listener.BeginAcceptSocket(AcceptClient, listener);
			}
		}

		private void AcceptClient(IAsyncResult ar)
		{
			// Gets the socket that handles the requests
			TcpListener listener = (TcpListener) ar.AsyncState;
			Socket handler = listener.EndAcceptSocket(ar);

			// Save the client data for async use
			ClientData data = new ClientData();
			data.WorkClient = handler;

			m_Clients.Add(data);

			// Start receiving data
			handler.BeginReceive(data.Buffer, 0, ClientData.BufferSize, 0, ReadCallback, data);

			// Restart the waiting for a new connection
			listener.BeginAcceptSocket(AcceptClient, listener);
		}

		private void ReadCallback(IAsyncResult ar)
		{
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
				string content = data.Builder.ToString();
				if (content.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
				{
					// Parse the command to the parser
					Util.ParseCommand(handler, content.Replace("<EOF>", ""),
						(command, parameters) =>
						{
							CommandHandler.GetHandler().ExecuteServerCommand(command, parameters);
						});

					// Clear the buffer and builder to prepare for new data
					data.Buffer = new byte[ClientData.BufferSize];
					data.Builder.Clear();
				}
			}

			// Start receiving again
			handler.BeginReceive(data.Buffer, 0, ClientData.BufferSize, 0, ReadCallback, data);
		}

		public void Send(Socket handler, string message)
		{
			// If the message already contains <EOF> then remove it.
			if (message.Contains("<EOF>")) message = message.Replace("<EOF>", "");

			// Convert the message to bytes
			byte[] data = Encoding.ASCII.GetBytes(message + "<EOF>");

			// Send message the message
			handler.Send(data, 0, data.Length, 0);
		}


		/// <summary>
		/// Sends a message to all connected clients
		/// </summary>
		/// <param name="sender">The client that is sending this</param>
		/// <param name="message">The message</param>
		public void SendRPC(Socket sender, string message)
		{
			foreach (ClientData client in m_Clients)
			{
				// Get the socket of the receiving end
				Socket receiver = client.WorkClient;

				// Check if the receiver and the sender are the same
				if (sender != null && receiver == sender) continue;

				// Send the message to the receiver
				Send(receiver, message);
			}
		}

		public bool IsRunning
		{
			get => m_IsRunning;
		}
	}
}