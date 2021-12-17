using System;
using System.Collections.Generic;
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
		
		public byte[] Data = null;

		public User UserData;

		public void SendData(Packet packet, User ignoreSender = null)
		{
			if (ignoreSender != null && ignoreSender == UserData) return;
			byte[] data = packet.SerializePacket();
			WorkClient.BeginSend(data, 0, data.Length, 0, OnSendDone, WorkClient);
		}

		void OnSendDone(IAsyncResult ar)
		{
			Socket s = (Socket) ar.AsyncState;
			s.EndSend(ar);
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
				Data = new byte[bytesToRead];
			}
			
			Array.Copy(data, Data, bytesToRead);

			return IsPacketComplete();
		}

		private bool IsPacketComplete()
		{
			byte[] endHeader = Packet.EndHeader;
			int endHeaderLength = endHeader.Length;
			int endHeaderIndex = Data.Length - endHeaderLength;
			
			if (endHeaderIndex < 0) return false;
			
			for (int i = 0; i < endHeaderLength; i++)
			{
				if (Data[endHeaderIndex + i] != endHeader[i]) return false;
			}
			
			return true;
		}

		public void ClearData()
		{
			Data = null;
		}

		public Packet PopPacket()
		{
			return !IsPacketComplete() ? null : Packet.DeserializePacket(Data);
		}
	}
}