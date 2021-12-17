using pNetworkStack.Commands;
using pNetworkStack.Core;
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
			// Server.GetCurrent().Send(sender, $"damage {args[1]}");
			Server.GetCurrent().Send(sender, new Packet($"damage {args[1]}"));
		}

		[ServerCommand("pl_respawn")]
		public void Respawn(User sender, string[] args)
		{
			if (Program.m_Red.Player.Equals(sender))
			{
				// Server.GetCurrent().Send(sender, $"pl_respawn {Program.m_Red.Respawn()}");
				Server.GetCurrent().Send(sender, new Packet($"pl_respawn {Program.m_Red.Respawn()}"));
			}
			else
			{
				// Server.GetCurrent().Send(sender, $"pl_respawn {Program.m_Blu.Respawn()}");
				Server.GetCurrent().Send(sender, new Packet($"pl_respawn {Program.m_Blu.Respawn()}"));
			}
		}
	}
}