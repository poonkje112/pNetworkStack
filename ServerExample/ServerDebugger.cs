using System;

namespace ServerExample
{
	public enum LogType
	{
		Info,
		Warning,
		Error
	}
	public class ServerDebugger
	{
		public static Action<string> OnInfo, OnWarning, OnError;

		public static void Log(string msg, LogType type = LogType.Info)
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