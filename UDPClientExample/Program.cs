using System;
using pNetworkStack.client;
using pNetworkStack.Core;

namespace UDPClientExample
{
	class Program
	{
		static void Main(string[] args)
		{
			InitializeDebugger();
			Client client = Client.CreateClient("127.0.0.1", 2117);
			
			while (client.IsReady)
			{
				string message = Console.ReadLine();
				
				if(string.IsNullOrEmpty(message)) continue;
				
				Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
				Console.WriteLine();
				client.Send(new Packet(message));
				
				if (message.ToLower() == "exit")
				{
					Environment.Exit(0);
				}
			}
			
			Console.ReadKey();
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