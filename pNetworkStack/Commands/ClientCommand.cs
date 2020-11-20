using System;

namespace pNetworkStack.Commands
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ClientCommand : Attribute
	{
		private readonly string m_Command;
		
		public ClientCommand(string command)
		{
			m_Command = command;
		}

		public string GetCommand()
		{
			return m_Command;
		}
	}
}