using System;

namespace pNetworkStack.Commands
{
	[AttributeUsage(AttributeTargets.Method)]
	public class TcpServerCommand : Attribute
	{
		private readonly string m_Command;
		
		public TcpServerCommand(string command)
		{
			m_Command = command;
		}

		public string GetCommand()
		{
			return m_Command;
		}
	}
}