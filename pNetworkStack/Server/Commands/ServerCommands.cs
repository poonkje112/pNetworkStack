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
			User data = JsonConvert.DeserializeObject<User>(Util.Join(' ', args));

			Server.GetCurrent().Clients[data.UUID].UserData = data;
			
			Server.GetCurrent().SendRPC(sender, $"pl_add {JsonConvert.SerializeObject(data)}");
			foreach (ClientData d in Server.GetCurrent().Clients.Values)
			{
				if(d.UserData.UUID == data.UUID) continue;
				Server.GetCurrent().Send(sender, $"pl_add {JsonConvert.SerializeObject(d.UserData)}");
			}
		}
		
		[ServerCommand("pl_update_position")]
		public void UpdatePlayerPosition(string[] args)
		{
			string uid = args[0];
			pVector pos = pVector.StringToPVector(args[1]);

			Server.GetCurrent().Clients[uid].UserData.UpdatePosition(pos);
		}

		[ServerCommand("pl_update_euler")]
		public void UpdatePlayerEuler(string[] args)
		{
			string uid = args[0];
			pVector euler = pVector.StringToPVector(args[1]);
			
			Server.GetCurrent().Clients[uid].UserData.UpdateEuler(euler);
		}

		[ServerCommand("pl_disconnect")]
		public void DisconnectPlayer(string[] args)
		{
			Server.GetCurrent().DisconnectClient(args[0]);
		}
	}
}