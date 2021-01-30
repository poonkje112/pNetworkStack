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
	public class ServerCommands
	{
		[ServerCommand("say")]
		public void Say(User sender, string[] args)
		{
			if (args == null || args.Length <= 0) return;

			Server.GetCurrent().SendRPC(sender, "say " + Util.Join(' ', args));
		}

		[ServerCommand("pl_init")]
		public void InitNewPlayer(User sender, string[] args)
		{
			// Converting the sent data back to a User object
			User data = JsonConvert.DeserializeObject<User>(Util.Join(' ', args));

			// Updating the User data of the sender
			Server.GetCurrent().ClientInit[data.UUID].UserData = data;

			// Telling everyone a new player has joined
			Server.GetCurrent().SendRPC(sender, $"pl_add {JsonConvert.SerializeObject(data)}");

			// Queue the new player to be taken into the update loop in the next tick
			Server.GetCurrent().AddClientQueue
				.Enqueue(new Tuple<string, ClientData>(data.UUID, Server.GetCurrent().ClientInit[data.UUID]));
			Server.GetCurrent().ClientInit.Remove(data.UUID);
		}

		[ServerCommand("pl_update_position")]
		public void UpdatePlayerPosition(User sender, string[] args)
		{
			string uid = sender.UUID;

			pVector pos = pVector.StringToPVector(args[0]);

			Server.GetCurrent().iClients[uid].UserData.UpdatePosition(pos);

			User u = Server.GetCurrent().iClients[uid].UserData;

			Server.GetCurrent().SendRPC(sender, $"pl_update_position {uid} {u.GetPosition()}");
		}

		[ServerCommand("pl_update_euler")]
		public void UpdatePlayerEuler(User sender, string[] args)
		{
			string uid = sender.UUID;

			pVector euler = pVector.StringToPVector(args[0]);

			Server.GetCurrent().iClients[uid].UserData.UpdateEuler(euler);

			User u = Server.GetCurrent().iClients[uid].UserData;

			Server.GetCurrent().SendRPC(sender, $"pl_update_euler {u.UUID} {u.GetEuler()}");
		}
		
		[ServerCommand("pl_update_transform")]
		public void UpdateTransform(User sender, string[] args)
		{
			string uid = sender.UUID;

			pVector position = pVector.StringToPVector(args[0]);
			pVector euler = pVector.StringToPVector(args[1]);

			Server.GetCurrent().iClients[uid].UserData.UpdatePosition(position);
			Server.GetCurrent().iClients[uid].UserData.UpdateEuler(euler);

			User u = Server.GetCurrent().iClients[uid].UserData;

			Server.GetCurrent().SendRPC(sender, $"pl_update_transform {u.UUID} {u.GetPosition()} {u.GetEuler()}");
		}

		[ServerCommand("pl_disconnect")]
		public void DisconnectPlayer(User sender, string[] args)
		{
			Server.GetCurrent().DisconnectClient(sender.UUID);
		}

		[ServerCommand("sync_list")]
		public void SyncList(User sender, string[] args)
		{
			List<User> users = new List<User>();

			foreach (ClientData clientsValue in Server.GetCurrent().iClients.Values)
			{
				if (clientsValue.UserData.UUID == sender.UUID) continue;

				users.Add(clientsValue.UserData);
			}

			Server.GetCurrent().Send(Server.GetCurrent().iClients[sender.UUID].WorkClient,
				$"sync_list {JsonConvert.SerializeObject(users.ToArray())}");
		}
	}
}