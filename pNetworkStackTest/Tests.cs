using NUnit.Framework;
using pNetworkStack.Core;

namespace pNetworkStackTest
{
	public class Tests
	{
		private static readonly object[] data =
		{
			new object[] {new SampleClass("Bob", 35)},
			new object[] {new SampleClass("Alice", 25)},
			new object[] {new SampleClass("Charlie", 45)},
			new object[] {new SampleClass("Dave", 55)},
			new object[] {new SampleClass("Eve", 65)},
			new object[] {new SampleClass("Frank", 75)},
			new object[] {new SampleClass("George", 85)}
		};
		
		[TestCaseSource(nameof(data))]
		public void TestPacket(SampleClass a)
		{
			Packet packet = new Packet();
			packet.SetCommand("Hello, World!");
			packet.SetData(a);
			byte[] data = packet.SerializePacket();
			
			Packet result = Packet.DeserializePacket(data);
			SampleClass resultData = result.GetData<SampleClass>();

			Assert.IsFalse(a.Equals(resultData));
		}
	}
}