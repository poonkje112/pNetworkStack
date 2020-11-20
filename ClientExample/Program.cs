using System;
using System.Collections.Generic;
using pNetworkStack.client;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;

namespace ClientExample
{
	class Program
	{
		static Dictionary<string, User> ConnectedUsers = new Dictionary<string, User>();
		private static void Main(string[] args)
		{
			InitializeDebugger();
			RegisterEvents();

			Console.Write("Username: ");
			string username = Console.ReadLine();

			Client.Username = username;
			
			Client client = Client.CreateClient( "127.0.0.1", 4007);

			while (client.IsConnected)
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

		public static void RegisterEvents()
		{
			Client.OnMessageReceived += (user, message) =>
			{
				Console.WriteLine($"{user.Username}: {message}");
			};

			Client.OnUserJoined += user =>
			{
				Console.WriteLine($"{user.Username} has joined the game!");
				ConnectedUsers.Add(user.UUID, user);
			};
			
			Client.OnUserLeft += uid =>
			{
				Console.WriteLine($"{ConnectedUsers[uid].Username} has left the game!");
				ConnectedUsers.Remove(uid);
			};
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