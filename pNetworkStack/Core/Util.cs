namespace pNetworkStack.Core
{
	public static class Util
	{
		public static string Join(char seperator, string[] data)
		{
			string output = "";
			foreach (string s in data)
			{
				output += s;
				output += seperator;
			}

			output = output.Remove(output.Length - 1);
			
			return output;
		}
	}
}