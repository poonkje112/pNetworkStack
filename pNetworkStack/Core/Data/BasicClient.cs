using System;
using pNetworkStack.client;

namespace pNetworkStack.Core.Data
{
	[System.Serializable]
	public struct BaseData
	{
		public string DisplayName;
		public pVector Position;
		public pVector EulerAngle;
		public string UID;
	}
	
	public struct BasicClient
	{
		public BaseData Base;
		
		public Action<pVector> OnPositionUpdate;
		public Action<pVector> OnEulerAngleUpdate;
		public Action OnRemovePlayer;

		public void UpdatePosition(pVector pos)
		{
			Base.Position = pos;
		}

		public void UpdateEuler(pVector euler)
		{
			Base.EulerAngle = euler;
		}
	}
}