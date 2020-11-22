using System;
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
		public void Say(Socket sender, string[] args)
		{
			if (args == null || args.Length <= 0) return;
			
			Server.GetCurrent().SendRPC(sender, "say " + Util.Join(' ', args));
		}

		[ServerCommand("pl_init")]
		public void InitNewPlayer(Socket sender, string[] args)
		{
			// Converting the sent data back to a User object
			User data = JsonConvert.DeserializeObject<User>(Util.Join(' ', args));
			
			// Updating the User data of the sender
			Server.GetCurrent().ClientInit[data.UUID].UserData = data;

			// Telling everyone a new player has joined
			Server.GetCurrent().SendRPC(sender, $"pl_add {JsonConvert.SerializeObject(data)}");

			// Queue the new player to be taken into the update loop in the next tick
			Server.GetCurrent().AddClientQueue.Enqueue(new Tuple<string, ClientData>(data.UUID, Server.GetCurrent().ClientInit[data.UUID]));
			Server.GetCurrent().ClientInit.Remove(data.UUID);
		}
		
		[ServerCommand("pl_update_position")]
		public void UpdatePlayerPosition(Socket sender, string[] args)
		{
			string uid = args[0];
			pVector pos = pVector.StringToPVector(args[1]);

			Server.GetCurrent().Clients[uid].UserData.UpdatePosition(pos);
		}

		[ServerCommand("pl_update_euler")]
		public void UpdatePlayerEuler(Socket sender, string[] args)
		{
			string uid = args[0];
			pVector euler = pVector.StringToPVector(args[1]);
			
			Server.GetCurrent().Clients[uid].UserData.UpdateEuler(euler);
		}

		[ServerCommand("pl_disconnect")]
		public void DisconnectPlayer(Socket sender, string[] args)
		{
			Server.GetCurrent().DisconnectClient(args[0]);
		}
	}
}