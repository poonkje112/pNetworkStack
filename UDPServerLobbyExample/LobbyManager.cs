using System;
using System.Collections.Generic;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;
using pNetworkStack.Server;

namespace UDPServerLobbyExample
{
	public class LobbyManager
	{
		private static LobbyManager Instance;

		private Dictionary<string, Lobby> m_Lobbies = new Dictionary<string, Lobby>();
		private Dictionary<User, Lobby> m_LobbyMasters = new Dictionary<User, Lobby>();

		private Dictionary<User, string> m_Users = new Dictionary<User, string>();

		public static LobbyManager GetLobbyManager()
		{
			return Instance ??= new LobbyManager();
		}

		private LobbyManager()
		{
		}

		public Lobby CreateLobby(User host)
		{
			string lobbyId = GenerateUniqueLobbyId();
			LobbySettings settings = new LobbySettings();
			Player creator = new Player()
			{
				PlayerData = host
			};

			settings.Host = creator;
			settings.MaxPlayers = 4;
			settings.Privacy = PrivacySettings.Public;

			Lobby lobby = new Lobby() { LobbyId = lobbyId, Settings = settings };
			lobby.Players = new List<Player>() { settings.Host };
			m_Lobbies.Add(lobbyId, lobby);
			m_LobbyMasters.Add(host, lobby);
			
			m_Users.Add(host, lobbyId);

			return lobby;
		}

		private string GenerateUniqueLobbyId()
		{
			// Generate a unique Lobby ID that does not exist in the dictionary
			string lobbyId = Guid.NewGuid().ToString();
			while (m_Lobbies.ContainsKey(lobbyId))
			{
				lobbyId = Guid.NewGuid().ToString();
			}

			return lobbyId;
		}

		public void JoinLobby(User user, string lobbyId)
		{
			if (m_Lobbies.ContainsKey(lobbyId))
			{
				Lobby lobby = m_Lobbies[lobbyId];
				Player joinedPlayer = new Player() { PlayerData = user };
				lobby.Players.Add(joinedPlayer);
				Debugger.OnInfo.Invoke("User has joined our lobby!");
				
				foreach(Player player in lobby.Players)
				{
					if(player.Equals(joinedPlayer)) continue;
					
					TCPServer.GetCurrent().Send(player.PlayerData, $"cl_lobby_player_joined {joinedPlayer.PlayerData.Username}");
				}
				
				m_Users.Add(user, lobbyId);
			}
			else
			{
				// Send lobby does not exist message
				TCPServer.GetCurrent().Send(user, "cl_lobby_does_not_exist");
			}
		}
		
		public void LeaveLobby(User user)
		{
			if (m_Users.ContainsKey(user))
			{
				string lobbyId = m_Users[user];
				Lobby lobby = m_Lobbies[lobbyId];
				
				Player leaver = lobby.Players.Find(p => p.PlayerData == user);
				
				lobby.Players.Remove(leaver);
				m_Lobbies[lobbyId] = lobby;
				
				m_Users.Remove(user);
				
				foreach(Player player in lobby.Players)
				{
					User receiver = player.PlayerData;
					TCPServer.GetCurrent().Send(receiver, $"cl_lobby_player_left {leaver.PlayerData.Username}");
				}
			}
		}

		public void TryUpdateHost(User user)
		{
			if (m_LobbyMasters.ContainsKey(user))
			{
				Lobby lobby = m_LobbyMasters[user];
				Player oldHost = lobby.Settings.Host;

				if (lobby.Players.Count > 1)
				{
					lobby.Players.Remove(lobby.Settings.Host);

					lobby.Settings.Host = lobby.Players[0];
					m_LobbyMasters.Add(lobby.Settings.Host.PlayerData, lobby);

					// Update the lobby
					m_Lobbies[lobby.LobbyId] = lobby;

					// Send message to all players that the host has changed
					foreach (Player player in lobby.Players)
					{
						if (player.Equals(oldHost)) continue;
						
						// Send message to player
					}
					
					Debugger.OnInfo.Invoke(
						$"{lobby.Settings.Host.PlayerData.LocalEndpoint} is now the host of the lobby.");
				}
				else
				{
					DestroyLobby(m_Lobbies[m_LobbyMasters[user].LobbyId].LobbyId);
					Debugger.OnInfo.Invoke($"No users left in lobby {lobby.LobbyId}\nLobby Destroyed.");
				}

				m_LobbyMasters.Remove(user);
				m_Users.Remove(user);
			}
		}

		private void DestroyLobby(string lobbyId)
		{
			m_Lobbies.Remove(lobbyId);
		}
	}
}