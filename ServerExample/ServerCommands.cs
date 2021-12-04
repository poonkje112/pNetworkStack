using pNetworkStack.Commands;
using pNetworkStack.Core.Data;
using pNetworkStack.Server;

namespace ServerExample
{
	public class ServerCommands
	{
		[TcpServerCommand("pl_shoot")]
		public void Shoot(User sender, string[] args)
		{
			TCPServer.GetCurrent().SendRPC(sender, $"shoot {sender.UUID}");
		}

		[TcpServerCommand("pl_hit")]
		public void Hit(User sender, string[] args)
		{
			TCPServer.GetCurrent().Send(sender, $"damage {args[1]}");
		}

		[TcpServerCommand("pl_respawn")]
		public void Respawn(User sender, string[] args)
		{
			if (Program.m_Red.Player.Equals(sender))
			{
				TCPServer.GetCurrent().Send(sender, $"pl_respawn {Program.m_Red.Respawn()}");
			}
			else
			{
				TCPServer.GetCurrent().Send(sender, $"pl_respawn {Program.m_Blu.Respawn()}");
			}
		}
	}
}