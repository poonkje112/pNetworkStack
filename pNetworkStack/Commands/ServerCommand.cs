using System;

namespace pNetworkStack.Commands
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ServerCommand : Attribute
	{
		private readonly string m_Command;
		
		public ServerCommand(string command)
		{
			m_Command = command;
		}

		public string GetCommand()
		{
			return m_Command;
		}
	}
}