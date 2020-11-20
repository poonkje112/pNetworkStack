using System;

namespace pNetworkStack.Commands
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ClientCommand : Attribute
	{
		string _Command;
		
		public ClientCommand(string command)
		{
			_Command = command;
		}

		public string GetCommand()
		{
			return _Command;
		}
	}
}