using System;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;
using pNetworkStack.Server;

namespace ServerExample
{
	class Program
	{
		private static int m_PlayerCount = 0;
		public static Team m_Red, m_Blu;
		
		private static void Main(string[] args)
		{
			InitializeDebugger();
			// x min: 17
			// x max 48
			m_Red = new Team()
			{
				minX = 17,
				maxX = 48,
				Y = 2,
				minZ = -41,
				maxZ = -37
			};
			
			m_Blu = new Team()
			{
				minX = 17,
				maxX = 48,
				Y = 2,
				minZ = -25,
				maxZ = -20
			};

			Server server = Server.CreateServer(4007, ConnectionType.UDP);

			server.OnUserJoined += OnUserJoined;
			server.OnUserLeft += OnUserLeft;

			while (server.IsRunning)
			{
				string message = Console.ReadLine();

				if (string.IsNullOrEmpty(message)) continue;

				Server.GetCurrent().SendRPC(null, new Packet(message));
			}

			Console.ReadLine();
		}

		private static void OnUserLeft(User obj)
		{
			m_PlayerCount--;
			Console.WriteLine($"{obj.Username} has left the server!");
		}

		private static void OnUserJoined(User connectedClient)
		{
			Packet packet = new Packet();
			if (m_PlayerCount + 1 > 2)
			{
				packet.SetCommand(new Command("pl_disconnect").AddArgument("full"));
			}
			else
			{
				Console.WriteLine($"{connectedClient.Username} has joined the server!");
				
				m_PlayerCount++;
				if (m_PlayerCount == 1)
				{
					m_Red.Player = connectedClient;
					packet.SetCommand(new Command($"pl_respawn").AddArgument($"{m_Red.Respawn()}"));
				}
				else
				{
					m_Blu.Player = connectedClient;
					packet.SetCommand(new Command($"pl_respawn").AddArgument($"{m_Blu.Respawn()}"));
				}
			}
			
			Server.GetCurrent().Send(connectedClient, packet);
		}

		private static void InitializeDebugger()
		{
			Debugger.OnInfo += s =>
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(s);
				Console.ForegroundColor = ConsoleColor.White;
			};

			Debugger.OnWarning += s =>
			{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine(s);
				Console.ForegroundColor = ConsoleColor.White;
			};

			Debugger.OnError += s =>
			{
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine(s);
				Console.ForegroundColor = ConsoleColor.White;
			};
		}
	}
}