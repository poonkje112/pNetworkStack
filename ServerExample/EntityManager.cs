using System;
using System.Collections.Generic;
using pNetworkStack.Core;
using pNetworkStack.Core.Data;
using pNetworkStack.Server;

namespace ServerExample
{
	public class EntityManager
	{
		private static EntityManager Instance;

		private Dictionary<string, Entity> m_Entities = new Dictionary<string, Entity>();
		public Action SendEntityUpdate;
		public static EntityManager GetEntityManager()
		{
			if(Instance == null) Instance = new EntityManager();

			return Instance;
		}
		
		private EntityManager()
		{
			Server.GetCurrent().LateUpdate += UpdateEntities;
		}
		
		public void SpawnEntity(User owner, pVector spawnPosition, pVector spawnRotation)
		{
			Random rand = new Random();
			string entityId = String.Empty;

			while (string.IsNullOrEmpty(entityId) || m_Entities.ContainsKey(entityId))
			{
				entityId = rand.Next(1, 99999999).ToString();
			}

			Entity targetEntity = new Entity(EntityType.bullet, spawnPosition, entityId);
			
			Server.GetCurrent().SendRPC(null, $"e_spawnentity {entityId} {(int)targetEntity.Type}");
			
			SendEntityUpdate += targetEntity.SendUpdate;
			targetEntity.ApplyForce(pVector.Right() * 0.1f);
			
			m_Entities.Add(entityId, targetEntity);
		}

		public void DestroyEntity(User owner, string id)
		{
			Entity target = m_Entities[id];
			target.Dispose();

			m_Entities.Remove(id);
		}

		public void UpdateEntities()
		{
			SendEntityUpdate?.Invoke();
		}
	}
}