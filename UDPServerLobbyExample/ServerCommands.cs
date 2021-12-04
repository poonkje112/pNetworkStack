using pNetworkStack.Commands;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;
using pNetworkStack.Server;

namespace UDPServerLobbyExample
{
	public class ServerCommands
	{
		[TcpServerCommand("cl_join_lobby")]
		public void JoinLobby(User sender, string[] args)
		{
			string lobbyId = args[0];
			LobbyManager lobbyManager = LobbyManager.GetLobbyManager();
			
			lobbyManager.JoinLobby(sender, lobbyId);
		}

		[TcpServerCommand("cl_leave_lobby")]
		public void LeaveLobby(User sender, string[] args)
		{
		}
		
		[TcpServerCommand("cl_start_game")]
		public void StartGame(User sender, string[] args)
		{
		}
		
		[TcpServerCommand("cl_player_count")]
		public void PlayerCount(User sender, string[] args)
		{
			
		}

		[TcpServerCommand("cl_create_lobby")]
		public void CreateLobby(User sender, string[] args)
		{
			LobbyManager lobbyManager = LobbyManager.GetLobbyManager();
			Lobby lobby = lobbyManager.CreateLobby(sender);
			TCPServer.GetCurrent().Send(sender, $"cl_lobby_created {lobby.LobbyId}");
			Debugger.OnInfo.Invoke($"Created lobby with id: {lobby.LobbyId}");
		}
	}
}