using System;

namespace pNetworkStack.Commands
{
	public class UdpServerCommand : Attribute
	{
		private readonly string m_Command;
		
		public UdpServerCommand(string command)
		{
			m_Command = command;
		}

		public string GetCommand()
		{
			return m_Command;
		}
	}
}