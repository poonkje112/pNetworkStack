using System;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;
using pNetworkStack.Server;

namespace ServerExample
{
	public enum EntityType
	{
		bullet
	}
	public class Entity : User, IDisposable
	{
		private EntityType m_Type;
		public EntityType Type => m_Type;

		public pVector Velocity, Acceleration;
		public pVector Position;
		public Entity(EntityType type, pVector spawnPosition, string uid)
		{
			UUID = uid;
			Username = uid;
			m_Type = type;
			
			Velocity = pVector.Zero();
			Acceleration = pVector.Zero();
			Position = spawnPosition;

			Server.GetCurrent().TransformUpdate += Update;
		}

		public void ApplyForce(pVector force)
		{
			Acceleration = force;
		}

		public void Update()
		{
			Velocity += Acceleration;
			Position += Velocity;
			Acceleration = pVector.Zero();
		}

		public void SendUpdate()
		{
			Server.GetCurrent().SendRPC(null, $"e_position {UUID} {Position}");
		}

		public void Dispose()
		{
			TpsHandler.GetHandler().TransfromUpdate -= Update;
		}
	}
}