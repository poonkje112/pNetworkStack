using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using pNetworkStack.client;
using pNetworkStack.Commands;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;

namespace pNetworkStack.Server
{
	public class Server
	{
		private static Server Instance;

		// All server/client commands gets handled here
		private CommandHandler m_CommandHandler;

		// Our state to check if the server is running or not
		private bool m_IsRunning;

		private TpsHandler m_TpsHandler;

		internal Dictionary<string, ClientData> Clients = new Dictionary<string, ClientData>();
		internal Dictionary<string, ClientData> ClientInit = new Dictionary<string, ClientData>();
		internal Queue<Tuple<string, ClientData>> AddClientQueue = new Queue<Tuple<string, ClientData>>();


		public Action<User> OnUserJoined, OnUserLeft;
		public Action TransformUpdate, LateUpdate;

		internal Action<Packet, User> OnSendRPC;

		/// <summary>
		/// Creates and starts a server on the specified port
		/// </summary>
		/// <param name="port">the port the server should listen on</param>
		/// <returns>An instance of the server</returns>
		public static Server CreateServer(int port)
		{
			return new Server(port);
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
				Instance = this;

				Debugger.Log("Starting server...");

				// Prepare the tps handler
				m_TpsHandler = TpsHandler.GetHandler();
				m_TpsHandler.TransfromUpdate += TransfromUpdate;
				m_TpsHandler.FinalUpdate += FinalUpdate;

				// Start listening
				TcpListener listener = new TcpListener(System.Net.IPAddress.Any, port);
				listener.Start();

				// Start the tps handler
				m_TpsHandler.StartTicker();

				// Set the running state to true
				m_IsRunning = true;

				// Get an instance of the CommandHandler
				m_CommandHandler = CommandHandler.GetHandler();

				Debugger.Log($"Server started on: {System.Net.IPAddress.Any}:{port}");

				// Wait for a new TcpClient
				listener.BeginAcceptSocket(AcceptClient, listener);
			}
		}

		private void TransfromUpdate()
		{
			TransformUpdate?.Invoke();
		}

		private void FinalUpdate()
		{
			LateUpdate?.Invoke();
			while (AddClientQueue.Count > 0)
			{
				Tuple<string, ClientData> data = AddClientQueue.Dequeue();

				List<User> users = new List<User>();

				foreach (ClientData clientsValue in Clients.Values)
				{
					users.Add(clientsValue.UserData);
				}

				// byte[] dataToSend =
					// Encoding.ASCII.GetBytes($"pl_add_bulk {JsonConvert.SerializeObject(users.ToArray())}<EOF>");
				
				data.Item2.SendData(new Packet($"pl_add_bulk {JsonConvert.SerializeObject(users.ToArray())}"));

				Clients.Add(data.Item1, data.Item2);
				OnSendRPC += Clients[data.Item2.UserData.UUID].SendData;

				OnUserJoined?.Invoke(data.Item2.UserData);
			}
		}

		private void AcceptClient(IAsyncResult ar)
		{
			// Gets the socket that handles the requests
			TcpListener listener = (TcpListener)ar.AsyncState;
			Socket client = listener.EndAcceptSocket(ar);

			// Save the client data for async use
			ClientData data = new ClientData();
			data.WorkClient = client;

			// Generating a new random user id
			Random rand = new Random();
			string randomUID = rand.Next(100000, 999999).ToString();

			while (Clients.ContainsKey(randomUID))
			{
				randomUID = rand.Next(100000, 999999).ToString();
			}

			// Set the user id
			data.UserData = new User()
			{
				UUID = randomUID
			};

			ClientInit.Add(randomUID, data);

			// Start receiving data
			client.BeginReceive(data.Buffer, 0, ClientData.BufferSize, 0, ReadCallback, data);

			// Send(client, $"pl_init {randomUID}", true);
			Send(client, new Packet($"pl_init {randomUID}"), true);

			// Restart the waiting for a new connection
			listener.BeginAcceptSocket(AcceptClient, listener);
		}

		private void ReadCallback(IAsyncResult ar)
		{
			// Retrieve our ClientData from our async object
			ClientData data = (ClientData)ar.AsyncState;

			try
			{
				Socket handler = data.WorkClient;

				// Get the amount of data
				int bytesToRead = handler.EndReceive(ar);

				// Check if there is any data to process
				if (bytesToRead > 0)
				{
					if (data.PushData(data.Buffer, bytesToRead))
					{
						Packet p = data.PopPacket();
						Util.ParseCommand(data.UserData, p.Command, (command, parameters) =>
						{ 
							CommandHandler.GetHandler().ExecuteServerCommand(command, parameters);
						});

						data.ClearData();
						data.Buffer = new byte[ClientData.BufferSize];
					}

					// Apply the received data to the string builder
					// data.Builder.Append(Encoding.ASCII.GetString(data.Buffer, 0, bytesToRead));

					// Check if we have reached the end
					// string content = data.Builder.ToString();
					// if (content.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
					// {
					// 	content = content.Substring(0, content.IndexOf("<EOF>", StringComparison.Ordinal));
					//
					// 	// Parse the command to the parser
					// 	Util.ParseCommand(data.UserData, content,
					// 		(command, parameters) =>
					// 		{
					// 			CommandHandler.GetHandler().ExecuteServerCommand(command, parameters);
					// 		});
					//
					// 	// Clear the buffer and builder to prepare for new data
					// 	data.Builder.Clear();
					//  data.Buffer = new byte[ClientData.BufferSize];
					// }
				}

				// Start receiving again
				handler.BeginReceive(data.Buffer, 0, ClientData.BufferSize, 0, ReadCallback, data);
			}
			catch (SocketException e)
			{
				// Assume the client has lost connection
				DisconnectClient(data.UserData.UUID);
			}
		}
		
		[Obsolete("Use Send(User, Packet, bool) instead")]
		internal void Send(Socket receiver, string message, bool init = false)
		{
			// If the message already contains <EOF> then remove it.
			// if (message.Contains("<EOF>")) message = message.Replace("<EOF>", "");

			// Convert the message to bytes
			// byte[] data = Encoding.ASCII.GetBytes(message + "<EOF>");
			
			Dictionary<string, ClientData> clientDict;

			if (init)
				clientDict = ClientInit;
			else
				clientDict = Clients;

			foreach (ClientData u in clientDict.Values)
			{
				if (u.WorkClient == receiver)
				{
					u.SendData(new Packet(message));
					break;
				}
			}
		}

		/// <summary>
		/// Sends a message to all connected clients
		/// </summary>
		/// <param name="sender">The client that is sending this</param>
		/// <param name="message">The message</param>
		[Obsolete("Use SendRPC(User, Packet) instead")]
		public void SendRPC(User sender, string message)
		{
			// If the message already contains <EOF> then remove it.
			if (message.Contains("<EOF>")) message = message.Replace("<EOF>", "");

			// Convert the message to bytes
			byte[] data = Encoding.ASCII.GetBytes(message + "<EOF>");

			OnSendRPC?.Invoke(new Packet(message), sender);
		}
		
		public void Send(User receiver, Packet packet, bool init = false)
		{
			Send(Clients[receiver.UUID].WorkClient, packet, init);
		}

		internal void Send(Socket receiver, Packet packet, bool init = false)
		{
			Dictionary<string, ClientData> clientDict;

			clientDict = init ? ClientInit : Clients;

			foreach (ClientData u in clientDict.Values.Where(u => u.WorkClient == receiver))
			{
				u.SendData(packet);
				break;
			}
		}

		public void SendRPC(User sender, Packet packet)
		{
			OnSendRPC?.Invoke(packet, sender);
		}

		public void DisconnectClient(string uid)
		{
			ClientData sender = Clients[uid];

			// SendRPC(sender.UserData, $"pl_remove {uid}");

			SendRPC(sender.UserData, new Packet($"pl_remove {uid}"));
			
			OnSendRPC -= Clients[uid].SendData;

			Clients.Remove(uid);

			OnUserLeft?.Invoke(sender.UserData);
		}

		public bool IsRunning
		{
			get => m_IsRunning;
		}
	}
}