using System;
using pNetworkStack.Core.Data;

namespace pNetworkStack.Core
{
	[Serializable]
	public class Command
	{
		private string m_Command;
		private object[] m_Arguments;

		public string CommandName => m_Command;

		public Command(string command)
		{
			m_Command = command;
		}

		public Command(string command, object[] args)
		{
			m_Command = command;
			m_Arguments = args;
		}

		public Command AddArgument(object arg)
		{
			// Add argument to the end of the array
			if (m_Arguments == null)
				m_Arguments = new object[1];
			else
				Array.Resize(ref m_Arguments, m_Arguments.Length + 1);
			
			m_Arguments[m_Arguments.Length - 1] = arg;

			return this;
		}

		public object[] GetArguments()
		{
			return m_Arguments;
		}

		public void AddSender(User sender)
		{
			// Add sender in the beginning of the array
			if (m_Arguments == null)
				m_Arguments = new object[1];
			else
				Array.Resize(ref m_Arguments, m_Arguments.Length + 1);
			
			// Move all arguments one position to the right
			for (int i = m_Arguments.Length - 1; i > 0; i--)
				m_Arguments[i] = m_Arguments[i - 1];
			
			m_Arguments[0] = sender;
		}
	}
}