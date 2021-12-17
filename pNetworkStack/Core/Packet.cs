using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace pNetworkStack.Core
{
	public class Packet
	{
		public static readonly byte[] BeginHeader = Encoding.ASCII.GetBytes("<Packet>");
		public static readonly byte[] EndHeader = Encoding.ASCII.GetBytes("</Packet>");
		private static readonly byte[] Separator = Encoding.ASCII.GetBytes("|");

		private byte[] m_SerializedPacket = new byte[0];
		private int m_SerializedPacketDestinationIndex;

		private string m_Command = String.Empty;
		private byte[] m_Data = new byte[0];

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
			// Escape the command to prevent injection
			m_Command = command.Replace("|", "\\|");

			// Remove the headers if they exist in the command
			m_Command = m_Command.Replace(Encoding.ASCII.GetString(BeginHeader), String.Empty);
			m_Command = m_Command.Replace(Encoding.ASCII.GetString(EndHeader), String.Empty);

			m_Command = command;
		}

		public byte[] SerializePacket()
		{
			byte[] command = Encoding.ASCII.GetBytes(m_Command);
			byte[] data = m_Data;

			command = Compress(command);
			data = Compress(data);

			m_SerializedPacket = new byte[BeginHeader.Length + EndHeader.Length + command.Length + data.Length +
			                              (Separator.Length * 3)];
			m_SerializedPacketDestinationIndex = 0;

			MergePacket(BeginHeader);
			MergePacket(command);
			MergePacket(data);
			MergePacket(EndHeader, false);

			return m_SerializedPacket;
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

		private void MergePacket(Array sourceArray, bool includeSeparator = true)
		{
			if (sourceArray == null)
				return;

			Array.Copy(sourceArray, 0, m_SerializedPacket, m_SerializedPacketDestinationIndex, sourceArray.Length);
			m_SerializedPacketDestinationIndex += sourceArray.Length;

			if (includeSeparator)
				MergePacket(Separator, false);
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
			for (int i = data.Length - 1; i > 0; i--)
			{
				if (data[i] == EndHeader[0])
				{
					end = i;
					break;
				}
			}

			// Get the command
			string command = String.Empty;
			int commandStart = +BeginHeader.Length + BeginHeader.Length;
			int commandEnd = end - Separator.Length;

			// Loop through the array until the separator is found or the end of the packet is reached
			for (int i = start + BeginHeader.Length + Separator.Length; i < end; i++)
			{
				if (data[i] == Separator[0])
				{
					commandEnd = i;
					break;
				}
			}

			byte[] commandBytes = new byte[commandEnd - BeginHeader.Length - Separator.Length];
			Array.Copy(data, start + BeginHeader.Length + Separator.Length, commandBytes, 0, commandEnd - commandStart);

			command = Encoding.ASCII.GetString(Decompress(commandBytes));

			// Get the data
			int dataStart = commandEnd + Separator.Length;
			int dataEnd = end - Separator.Length;
			byte[] dataBytes = new byte[dataEnd - dataStart];
			Array.Copy(data, dataStart, dataBytes, 0, dataEnd - dataStart);
			dataBytes = Decompress(dataBytes);

			// Create the packet
			Packet packet = new Packet
			{
				m_Command = command,
				m_Data = dataBytes
			};

			// Return the packet
			return packet;
		}

		public Packet()
		{
		}

		public Packet(string command)
		{
			m_Command = command;
		}

		public Packet(string command, object data)
		{
			SetCommand(command);
			SetData(data);
		}
	}
}