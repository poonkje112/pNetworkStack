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

		private Command m_Command;
		private byte[] m_Data = new byte[0];

		public Command Command => m_Command;
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
			m_Data = Util.GetBytes(data);
		}

		public T GetData<T>()
		{
			return Util.ConvertBytesToObject<T>(m_Data);
		}

		public void SetCommand(Command command)
		{
			m_Command = command;
		}

		internal byte[] SerializePacket()
		{
			byte[] command = Util.GetBytes(m_Command);
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
			Command command = null;
			int commandStart = BeginHeader.Length + Separator.Length;
			int commandEnd = FindPart(data, Separator, commandStart);

			byte[] commandBytes = new byte[commandEnd - BeginHeader.Length - Separator.Length];
			Array.Copy(data, BeginHeader.Length + Separator.Length, commandBytes, 0, commandEnd - commandStart);

			command = Util.ConvertBytesToObject<Command>(commandBytes);
			
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

		private void SetCreationTime()
		{
			// Set the creation time using unix time
			m_CreationTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		}
		
		public Packet()
		{
			// Set the creation time using unix time
			m_CreationTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		}

		public Packet(Command command)
		{
			SetCreationTime();
			SetCommand(command);
		}

		public Packet(Command command, object data)
		{
			SetCreationTime();
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

		public Packet(string commandAndArgs)
		{
			SetCreationTime();
			Util.ParseCommand(commandAndArgs, (command, args) =>
			{
				m_Command = new Command(command, args);
			});
		}
		
		~Packet()
		{
			m_Data = null;
			m_SerializedPacket = null;
		}
	}
}