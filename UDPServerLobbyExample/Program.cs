using System;
using System.Collections.Generic;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;
using pNetworkStack.Server;

namespace UDPServerLobbyExample
{
	class Program
	{
		static void Main(string[] args)
		{
			InitializeDebugger();

			Server server = Server.CreateServer(2117, ConnectionType.UDP);
			server.OnUserJoined += OnUserJoined;
			server.OnUserLeft += OnUserLeft;
		}

		private static void OnUserLeft(User obj)
		{
			Debugger.OnInfo($"User left on endpoint: {obj.LocalEndpoint}");
			LobbyManager.GetLobbyManager().TryUpdateHost(obj);
			LobbyManager.GetLobbyManager().LeaveLobby(obj);
		}

		private static void OnUserJoined(User obj)
		{
			Debugger.OnInfo($"User joined on endpoint: {obj.LocalEndpoint}");
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