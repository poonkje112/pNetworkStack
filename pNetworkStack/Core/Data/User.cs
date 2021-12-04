using System;
using System.Net;

namespace pNetworkStack.Core.Data
{
	public class User
	{
		public string UUID;
		public string Username;


		public Action<pVector> OnPositionUpdated, OnEulerUpdated;
		private pVector m_Position, m_Euler;
		public string LocalEndpoint;
		
		
		public User()
		{
			m_Position = pVector.Zero();
			m_Euler = pVector.Zero();
		}
		
		public pVector GetPosition()
		{
			return m_Position;
		}

		public pVector GetEuler()
		{
			return m_Euler;
		}

		internal void UpdatePosition(pVector pos)
		{
			m_Position = pos;
			OnPositionUpdated?.Invoke(pos);
		}

		internal void UpdateEuler(pVector euler)
		{
			m_Euler = euler;
			OnEulerUpdated?.Invoke(euler);
		}
	}
}