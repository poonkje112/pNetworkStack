using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using pNetworkStack.Commands;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;

namespace pNetworkStack.client.Commands
{
	public class ClientCommands
	{
	#region Client Commands

		[ClientCommand("say")]
		public void Say(string[] args)
		{
			string uid = args[0];

			User sender = Client.GetCurrent().ConnectedUsers[uid];
			
			List<string> rawMessage = args.ToList();
			rawMessage.RemoveAt(0);

			Client.OnMessageReceived?.Invoke(sender, Util.Join(' ', rawMessage.ToArray()));
		}

	#endregion

	#region Player Commands

		[ClientCommand("pl_init")]
		public void InitPlayer(string[] args)
		{
			string uid = args[0];
			Client.GetCurrent().OurUser.UUID = uid;
				
			Client.GetCurrent().Send($"pl_init {JsonConvert.SerializeObject(Client.GetCurrent().OurUser)}");
		}
		
		[ClientCommand("pl_add")]
		public void AddPlayer(string[] args)
		{
			User user = JsonConvert.DeserializeObject<User>(Util.Join(' ', args));
			Client.GetCurrent().ConnectedUsers.Add(user.UUID, user);
			Client.OnUserJoined?.Invoke(user);
		}

		[ClientCommand("pl_add_bulk")]
		public void AddPlayerBulk(string[] args)
		{
			User[] users = JsonConvert.DeserializeObject<User[]>(args[0]);

			foreach (User userData in users)
			{
				Client.GetCurrent().ConnectedUsers.Add(userData.UUID, userData);
				Client.OnUserJoined?.Invoke(userData);
			}
		}

		[ClientCommand("pl_remove")]
		public void RemovePlayer(string[] args)
		{
			string uid = args[0];
			Client.GetCurrent().ConnectedUsers.Remove(uid);
			Client.OnUserLeft?.Invoke(uid);
		}

		[ClientCommand("pl_update_position")]
		public void UpdatePlayerPosition(string[] args)
		{
			string uid = args[0];
			pVector pos = pVector.StringToPVector(args[1]);

			Client.GetCurrent().ConnectedUsers[uid].UpdatePosition(pos);
		}

		[ClientCommand("pl_update_euler")]
		public void UpdatePlayerEuler(string[] args)
		{
			string uid = args[0];
			pVector euler = pVector.StringToPVector(args[1]);

			Client.GetCurrent().ConnectedUsers[uid].UpdateEuler(euler);
		}

	#endregion
	}
}