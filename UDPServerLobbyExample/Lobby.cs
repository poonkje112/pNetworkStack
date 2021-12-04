using System.Collections.Generic;

namespace UDPServerLobbyExample
{
	public struct Lobby
	{
		public string LobbyId;
		public LobbySettings Settings;
		public List<Player> Players;
	}
}