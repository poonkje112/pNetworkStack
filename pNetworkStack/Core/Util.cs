using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using pNetworkStack.Commands;
using pNetworkStack.Core.Data;

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
			if (data == null || data.Length <= 0) return string.Empty;

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
		internal static void ParseCommand(User sender, string message, Action<string, object[]> callback)
		{
			List<object> param = new List<object>();

			// Check if the sender is available, if so we add it as the first parameter
			if (sender != null) param.Add(sender);

			// Split every word by space and temporarily store it
			List<string> data = message.Split(' ').ToList();

			// The first element is always the command
			string command = data[0];
			data.RemoveAt(0);

			// The all other elements are parameters so add this as a string array
			param.Add(data.ToArray());

			// Invoke the callback and convert our param list to an object array
			callback.Invoke(command, param.ToArray());
		}

		internal static IPEndPoint GetIpEndPoint(string endPoint)
		{
			// Convert the string to an IPEndPoint
			IPEndPoint ipEndPoint = null;

			// Check if the string is a valid IPEndPoint
			if (IPAddress.TryParse(endPoint, out IPAddress ipAddress))
			{
				// Check if the string is a valid IPEndPoint
				if (int.TryParse(endPoint.Split(':')[1], out int port))
				{
					// Create a new IPEndPoint
					ipEndPoint = new IPEndPoint(ipAddress, port);
				}
			}

			return ipEndPoint;
		}

		// TODO Implement compression and decompression
		internal static byte[] Compress(byte[] data)
		{
			return data;
		}

		internal static byte[] Decompress(byte[] data)
		{
			return data;
		}

		internal static string SafeString(string data)
		{
			// Escape the command to prevent injection
			data = data.Replace("|", "\\|");

			// Remove the headers if they exist in the command
			data = data.Replace(Encoding.ASCII.GetString(Packet.BeginHeader), String.Empty);
			data = data.Replace(Encoding.ASCII.GetString(Packet.EndHeader), String.Empty);

			return data;
		}

		internal static byte[] GetBytes(object target)
		{
			if (target == null)
				return null;

			byte[] result = null;

			// Convert the object to bytes
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream())
			{
				bf.Serialize(ms, target);
				result = ms.ToArray();
			}

			return result;
		}

		internal static T ConvertBytesToObject<T>(byte[] data)
		{
			// Return the object from the bytes
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream(data))
			{
				return (T)bf.Deserialize(ms);
			}
		}

		/// <summary>
		/// This finds the exact match of bytes in the source and returns it starting index
		/// </summary>
		/// <param name="source">The source you want to find the part in</param>
		/// <param name="part">The part you want to find</param>
		/// <param name="startIndex">The start</param>
		/// <returns>The start index of the part position in the source</returns>
		internal static int FindPart(byte[] source, byte[] part, int startIndex)
		{
			int index = -1;
			bool found = false;

			for (int i = startIndex; i < source.Length; i++)
			{
				if (i + part.Length > source.Length)
					break;

				for (int j = 0; j < part.Length; j++)
				{
					if (source[i + j] == part[j])
					{
						index = found ? index : i + j;
						found = true;
					}
					else
					{
						index = -1;
						found = false;
					}
				}

				if (found)
					break;
			}

			return index;
		}

		internal static void ProcessData(ref ClientData client, int bytesToRead, Action<Command> callback)
		{
			ProcessData(ref client, bytesToRead, false, callback);
		}
		
		internal static void ProcessData(ref ClientData client, int bytesToRead, bool addSender, Action<Command> callback)
		{
			// Check if there is any data to process
			if (bytesToRead > 0)
			{
				if (client.PushData(client.Buffer, bytesToRead))
				{
					Packet p = client.PopPacket();

					if (p == null)
					{
						// Assume the packet was corrupted so we just drop it and clear the buffer
						client.ClearData();
						return;
					}

					if(addSender)
						p.Command.AddSender(client.UserData);

					callback.Invoke(p.Command);

					client.ClearData();
				}
			}
		}
	}
}