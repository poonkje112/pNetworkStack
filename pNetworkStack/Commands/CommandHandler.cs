using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using pNetworkStack.Core;

namespace pNetworkStack.Commands
{
	public class CommandHandler
	{
		private static CommandHandler Instance;
		private Dictionary<string, MethodInfo> m_ServerCommands = new Dictionary<string, MethodInfo>();
		private Dictionary<string, MethodInfo> m_ClientCommands = new Dictionary<string, MethodInfo>();

		private CommandHandler()
		{
			IEnumerable<MethodInfo> serverCommandsEnum = GetMethodsWithAttribute(typeof(ServerCommand));

			foreach (MethodInfo methodInfo in serverCommandsEnum)
			{
				ServerCommand serverCommand = (ServerCommand)methodInfo.GetCustomAttribute(typeof(ServerCommand));
				m_ServerCommands.Add(serverCommand.GetCommand(), methodInfo);
			}
			
			IEnumerable<MethodInfo> clientCommandsEnum = GetMethodsWithAttribute(typeof(ClientCommand));
			foreach (MethodInfo methodInfo in clientCommandsEnum)
			{
				ClientCommand clientCommand = (ClientCommand)methodInfo.GetCustomAttribute(typeof(ClientCommand));
				m_ClientCommands.Add(clientCommand.GetCommand(), methodInfo);
			}
		}

		/// <summary>
		/// Get an instance of the CommandHandler
		/// </summary>
		/// <returns>An active instance of the CommandHandler</returns>
		public static CommandHandler GetHandler()
		{
			return Instance ?? (Instance = new CommandHandler());
		}

		/// <summary>
		/// Execute a server registered command using the [ServerCommand(commandName)] Attribute
		/// </summary>
		/// <param name="command">The command that you want to use</param>
		/// <returns>If the command was found and executed</returns>
		public bool ExecuteServerCommand(Command command)
		{
			try
			{
				if (!m_ServerCommands.ContainsKey(command.CommandName) || m_ServerCommands[command.CommandName].DeclaringType == null)
					return false;

				m_ServerCommands[command.CommandName]
					.Invoke(Activator.CreateInstance(m_ServerCommands[command.CommandName].DeclaringType), command.GetArguments());
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		/// <summary>
		/// Execute a client registered command using the [ClientCommand(commandName)] Attribute
		/// </summary>
		/// <param name="command">The command that you want to use</param>
		/// <returns>If the command was found and executed</returns>
		public bool ExecuteClientCommand(Command command)
		{
			try
			{
				if (!m_ClientCommands.ContainsKey(command.CommandName) || m_ClientCommands[command.CommandName].DeclaringType == null)
					return false;

				m_ClientCommands[command.CommandName]
					.Invoke(Activator.CreateInstance(m_ClientCommands[command.CommandName].DeclaringType), command.GetArguments());
				return true;
			}
			catch (Exception e)
			{
				Debugger.Log($"Was processing:\n{command}\n{command.GetArguments()}");
				Debugger.Log(e.Message, LogType.Error);
				return false;
			}
		}
		
		/// <summary>
		/// Get all methods registered with the specified attribute
		/// </summary>
		/// <param name="attributeType">The attribute you want to find</param>
		/// <returns>IEnumerable of all methods with the specified attribute</returns>
		private IEnumerable<MethodInfo> GetMethodsWithAttribute(Type attributeType)
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