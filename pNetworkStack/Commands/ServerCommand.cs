using System;

namespace pNetworkStack.Commands
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ServerCommand : Attribute
	{
		string _Command;
		
		public ServerCommand(string command)
		{
			_Command = command;
		}

		public string GetCommand()
		{
			return _Command;
		}
	}
}