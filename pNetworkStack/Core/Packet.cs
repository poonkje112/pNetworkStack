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
		private int m_CreationTime;

		public static readonly byte[] BeginHeader = Encoding.ASCII.GetBytes("<Packet>");
		public static readonly byte[] EndHeader = Encoding.ASCII.GetBytes("</Packet>");
		private static readonly byte[] Separator = Encoding.ASCII.GetBytes("|");

		private byte[] m_SerializedPacket = new byte[0];
		private int m_SerializedPacketDestinationIndex;

		private string m_Command = String.Empty;
		private byte[] m_Data = new byte[0];

		public string Command => m_Command;
		public byte[] PacketBytes => SerializePacket();
		public int CreationTime => m_CreationTime;

		private void MergePacket(Array sourceArray, bool includeSeparator = true)
		{
			if (sourceArray == null)
				return;

			Array.Copy(sourceArray, 0, m_SerializedPacket, m_SerializedPacketDestinationIndex, sourceArray.Length);
			m_SerializedPacketDestinationIndex += sourceArray.Length;

			if (includeSeparator)
				MergePacket(Separator, false);
		}

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

		internal byte[] SerializePacket()
		{
			byte[] command = Encoding.ASCII.GetBytes(m_Command);
			byte[] data = m_Data;
			byte[] creationTime = BitConverter.GetBytes(m_CreationTime);

			command = Util.Compress(command);
			data = Util.Compress(data);

			m_SerializedPacket = new byte[BeginHeader.Length + EndHeader.Length + command.Length + data.Length +
			                              creationTime.Length + (Separator.Length * 4)];
			m_SerializedPacketDestinationIndex = 0;

			MergePacket(BeginHeader);
			MergePacket(command);
			MergePacket(data);
			MergePacket(creationTime);
			MergePacket(EndHeader, false);

			return m_SerializedPacket;
		}

		/// <summary>
		/// This finds the exact match of bytes in the source and returns it starting index
		/// </summary>
		/// <param name="source">The source you want to find the part in</param>
		/// <param name="part">The part you want to find</param>
		/// <param name="startIndex">The start</param>
		/// <returns>The start index of the part position in the source</returns>
		private static int FindPart(byte[] source, byte[] part, int startIndex)
		{
			int index = -1;
			bool found = false;
			
			for (int i = startIndex; i < source.Length; i++)
			{
				if(i + part.Length > source.Length)
					break;
				
				for(int j = 0; j < part.Length; j++)
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

		internal static Packet DeserializePacket(byte[] data)
		{
			// Find the end of the packet
			int endHeaderBeginning = data.Length - EndHeader.Length;

			// Get the command
			string command = String.Empty;
			int commandStart = BeginHeader.Length + Separator.Length;
			int commandEnd = FindPart(data, Separator, commandStart);

			byte[] commandBytes = new byte[commandEnd - BeginHeader.Length - Separator.Length];
			Array.Copy(data, BeginHeader.Length + Separator.Length, commandBytes, 0, commandEnd - commandStart);

			command = Encoding.ASCII.GetString(Util.Decompress(commandBytes));
			
			// Get the data
			int dataStart = commandEnd + Separator.Length;
			int dataEnd = FindPart(data, Separator, dataStart);

			byte[] dataBytes = new byte[dataEnd - dataStart];
			Array.Copy(data, dataStart, dataBytes, 0, dataEnd - dataStart);
			dataBytes = Util.Decompress(dataBytes);
			
			// Get the creation time
			int creationTimeStart = dataEnd + Separator.Length;
			int creationTimeEnd = endHeaderBeginning - Separator.Length;
			
			byte[] creationTimeBytes = new byte[creationTimeEnd - creationTimeStart];
			Array.Copy(data, creationTimeStart, creationTimeBytes, 0, creationTimeEnd - creationTimeStart);
			
			int creationTime = BitConverter.ToInt32(creationTimeBytes, 0);

			// Create the packet
			Packet packet = new Packet
			{
				m_Command = command,
				m_Data = dataBytes,
				m_CreationTime = creationTime
			};

			// Return the packet
			return packet;
		}

		public Packet()
		{
			// Set the creation time using unix time
			m_CreationTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		}

		public Packet(string command)
		{
			// Set the creation time using unix time
			m_CreationTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			SetCommand(command);
		}

		public Packet(string command, object data)
		{
			// Set the creation time using unix time
			m_CreationTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			SetCommand(command);
			SetData(data);
		}

		public Packet(byte[] rawPacket)
		{
			Packet temp = DeserializePacket(rawPacket);
			m_Command = temp.m_Command;
			m_Data = temp.m_Data;
			m_CreationTime = temp.m_CreationTime;
		}

		~Packet()
		{
			m_Data = null;
			m_SerializedPacket = null;
		}
	}
}