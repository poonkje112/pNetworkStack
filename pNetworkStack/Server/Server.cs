using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

		internal UdpClient UdpClient;

		internal Dictionary<string, ClientData> Clients = new Dictionary<string, ClientData>();
		internal Dictionary<string, ClientData> ClientInit = new Dictionary<string, ClientData>();
		internal Queue<Tuple<string, ClientData>> AddClientQueue = new Queue<Tuple<string, ClientData>>();

		internal Action<Packet, User> OnSendRPC;

		public Action<User> OnUserJoined, OnUserLeft;
		public Action TransformUpdate, LateUpdate;

		public ConnectionType ConnectionType { get; private set; }

		/// <summary>
		/// Creates and starts a server on the specified port
		/// </summary>
		/// <param name="port">the port the server should listen on</param>
		/// <param name="connectionType">Set the type of connection to either TCP or UDP</param>
		/// <returns>An instance of the server</returns>
		public static Server CreateServer(int port, ConnectionType connectionType = ConnectionType.TCP)
		{
			return new Server(port, connectionType);
		}

		/// <summary>
		/// Gets the current active and running server instance
		/// </summary>
		/// <returns>Server instance</returns>
		public static Server GetCurrent()
		{
			return Instance;
		}

		private Server(int port, ConnectionType connectionType)
		{
			if (Instance == null)
			{
				Instance = this;

				Debugger.Log("Starting server...");

				ConnectionType = connectionType;

				// Prepare the tps handler
				m_TpsHandler = TpsHandler.GetHandler();
				m_TpsHandler.TransfromUpdate += TransfromUpdate;
				m_TpsHandler.FinalUpdate += FinalUpdate;

				if (connectionType == ConnectionType.TCP)
					SetupTCPServer(port);
				else
					SetupUDPServer(port);

				// Start the tps handler
				m_TpsHandler.StartTicker();

				// Set the running state to true
				m_IsRunning = true;

				// Get an instance of the CommandHandler
				m_CommandHandler = CommandHandler.GetHandler();

				Debugger.Log($"Server started on: {IPAddress.Any}:{port}");
			}
		}

		private void SetupTCPServer(int port)
		{
			// Start listening
			TcpListener listener = new TcpListener(IPAddress.Any, port);
			listener.Start();

			// Wait for a new TcpClient
			listener.BeginAcceptSocket(AcceptClientTCP, listener);
		}

		private void SetupUDPServer(int port)
		{
			// Start listening
			UdpClient = new UdpClient(port);

			// Start accepting new clients
			UdpClient.BeginReceive(ReadCallbackUDP, UdpClient);
		}

		private void TransfromUpdate()
		{
			TransformUpdate?.Invoke();
		}

		private void FinalUpdate()
		{
			LateUpdate?.Invoke();

			// Check if we have any new clients to add
			while (AddClientQueue.Count > 0)
			{
				Tuple<string, ClientData> data = AddClientQueue.Dequeue();

				List<User> users = new List<User>();

				foreach (ClientData clientsValue in Clients.Values)
				{
					users.Add(clientsValue.UserData);
				}

				data.Item2.SendData(new Packet($"pl_add_bulk {JsonConvert.SerializeObject(users.ToArray())}"));

				Clients.Add(data.Item1, data.Item2);
				OnSendRPC += Clients[data.Item2.UserData.UUID].SendData;

				OnUserJoined?.Invoke(data.Item2.UserData);
			}
		}

		private void AcceptClientTCP(IAsyncResult ar)
		{
			// Gets the socket that handles the requests
			TcpListener listener = (TcpListener)ar.AsyncState;
			Socket client = listener.EndAcceptSocket(ar);

			// Save the client data for async use
			ClientData data = new ClientData();
			data.WorkClient = client;

			ProcessClient(ref data);

			// Start receiving data
			client.BeginReceive(data.Buffer, 0, ClientData.BufferSize, 0, ReadCallback, data);

			Send(client, new Packet($"pl_init {data.UserData.UUID}"), true);

			// Restart the waiting for a new connection
			listener.BeginAcceptSocket(AcceptClientTCP, listener);
		}

		private void ProcessClient(ref ClientData data)
		{
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
		}

		private void ReadCallback(IAsyncResult ar)
		{
			// Retrieve our ClientData from our async object
			ClientData client = (ClientData)ar.AsyncState;

			try
			{
				Socket handler = client.WorkClient;

				// Get the amount of data
				int bytesToRead = handler.EndReceive(ar);

				Util.ProcessData(ref client, bytesToRead, true,
					(cmd) => { CommandHandler.GetHandler().ExecuteServerCommand(cmd); });

				// Start receiving again
				handler.BeginReceive(client.Buffer, 0, ClientData.BufferSize, 0, ReadCallback, client);
			}
			catch (SocketException e)
			{
				// Assume the client has lost connection
				DisconnectClient(client.UserData.UUID);
			}
		}

		private void ReadCallbackUDP(IAsyncResult ar)
		{
			try
			{
				UdpClient listener = (UdpClient)ar.AsyncState;

				IPEndPoint source = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = listener.EndReceive(ar, ref source);
				int bytesToRead = data.Length;

				// Check if there is a client with this ipendpoint
				ClientData client = Clients.Values.FirstOrDefault(x => Equals(x.RemoteEndPoint, source));
				client = client ?? ClientInit.Values.FirstOrDefault(x => Equals(x.RemoteEndPoint, source));
				client = client ?? AddClientQueue.FirstOrDefault(x => Equals(x.Item2.RemoteEndPoint, source))?.Item2;

				// If the client has not yet been registered then we ignore their first packet and register the new client
				if (client != null)
				{
					client.Buffer = data;

					Util.ProcessData(ref client, bytesToRead, true,
						(cmd) => { CommandHandler.GetHandler().ExecuteServerCommand(cmd); });
				}
				else
				{
					client = new ClientData
					{
						RemoteEndPoint = source
					};

					ProcessClient(ref client);

					Send(source, new Packet($"pl_init {client.UserData.UUID}"), true);
				}

				// Start receiving again
				listener.BeginReceive(ReadCallbackUDP, listener);
			}
			catch (Exception e)
			{
				Debugger.Log(e.Message, LogType.Error);
			}
		}

		public void Send(User receiver, Packet packet, bool init = false)
		{
			if (ConnectionType == ConnectionType.TCP)
				Send(Clients[receiver.UUID].WorkClient, packet, init);
			else
				Send(Clients[receiver.UUID].RemoteEndPoint, packet, init);
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

		internal void Send(IPEndPoint receiver, Packet packet, bool init = false)
		{
			Dictionary<string, ClientData> clientDict;

			clientDict = init ? ClientInit : Clients;

			foreach (ClientData u in clientDict.Values.Where(u => Equals(u.RemoteEndPoint, receiver)))
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