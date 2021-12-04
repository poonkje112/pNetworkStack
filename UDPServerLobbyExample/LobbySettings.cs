namespace UDPServerLobbyExample
{
	public enum PrivacySettings
	{
		Public,
		InviteOnly,
		FriendsOnly
	}

	public struct LobbySettings
	{
		public Player Host;
		public int MaxPlayers;
		public PrivacySettings Privacy;
	}
}