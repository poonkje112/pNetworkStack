using pNetworkStack.Commands;
using pNetworkStack.Core.Data;
using pNetworkStack.Server;

namespace ServerExample
{
	public class ServerCommands
	{
		[ServerCommand("pl_shoot")]
		public void Shoot(User sender, string[] args)
		{
			Server.GetCurrent().SendRPC(sender, $"shoot {sender.UUID}");
		}

		[ServerCommand("pl_hit")]
		public void Hit(User sender, string[] args)
		{
			Server.GetCurrent().Send(Server.GetCurrent().Clients[args[0]].WorkClient, $"damage {args[1]}");
		}
	}
}