using System;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;
using pNetworkStack.Server;
using ServerExample.Physics;

namespace ServerExample
{
	class Program
	{
		private static void Main(string[] args)
		{
			InitializeDebugger();
			
			PhysicsEngine physicsEngine = new PhysicsEngine();
			physicsEngine.InitializePhysicsEngine();
			
			Server server = Server.CreateServer(4007);

			server.OnUserJoined += OnUserJoined;
			server.OnUserLeft += OnUserLeft;
			
			while (server.IsRunning)
			{
				string message = Console.ReadLine();
				
				if(string.IsNullOrEmpty(message)) continue;
				
				Server.GetCurrent().SendRPC(null, message);
			}
			
			Console.ReadLine();
		}

		private static void OnUserLeft(User obj)
		{
			Console.WriteLine($"{obj.Username} has left the server!");
		}

		private static void OnUserJoined(User obj)
		{
			Console.WriteLine($"{obj.Username} has joined the server!");
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
			
			ServerDebugger.OnInfo += s =>
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(s);
				Console.ForegroundColor = ConsoleColor.White;
			};

			ServerDebugger.OnWarning += s =>
			{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine(s);
				Console.ForegroundColor = ConsoleColor.White;
			};

			ServerDebugger.OnError += s =>
			{
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine(s);
				Console.ForegroundColor = ConsoleColor.White;
			};
		}
	}
}