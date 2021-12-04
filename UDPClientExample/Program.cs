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
			P2PUdpClient client = P2PUdpClient.CreateClient();
			
			while (client.IsReady)
			{
				string message = Console.ReadLine();
				
				if(string.IsNullOrEmpty(message)) continue;
				
				Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
				Console.WriteLine();
				client.Send(message);
				
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