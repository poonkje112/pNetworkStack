using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace pNetworkStack.Core
{
	public class Packet
	{
		public static readonly byte[] BeginHeader = Encoding.ASCII.GetBytes("<Packet>");
		public static readonly byte[] EndHeader = Encoding.ASCII.GetBytes("</Packet>");
		private static readonly byte[] Seperator = Encoding.ASCII.GetBytes("|");
		
		private string m_Command;
		private byte[] m_Data;

		public string Command => m_Command;
		
		public void SetData(object data)
		{
			if (data == null)
				return;
			
			// Convert the object to bytes
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream())
			{
				bf.Serialize(ms, data);
				m_Data = ms.ToArray();
			}
		}
		
		public T GetData<T>()
		{
			// Return the object from the bytes
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream(m_Data))
			{
				return (T)bf.Deserialize(ms);
			}
		}
		
		public void SetCommand(string command)
		{
			m_Command = command;
		}

		public byte[] SerializePacket()
		{
			byte[] command = Encoding.ASCII.GetBytes(m_Command);
			byte[] data = m_Data;
			
			byte[] packet = new byte[BeginHeader.Length + EndHeader.Length + command.Length + data.Length + (Seperator.Length * 3)];

			int index = 0;
			
			// Begin header
			Array.Copy(BeginHeader, 0, packet, index, BeginHeader.Length);
			index += BeginHeader.Length;
			
			// Separator
			Array.Copy(Seperator, 0, packet, index, Seperator.Length);
			index += Seperator.Length;
			
			// Command
			Array.Copy(command, 0, packet, index, command.Length);
			index += command.Length;
			
			// Separator
			Array.Copy(Seperator, 0, packet, index, Seperator.Length);
			index += Seperator.Length;
			
			// Data
			Array.Copy(data, 0, packet, index, data.Length);
			index += data.Length;
			
			// Separator
			Array.Copy(Seperator, 0, packet, index, Seperator.Length);
			index += Seperator.Length;
			
			// End header
			Array.Copy(EndHeader, 0, packet, index, EndHeader.Length);
			index += EndHeader.Length;
			
			return packet;
		}

		public static Packet DeserializePacket(byte[] data)
		{
			// Find the start of the packet
			int start = 0;
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] == BeginHeader[0])
				{
					start = i;
					break;
				}
			}
			
			// Find the end of the packet
			int end = 0;
			for(int i = data.Length-1; i > 0; i--)
			{
				if (data[i] == EndHeader[0])
				{
					end = i;
					break;
				}
			}
			
			// Get the command
			string command = String.Empty;
			int commandEnd = end - Seperator.Length;
			
			// Loop through the array until the seperator is found or the end of the packet is reached
			for (int i = start + BeginHeader.Length + Seperator.Length; i < end; i++)
			{
				if (data[i] == Seperator[0])
				{
					commandEnd = i;
					break;
				}

				command += Encoding.ASCII.GetString(new byte[] { data[i] });
			}
			
			// Get the data
			int dataStart = commandEnd + Seperator.Length;
			int dataEnd = end - Seperator.Length;
			byte[] dataBytes = new byte[dataEnd - dataStart];
			Array.Copy(data, dataStart, dataBytes, 0, dataEnd - dataStart);

			// Create the packet
			Packet packet = new Packet();
			packet.m_Command = command;
			packet.m_Data = dataBytes;
			
			// Return the packet
			return packet;
		}
	}
}