using System;
using pNetworkStack.Core;
using pNetworkStack.Server;

namespace ServerExample
{
	class Program
	{
		private static void Main(string[] args)
		{
			InitializeDebugger();

			Server server = Server.CreateServer(4007);

			while (server.IsRunning)
			{
				string message = Console.ReadLine();
				
				if(string.IsNullOrEmpty(message)) continue;
				
				Server.GetCurrent().SendRPC(null, message);
			}
			
			Console.ReadLine();
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