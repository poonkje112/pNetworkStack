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
		private Dictionary<string, MethodInfo> m_TcpServerCommands = new Dictionary<string, MethodInfo>();
		private Dictionary<string, MethodInfo> m_UdpServerCommands = new Dictionary<string, MethodInfo>();
		private Dictionary<string, MethodInfo> m_ClientCommands = new Dictionary<string, MethodInfo>();

		private CommandHandler()
		{
			IEnumerable<MethodInfo> tcpServerCommandsEnum = GetMethodsWithAttribute(typeof(TcpServerCommand));
			foreach (MethodInfo methodInfo in tcpServerCommandsEnum)
			{
				TcpServerCommand tcpServerCommand = (TcpServerCommand)methodInfo.GetCustomAttribute(typeof(TcpServerCommand));
				m_TcpServerCommands.Add(tcpServerCommand.GetCommand(), methodInfo);
			}
			
			IEnumerable<MethodInfo> udpServerCommandsEnum = GetMethodsWithAttribute(typeof(UdpServerCommand));
			foreach (MethodInfo methodInfo in udpServerCommandsEnum)
			{
				UdpServerCommand udpServerCommand = (UdpServerCommand)methodInfo.GetCustomAttribute(typeof(UdpServerCommand));
				m_UdpServerCommands.Add(udpServerCommand.GetCommand(), methodInfo);
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
		/// <param name="args">The parameters that come with it</param>
		/// <returns>If the command was found and executed</returns>
		public bool ExecuteTcpServerCommand(string command, object[] args = null)
		{
			try
			{
				if (!m_TcpServerCommands.ContainsKey(command) || m_TcpServerCommands[command].DeclaringType == null)
					return false;

				m_TcpServerCommands[command]
					.Invoke(Activator.CreateInstance(m_TcpServerCommands[command].DeclaringType), args);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
		
		/// <summary>
		/// Execute a server registered command using the [ServerCommand(commandName)] Attribute
		/// </summary>
		/// <param name="command">The command that you want to use</param>
		/// <param name="args">The parameters that come with it</param>
		/// <returns>If the command was found and executed</returns>
		public bool ExecuteUdpServerCommand(string command, object[] args = null)
		{
			try
			{
				if (!m_UdpServerCommands.ContainsKey(command) || m_UdpServerCommands[command].DeclaringType == null)
					return false;

				m_UdpServerCommands[command]
					.Invoke(Activator.CreateInstance(m_UdpServerCommands[command].DeclaringType), args);
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
		/// <param name="args">The parameters that come with it</param>
		/// <returns>If the command was found and executed</returns>
		public bool ExecuteClientCommand(string command, object[] args = null)
		{
			try
			{
				if (!m_ClientCommands.ContainsKey(command) || m_ClientCommands[command].DeclaringType == null)
					return false;

				m_ClientCommands[command]
					.Invoke(Activator.CreateInstance(m_ClientCommands[command].DeclaringType), args);
				return true;
			}
			catch (Exception e)
			{
				Debugger.Log($"Was processing:\n{command}\n{args}");
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