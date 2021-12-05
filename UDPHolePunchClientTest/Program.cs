using System;
using System.Net;
using System.Threading;
using UDPHolePunchTest;

namespace UDPHolePunchClientTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Thread client = new Thread(() =>
			{
				Client client = new Client();

				client.Connect("127.0.0.1", 2117, false);

				while (true)
				{
					string message = Console.ReadLine();

					if (message == "host")
					{
						client.Host();
						Console.WriteLine("Hosting...");

						continue;
					}

					client.Send(message);
				}
			});

			client.Start();
		}
	}
}