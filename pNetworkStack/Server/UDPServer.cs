using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

		private UdpClient m_Server;
		
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

				m_Server = new UdpClient(endPoint);

				// Start receiving data on UDP
				ConnectionInfo connectionInfo = new ConnectionInfo()
				{
					UdpClient = m_Server,
					EndPoint = endPoint,
					Type = ConnectionType.UDP
				};

				m_Server.BeginReceive(ReceiveCallback, connectionInfo);

				m_TpsHandler.StartTicker();

				m_IsRunning = true;

				m_CommandHandler = CommandHandler.GetHandler();

				Debugger.Log($"Server started on: {endPoint.Address}:{endPoint.Port}");
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			ConnectionInfo connectionInfo = (ConnectionInfo)ar.AsyncState;

			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 2117);
			byte[] data = connectionInfo.UdpClient.EndReceive(ar, ref endPoint);

			// Log the client connection
			Debugger.Log($"Data received from: {endPoint.Address}:{endPoint.Port}");

			// Convert the data back to a string
			string content = Encoding.ASCII.GetString(data);

			// Check if the data is fully received if not we simply skip it
			if (content.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
			{
				content = content.Substring(0, content.IndexOf("<EOF>", StringComparison.Ordinal));

				Util.ParseUdpCommand(endPoint, content,
					(command, args) => { CommandHandler.GetHandler().ExecuteUdpServerCommand(command, args); });
			}

			// Start receiving data on UDP
			connectionInfo.UdpClient.BeginReceive(ReceiveCallback, connectionInfo);
		}

		private void FinalUpdate()
		{
		}

		private void TransfromUpdate()
		{
		}

		public void Send(byte[] data, string endPoint)
		{
			m_Server.Send(data, data.Length, Util.GetIpEndPoint(endPoint));
		}
	}
}