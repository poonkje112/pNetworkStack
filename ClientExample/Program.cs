using System;
using pNetworkStack.client;
using pNetworkStack.Core;

namespace ClientExample
{
	class Program
	{
		static void Main(string[] args)
		{
			InitializeDebugger();

			Client client = new Client( "127.0.0.1", 4007);

			while (client.IsConnected)
			{
				string message = Console.ReadLine();
				
				if(string.IsNullOrEmpty(message)) continue;
				
				Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
				client.Send(message);
				
				if (message.ToLower() == "exit")
				{
					Environment.Exit(0);
				}
			}
			
			Console.ReadKey();
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