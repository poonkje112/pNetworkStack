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

			Packet p = new Packet($"pl_init {JsonConvert.SerializeObject(Client.GetCurrent().OurUser)}");
			byte[] data = p.SerializePacket();
			Debugger.Log(data.Length.ToString());
			
			
			Client.GetCurrent().Send(p);
		}

		[ClientCommand("pl_add")]
		public void AddPlayer(string[] args)
		{
			User user = JsonConvert.DeserializeObject<User>(Util.Join(' ', args));

			// Checking if the user already exists for some weird reason
			if (Client.GetCurrent().ConnectedUsers.ContainsKey(user.UUID)) return;

			Client.GetCurrent().ConnectedUsers.Add(user.UUID, user);
			Client.OnUserJoined?.Invoke(user);
		}

		[ClientCommand("pl_add_bulk")]
		public void AddPlayerBulk(string[] args)
		{
			User[] users = JsonConvert.DeserializeObject<User[]>(args[0]);

			if (!Client.GetCurrent().m_IsReady)
				Client.GetCurrent().m_IsReady = true;

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
			
			// Check if the user exists in our local list
			if (!Client.GetCurrent().ConnectedUsers.ContainsKey(uid)) return;
			
			Client.GetCurrent().ConnectedUsers.Remove(uid);
			Client.OnUserLeft?.Invoke(uid);
		}

		[ClientCommand("pl_update_position")]
		public void UpdatePlayerPosition(string[] args)
		{
			string uid = args[0];

			// Check if the user exists in our local list if not then we are out of sync
			if (!Client.GetCurrent().ConnectedUsers.ContainsKey(uid))
			{
				Client.GetCurrent().Send(new Packet("sync_list"));
				return;
			}

			pVector pos = pVector.StringToPVector(args[1]);
			Client.GetCurrent().ConnectedUsers[uid].UpdatePosition(pos);
		}

		[ClientCommand("pl_update_euler")]
		public void UpdatePlayerEuler(string[] args)
		{
			string uid = args[0];

			// Check if the user exists in our local list if not then we are out of sync
			if (!Client.GetCurrent().ConnectedUsers.ContainsKey(uid))
			{
				Client.GetCurrent().Send(new Packet("sync_list"));
				return;
			}

			pVector euler = pVector.StringToPVector(args[1]);
			Client.GetCurrent().ConnectedUsers[uid].UpdateEuler(euler);
		}
		
		[ClientCommand("pl_update_transform")]
		public void UpdateTransform(string[] args)
		{
			string uid = args[0];

			// Check if the user exists in our local list if not then we are out of sync
			if (!Client.GetCurrent().ConnectedUsers.ContainsKey(uid))
			{
				Client.GetCurrent().Send(new Packet("sync_list"));
				return;
			}

			pVector position = pVector.StringToPVector(args[1]);
			pVector euler = pVector.StringToPVector(args[2]);
			Client.GetCurrent().ConnectedUsers[uid].UpdatePosition(position);
			Client.GetCurrent().ConnectedUsers[uid].UpdateEuler(euler);
		}

		[ClientCommand("sync_list")]
		public void SyncPlayerList(string[] args)
		{
			User[] users = JsonConvert.DeserializeObject<User[]>(args[0]);

			foreach (User user in users)
			{
				// Checks if we already have an instance of the user
				if (Client.GetCurrent().ConnectedUsers.ContainsKey(user.UUID)) continue;

				// Adds the missing user
				Client.GetCurrent().ConnectedUsers.Add(user.UUID, user);
				Client.OnUserJoined?.Invoke(user);
			}
		}

	#endregion
	}
}