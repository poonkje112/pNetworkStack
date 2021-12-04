using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using pNetworkStack.Commands;
using pNetworkStack.Core.Data;

namespace pNetworkStack.Core
{
	internal class DataProcessor
	{
		internal static DataProcessor Instance = GetInstance();
		private static DataProcessor m_Instance;


		private static DataProcessor GetInstance()
		{
			return m_Instance ?? (m_Instance = new DataProcessor());
		}


		public void ReceiveCallback()
		{
			
		}
	}
}