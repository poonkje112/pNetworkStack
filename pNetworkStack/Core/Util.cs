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
		/// <param name="separator">the character that needs to go between each element</param>
		/// <param name="data">the array of a string</param>
		/// <returns>The combined string</returns>
		internal static string Join(char separator, string[] data)
		{
			// Check if there is data available if not just return an empty string instead of null
			if (data == null || data.Length <= 0) return "";
			
			// Loop through all elements and add them to the output including the separator character
			string output = "";
			foreach (string s in data)
			{
				output += s;
				output += separator;
			}
			
			// Remove the last separator character
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
			
			// Check if the sender is available, if so we add it as the first parameter
			if(sender != null) param.Add(sender);
			
			// Split every word by space and temporarily store it
			List<string> data = message.Split(' ').ToList();
			
			// The first element is always the command
			string command = data[0];

			// Remove the first element
			data.RemoveAt(0);
			
			// Loop through all elements and add the item to the param list
			data.ForEach((s) => param.Add(s));
			
			// Invoke the callback and convert our param list to an object array
			callback.Invoke(command, param.ToArray());
		}
	}
}