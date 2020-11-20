using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace pNetworkStack.Core
{
	internal static class Util
	{
		/// <summary>
		/// Converts a string array to one string
		/// </summary>
		/// <param name="seperator">the character that needs to go between each element</param>
		/// <param name="data">the array of a string</param>
		/// <returns>The combined string</returns>
		internal static string Join(char seperator, string[] data)
		{
			if (data == null || data.Length <= 0) return "";
			
			string output = "";
			foreach (string s in data)
			{
				output += s;
				output += seperator;
			}

			output = output.Remove(output.Length - 1);
			
			return output;
		}

		/// <summary>
		/// Gets the string and process it into a command with parameters
		/// </summary>
		/// <param name="message">The message that needs to get parsed</param>
		/// <param name="callback">The command processor</param>
		internal static void ParseCommand(string message, Action<string, object[]> callback)
		{
			ParseCommand(null, message, callback);
		}

		/// <summary>
		/// Gets the string and process it into a command with parameters
		/// </summary>
		/// <param name="sender">The socket that sent this command</param>
		/// <param name="message">The message that needs to get parsed</param>
		/// <param name="callback">The command processor</param>
		internal static void ParseCommand(Socket sender, string message, Action<string, object[]> callback)
		{
			List<object> param = new List<object>();
			if(sender != null) param.Add(sender);

			List<string> data = message.Split(' ').ToList();
			
			string command = data[0];

			data.RemoveAt(0);
			data.ForEach((s) => param.Add(s));
			
			callback.Invoke(command, param.ToArray());
		}
	}
}