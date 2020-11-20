using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace pNetworkStack.Commands
{
	public class CommandHandler
	{
		static CommandHandler Instance;
		Dictionary<string, MethodInfo> m_ServerCommands = new Dictionary<string, MethodInfo>();
		Dictionary<string, MethodInfo> m_ClientCommands = new Dictionary<string, MethodInfo>();

		private CommandHandler()
		{
			IEnumerable<MethodInfo> serverCommandsEnum = GetMethodsWithAttribute(AppDomain.CurrentDomain.GetAssemblies().GetType(), typeof(ServerCommand));

			foreach (MethodInfo methodInfo in serverCommandsEnum)
			{
				ServerCommand serverCommand = (ServerCommand)methodInfo.GetCustomAttribute(typeof(ServerCommand));
				m_ServerCommands.Add(serverCommand.GetCommand(), methodInfo);
			}
			
			IEnumerable<MethodInfo> clientCommandsEnum = GetMethodsWithAttribute(AppDomain.CurrentDomain.GetAssemblies().GetType(), typeof(ClientCommand));
			foreach (MethodInfo methodInfo in clientCommandsEnum)
			{
				ClientCommand clientCommand = (ClientCommand)methodInfo.GetCustomAttribute(typeof(ClientCommand));
				m_ClientCommands.Add(clientCommand.GetCommand(), methodInfo);
			}
		}

		public static CommandHandler GetHandler()
		{
			return Instance ?? (Instance = new CommandHandler());
		}

		public bool ExecuteServerCommand(string command)
		{
			return ExecuteServerCommand(command, null);
		}

		public bool ExecuteServerCommand(string command, object[] args)
		{
			if (!m_ServerCommands.ContainsKey(command) || m_ServerCommands[command].DeclaringType == null) return false;

			m_ServerCommands[command].Invoke(Activator.CreateInstance(m_ServerCommands[command].DeclaringType), args);
			return true;
		}
		
		public bool ExecuteClientCommand(string command)
		{
			return ExecuteClientCommand(command, null);
		}

		public bool ExecuteClientCommand(string command, object[] args)
		{
			if (!m_ClientCommands.ContainsKey(command) || m_ClientCommands[command].DeclaringType == null) return false;

			m_ClientCommands[command].Invoke(Activator.CreateInstance(m_ClientCommands[command].DeclaringType), args);
			return true;
		}
		
		private IEnumerable<MethodInfo> GetMethodsWithAttribute(Type classType, Type attributeType)
		{
			IEnumerable<MethodInfo> methods = AppDomain.CurrentDomain.GetAssemblies() // Returns all currenlty loaded assemblies
				.SelectMany(x => x.GetTypes()) // returns all types defined in this assemblies
				.Where(x => x.IsClass) // only yields classes
				.SelectMany(x => x.GetMethods()) // returns all methods defined in those classes
				.Where(x => x.GetCustomAttributes(attributeType, false).FirstOrDefault() != null); // returns only methods that have the InvokeAttribute

			return methods;
		}
	}
}