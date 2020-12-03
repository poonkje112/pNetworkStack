using System;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using pNetworkStack.Core;

namespace ServerExample.Physics
{
	public class PhysicsEngine : IDisposable
	{
		public Simulation Simulation { get; protected set; }

		public BufferPool BufferPool { get; protected set; }

		public SimpleThreadDispatcher SimpleThreadDispatcher;
		public void InitializePhysicsEngine()
		{
			ServerDebugger.Log("Starting physics engine...");
			
			BufferPool = new BufferPool();
			
			int threadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
			SimpleThreadDispatcher = new SimpleThreadDispatcher(threadCount);

			Simulation = Simulation.Create(BufferPool, null, null, new PositionFirstTimestepper());
			
			Simulation.Statics.Add(new StaticDescription(new Vector3(0, 0, 0),
				new CollidableDescription(Simulation.Shapes.Add(new Box(200, 1, 200)), 0.1f)));
			
			TpsHandler.GetHandler().TransfromUpdate += Update;
			
			ServerDebugger.Log("Physics engine started!");
		}

		public void Update()
		{
			Simulation.Timestep(1 / 60f, SimpleThreadDispatcher);
		}
		
		public void Dispose()
		{
			Simulation.Dispose();
			BufferPool.Clear();
			SimpleThreadDispatcher.Dispose();
		}
	}
}