using System;
using System.Threading;

namespace UDPHolePunchTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Thread server = new Thread(() =>
			{
				Server server = new Server();
				server.Start(2117);

				Console.WriteLine("Server has started!");
				while (server.IsRunning)
				{
					// Do nothing
				}
			});
			
			server.Start();
		}
	}
}