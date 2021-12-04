using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using pNetworkStack.Commands;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;

namespace pNetworkStack.Server
{
	public class UDPServer
	{
		private static UDPServer Instance;

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

		internal Action<byte[], User> OnSendRPC;

		/// <summary>
		/// Creates and starts a server on the any available port
		/// </summary>
		/// <returns>An instance of the server</returns>
		public static UDPServer CreateServer()
		{
			return new UDPServer();
		}

		/// <summary>
		/// Creates and starts a server on the specified port
		/// </summary>
		/// <param name="port">the port the server should listen on</param>
		/// <returns>An instance of the server</returns>
		public static UDPServer CreateServer(int port)
		{
			return new UDPServer(port);
		}

		/// <summary>
		/// Creates and starts a server on the specified endpoint
		/// </summary>
		/// <param name="endPoint">The information the server should use</param>
		/// <returns>An instance of the server</returns>
		public static UDPServer CreateServer(IPEndPoint endPoint)
		{
			return new UDPServer(endPoint);
		}


		/// <summary>
		/// Gets the current active and running server instance
		/// </summary>
		/// <returns>Server instance</returns>
		public static UDPServer GetCurrent()
		{
			return Instance;
		}

		private UDPServer()
		{
			new UDPServer(new IPEndPoint(IPAddress.Any, 0));
		}

		private UDPServer(int port)
		{
			new UDPServer(new IPEndPoint(IPAddress.Any, port));
		}

		private UDPServer(IPEndPoint endPoint)
		{
			if (Instance == null)
			{
				Instance = this;

				Debugger.Log("Starting UDP Server...");

				// Prepare the tps handler
				m_TpsHandler = TpsHandler.GetHandler();
				m_TpsHandler.TransfromUpdate += TransfromUpdate;
				m_TpsHandler.FinalUpdate += FinalUpdate;

				// Start receiving data on UDP
				UdpClient udpClient = new UdpClient(endPoint);
				udpClient.BeginReceive(ReceiveCallback, udpClient);

				m_TpsHandler.StartTicker();

				m_IsRunning = true;

				m_CommandHandler = CommandHandler.GetHandler();

				Debugger.Log($"Server started on: {endPoint.Address}:{endPoint.Port}");
			}
		}

		private void FinalUpdate()
		{
		}

		private void TransfromUpdate()
		{
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			// Retrieve the state object and the client socket 
			// from the asynchronous state object
			UdpClient udpClient = (UdpClient)ar.AsyncState;

			// Read data from the remote device.
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 2117);
			byte[] data = udpClient.EndReceive(ar, ref endPoint);

			// Handle the data
			// HandleData(data, endPoint);

			// Log the senders endpoint
			Debugger.Log($"Received data from: {endPoint.Address}:{endPoint.Port}");

			// Start receiving data again
			udpClient.BeginReceive(ReceiveCallback, udpClient);
		}
	}
}