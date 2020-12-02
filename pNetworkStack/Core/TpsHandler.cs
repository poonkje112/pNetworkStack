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

		private int m_TickTrack = 0;
		public int TickCount => m_TickTrack;

		public Action PreUpdate, TransfromUpdate, FinalUpdate;

		public static TpsHandler GetHandler()
		{
			return Instance ?? new TpsHandler();
		}

		private TpsHandler()
		{
			m_TickerThread = new Thread(() => { Ticker(); });
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
				Stopwatch tickStopWatch = Stopwatch.StartNew();

				PreUpdate?.Invoke();
				TransfromUpdate?.Invoke();
				FinalUpdate?.Invoke();

				m_TickTrack += 1;

				if (tickStopWatch.ElapsedMilliseconds < (1000 / ConVars.TicksPerSecond))
				{
					int tickSleepTime = (int) ((1000 / ConVars.TicksPerSecond) - tickStopWatch.ElapsedMilliseconds);

					if (tickSleepTime >= 0)
					{
						Thread.Sleep(tickSleepTime);
					}
				}
				else
				{
					Debugger.Log("Could not keep up!", LogType.Warning);
				}

				tickStopWatch.Reset();
			}
		}
	}
}