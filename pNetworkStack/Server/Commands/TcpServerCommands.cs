using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Newtonsoft.Json;
using pNetworkStack.Commands;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;

namespace pNetworkStack.Server.Commands
{
	public class TcpServerCommands
	{
		private Connections m_Connections = Connections.Instance;
		
		[TcpServerCommand("say")]
		public void Say(User sender, string[] args)
		{
			if (args == null || args.Length <= 0) return;

			TCPServer.GetCurrent().SendRPC(sender, "say " + Util.Join(' ', args));
		}

		[TcpServerCommand("pl_init")]
		public void InitNewPlayer(User sender, string[] args)
		{
			// Converting the sent data back to a User object
			User data = JsonConvert.DeserializeObject<User>(Util.Join(' ', args));

			// Updating the User data of the sender
			TCPServer.GetCurrent().ClientInit[data.UUID].UserData = data;

			// Telling everyone a new player has joined
			TCPServer.GetCurrent().SendRPC(sender, $"pl_add {JsonConvert.SerializeObject(data)}");

			// Queue the new player to be taken into the update loop in the next tick
			TCPServer.GetCurrent().AddClientQueue
				.Enqueue(new Tuple<string, ClientData>(data.UUID, TCPServer.GetCurrent().ClientInit[data.UUID]));
			TCPServer.GetCurrent().ClientInit.Remove(data.UUID);
		}

		[TcpServerCommand("pl_update_position")]
		public void UpdatePlayerPosition(User sender, string[] args)
		{
			string uid = sender.UUID;

			pVector pos = pVector.StringToPVector(args[0]);

			m_Connections.Clients[uid].UserData.UpdatePosition(pos);

			User u = m_Connections.Clients[uid].UserData;

			TCPServer.GetCurrent().SendRPC(sender, $"pl_update_position {uid} {u.GetPosition()}");
		}

		[TcpServerCommand("pl_update_euler")]
		public void UpdatePlayerEuler(User sender, string[] args)
		{
			string uid = sender.UUID;

			pVector euler = pVector.StringToPVector(args[0]);

			m_Connections.Clients[uid].UserData.UpdateEuler(euler);

			User u = m_Connections.Clients[uid].UserData;

			TCPServer.GetCurrent().SendRPC(sender, $"pl_update_euler {u.UUID} {u.GetEuler()}");
		}
		
		[TcpServerCommand("pl_update_transform")]
		public void UpdateTransform(User sender, string[] args)
		{
			string uid = sender.UUID;

			pVector position = pVector.StringToPVector(args[0]);
			pVector euler = pVector.StringToPVector(args[1]);

			m_Connections.Clients[uid].UserData.UpdatePosition(position);
			m_Connections.Clients[uid].UserData.UpdateEuler(euler);

			User u = m_Connections.Clients[uid].UserData;

			TCPServer.GetCurrent().SendRPC(sender, $"pl_update_transform {u.UUID} {u.GetPosition()} {u.GetEuler()}");
		}

		[TcpServerCommand("pl_disconnect")]
		public void DisconnectPlayer(User sender, string[] args)
		{
			TCPServer.GetCurrent().DisconnectClient(sender.UUID);
		}

		[TcpServerCommand("sync_list")]
		public void SyncList(User sender, string[] args)
		{
			List<User> users = new List<User>();

			foreach (ClientData clientsValue in m_Connections.Clients.Values)
			{
				if (clientsValue.UserData.UUID == sender.UUID) continue;

				users.Add(clientsValue.UserData);
			}

			TCPServer.GetCurrent().Send(m_Connections.Clients[sender.UUID].WorkClient,
				$"sync_list {JsonConvert.SerializeObject(users.ToArray())}");
		}
	}
}