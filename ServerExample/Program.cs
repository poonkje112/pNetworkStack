using System;
using pNetworkStack.Core;
using pNetworkStack.Server;

namespace ServerExample
{
	class Program
	{
		static int count = 0;

		static void Main(string[] args)
		{
			InitializeDebugger();

			Server server = Server.CreateServer(4007);

			while (server.IsRunning)
			{
				string cmd = Console.ReadLine();
				if (Server.GetCurrent() == null)
				{
					Console.WriteLine("ERROR!");
					continue;
				}
				Server.GetCurrent().SendRPC(null, cmd);
			}


			Console.ReadLine();
		}

		static void InitializeDebugger()
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