using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace pNetworkStack.Core.Data
{
	public class ClientData
	{
		public const int BufferSize = 1024;
		public byte[] Buffer = new byte[BufferSize];
		public StringBuilder Builder = new StringBuilder();
		public Socket WorkClient = null;
		public IPEndPoint RemoteEndPoint = null;

		public byte[] Data = null;

		public User UserData;

		public void SendData(Packet packet, User ignoreSender = null)
		{
			if (ignoreSender != null && ignoreSender == UserData) return;
			byte[] data = packet.SerializePacket();

			if (Server.Server.GetCurrent().ConnectionType == ConnectionType.TCP)
				WorkClient.BeginSend(data, 0, data.Length, 0, OnSendDone, WorkClient);
			else
				Server.Server.GetCurrent().UdpClient.BeginSend(data, data.Length, RemoteEndPoint, OnSendDone,
					Server.Server.GetCurrent().UdpClient);
		}

		void OnSendDone(IAsyncResult ar)
		{
			if (Server.Server.GetCurrent().ConnectionType == ConnectionType.TCP)
			{
				Socket s = (Socket)ar.AsyncState;
				s.EndSend(ar);
			}
			else
			{
				UdpClient u = (UdpClient)ar.AsyncState;
				u.EndSend(ar);
			}
		}

		/// <summary>
		/// Add data to the buffer
		/// </summary>
		/// <param name="data">The received data</param>
		/// <returns>If the packet is complete</returns>
		public bool PushData(byte[] data, int bytesToRead)
		{
			if (Data == null)
			{
				Data = new byte[BufferSize];
			}

			Array.Copy(data, Data, bytesToRead);

			return IsPacketComplete();
		}

		private bool IsPacketComplete()
		{
			byte[] endHeader = Packet.EndHeader;
			int endHeaderLength = endHeader.Length;
			int endHeaderIndex = Util.FindPart(Data, endHeader, 0);

			if (endHeaderIndex < 0 || endHeaderIndex == -1) return false;
			
			// Trim the data to the end of the packet
			byte[] packetData = new byte[endHeaderIndex + endHeaderLength];
			Array.Copy(Data, packetData, packetData.Length);

			Data = packetData;

			return true;
		}

		public void ClearData()
		{
			Data = null;
			Buffer = new byte[BufferSize];
		}

		public Packet PopPacket()
		{
			return !IsPacketComplete() ? null : Packet.DeserializePacket(Data);
		}
	}
}