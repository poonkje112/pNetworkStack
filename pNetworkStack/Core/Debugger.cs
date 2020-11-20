using System;

namespace pNetworkStack.Core
{
	internal enum LogType
	{
		info,
		warning,
		error
	}

	public class Debugger
	{
		public static Action<string> OnInfo, OnWarning, OnError;

		internal static void Log(string msg, LogType type = LogType.info)
		{
			switch (type)
			{
				case LogType.info:
					OnInfo?.Invoke(msg);
					break;

				case LogType.warning:
					OnWarning?.Invoke(msg);
					break;

				case LogType.error:
					OnError?.Invoke(msg);
					break;
			}
		}
	}
}