using System;

namespace pNetworkStack.Core
{
	internal enum LogType
	{
		Info,
		Warning,
		Error
	}

	public static class Debugger
	{
		public static Action<string> OnInfo, OnWarning, OnError;

		internal static void Log(string msg, LogType type = LogType.Info)
		{
			switch (type)
			{
				case LogType.Info:
					OnInfo?.Invoke(msg);
					break;

				case LogType.Warning:
					OnWarning?.Invoke(msg);
					break;

				case LogType.Error:
					OnError?.Invoke(msg);
					break;
			}
		}
	}
}