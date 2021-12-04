using System.Text;
using pNetworkStack.Commands;
using pNetworkStack.Core;
using pNetworkStack.Core.Settings;

namespace pNetworkStack.Server.Commands
{
	public class UdpServerCommands
	{
		Connections m_Connections = Connections.Instance;

		[UdpServerCommand("pl_join")]
		public void Dummy(string endPoint, string[] args)
		{
			Debugger.Log("Received pl_join command from: " + endPoint);

			string message = m_Connections.Clients.Count >= ConVars.max_players ? "srv_full" : "srv_welcome";
			
			byte[] data = Encoding.ASCII.GetBytes(message);
			
			UDPServer.GetCurrent().Send(data, endPoint);
		}
	}
}