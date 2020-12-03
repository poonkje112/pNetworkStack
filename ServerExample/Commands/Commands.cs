using pNetworkStack.Commands;
using pNetworkStack.Core.Data;
using pNetworkStack.Server;

namespace ServerExample.Commands
{
	public class Commands
	{
		[ServerCommand("shoot")]
		public void Shoot(User sender, string[] args)
		{
			pVector shootinPos = pVector.StringToPVector(args[0]);

			if (pVector.GetDistance(sender.GetPosition(), shootinPos) > 2f) return;
			
			EntityManager.GetEntityManager().SpawnEntity(sender,shootinPos, pVector.StringToPVector(args[0]));
		}
	}
}