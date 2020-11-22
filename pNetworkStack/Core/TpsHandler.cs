using System;
using System.Diagnostics;
using System.Threading;
using pNetworkStack.Core.Settings;

namespace pNetworkStack.Core
{
	public class TpsHandler
	{
		private static TpsHandler Instance;

		private Thread m_TickerThread;
		private bool m_Active;
		private int m_TickCount = 0;

		public Action PreUpdate, TransfromUpdate, FinalUpdate;
		
		public static TpsHandler GetHandler()
		{
			return Instance ?? new TpsHandler();
		}

		private TpsHandler()
		{
			TransfromUpdate += () => { m_TickCount++; };

			m_TickerThread = new Thread(() =>
			{
				Ticker();
			});
		}
		
		public void StartTicker()
		{
			m_Active = true;
			m_TickerThread.Start();
		}

		public void StopTicker()
		{
			m_Active = false;
			m_TickerThread.Abort();
		}
		
		private void Ticker()
		{
			Stopwatch sw = Stopwatch.StartNew();
			
			while (m_Active)
			{
				if (sw.ElapsedMilliseconds <= 1000)
				{
					if (m_TickCount < ConVars.TicksPerSecond)
					{
						PreUpdate?.Invoke();
						TransfromUpdate?.Invoke();
						FinalUpdate?.Invoke();
					}
					else
					{
						int waitTime = (int)(1000 - sw.ElapsedMilliseconds);
						
						if(waitTime >= 0)
							Thread.Sleep(waitTime);

						m_TickCount = 0;
						sw.Restart();
					}
				}
			}
		}
	}
}