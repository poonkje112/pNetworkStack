using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace pNetworkStack.Commands
{
	public class CommandHandler
	{
		static CommandHandler _Instance;
		Dictionary<string, MethodInfo> _ServerCommands = new Dictionary<string, MethodInfo>();
		Dictionary<string, MethodInfo> _ClientCommands = new Dictionary<string, MethodInfo>();

		private CommandHandler()
		{
			IEnumerable<MethodInfo> serverCommandsEnum = GetMethodsWithAttribute(AppDomain.CurrentDomain.GetAssemblies().GetType(), typeof(ServerCommand));

			foreach (MethodInfo methodInfo in serverCommandsEnum)
			{
				ServerCommand serverCommand = (ServerCommand)methodInfo.GetCustomAttribute(typeof(ServerCommand));
				_ServerCommands.Add(serverCommand.GetCommand(), methodInfo);
			}
			
			IEnumerable<MethodInfo> clientCommandsEnum = GetMethodsWithAttribute(AppDomain.CurrentDomain.GetAssemblies().GetType(), typeof(ClientCommand));
			foreach (MethodInfo methodInfo in clientCommandsEnum)
			{
				ClientCommand clientCommand = (ClientCommand)methodInfo.GetCustomAttribute(typeof(ClientCommand));
				_ClientCommands.Add(clientCommand.GetCommand(), methodInfo);
			}
		}
		
		IEnumerable<MethodInfo> GetMethodsWithAttribute(Type classType, Type attributeType)
		{
			IEnumerable<MethodInfo> methods = AppDomain.CurrentDomain.GetAssemblies() // Returns all currenlty loaded assemblies
				.SelectMany(x => x.GetTypes()) // returns all types defined in this assemblies
				.Where(x => x.IsClass) // only yields classes
				.SelectMany(x => x.GetMethods()) // returns all methods defined in those classes
				.Where(x => x.GetCustomAttributes(attributeType, false).FirstOrDefault() != null); // returns only methods that have the InvokeAttribute

			return methods;
		}

		public static CommandHandler GetHandler()
		{
			if (_Instance == null)
				_Instance = new CommandHandler();

			return _Instance;
		}

		public bool ExecuteServerCommand(string command)
		{
			return ExecuteServerCommand(command, null);
		}

		public bool ExecuteServerCommand(string command, object[] args)
		{
			if (!_ServerCommands.ContainsKey(command) || _ServerCommands[command].DeclaringType == null) return false;

			_ServerCommands[command].Invoke(Activator.CreateInstance(_ServerCommands[command].DeclaringType), args);
			return true;
		}
		
		public bool ExecuteClientCommand(string command)
		{
			return ExecuteClientCommand(command, null);
		}

		public bool ExecuteClientCommand(string command, object[] args)
		{
			if (!_ClientCommands.ContainsKey(command) || _ClientCommands[command].DeclaringType == null) return false;

			_ClientCommands[command].Invoke(Activator.CreateInstance(_ClientCommands[command].DeclaringType), args);
			return true;
		}
	}
}