using pNetworkStack.Commands;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;

namespace UDPClientExample
{
	public class ClientCommands
	{
		[ClientCommand("cl_lobby_created")]
		public void OnLobbyCreated(string[] args)
		{
			// Convert args to one string
			string lobbyId = args[0];
			Debugger.OnInfo.Invoke($"Lobby was created with id: {lobbyId}");
		}
		
		[ClientCommand("cl_lobby_player_joined")]
		public void OnLobbyPlayerJoined(string[] args)
		{
			string username = args[0];
			Debugger.OnInfo.Invoke($"{username} has joined our lobby!");
		}
		
		[ClientCommand("cl_lobby_player_left")]
		public void OnLobbyPlayerLeft(string[] args)
		{
			string username = args[0];
			Debugger.OnInfo.Invoke($"{username} has left our lobby!");
		}
		
		[ClientCommand("cl_lobby_does_not_exist")]
		public void OnLobbyDoesNotExist(string[] args)
		{
			Debugger.OnInfo.Invoke("Lobby does not exist!");
		}
		
		[ClientCommand("srv_welcome")]
		public void OnWelcome(string[] args)
		{
			Debugger.OnInfo.Invoke($"We have joined the server!");
		}
		
		[ClientCommand("srv_full")]
		public void OnServerFull(string[] args)
		{
			Debugger.OnInfo.Invoke("Server is full!");
		}
		
	}
}