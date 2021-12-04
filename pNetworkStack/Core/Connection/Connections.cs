using System.Collections.Generic;
using System.Net;
using pNetworkStack.Core.Data;

namespace pNetworkStack.Core
{
	internal class Connections
	{
		internal Dictionary<string, string> EndPointIds = new Dictionary<string, string>();
		internal Dictionary<string, ClientData> Clients = new Dictionary<string, ClientData>();

		internal static Connections Instance = GetInstance();
		private static Connections m_Instance;


		private static Connections GetInstance()
		{
			return m_Instance ?? (m_Instance = new Connections());
		}

		public void AddClient(string uid, ClientData data)
		{
			EndPointIds.Add(data.EndPoint, uid);
			Clients.Add(uid, data);
		}

		public void RemoveClient(string endPoint)
		{
			var uid = EndPointIds[endPoint];
			EndPointIds.Remove(endPoint);
			Clients.Remove(uid);
		}

		public ClientData GetClient(string uid)
		{
			return Clients.ContainsKey(uid) ? Clients[uid] : null;
		}

		public ClientData GetClientId(string endPoint)
		{
			return Clients.ContainsKey(EndPointIds[endPoint]) ? Clients[EndPointIds[endPoint]] : null;
		}

		public bool CheckIfClientExists(string endPoint)
		{
			return EndPointIds.ContainsKey(endPoint);
		}
	}
}