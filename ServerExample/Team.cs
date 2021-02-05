using System;
using pNetworkStack.Core.Data;

namespace ServerExample
{
	public class Team
	{
		public User Player;
		public int minX, maxX, Y, minZ, maxZ;

		private Random m_Rand = new Random();

		
		public pVector Respawn()
		{
			return new pVector(m_Rand.Next(minX, maxX), Y, m_Rand.Next(minZ, maxZ));
		}
	}
}