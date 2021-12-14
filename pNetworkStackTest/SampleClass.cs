using System;

namespace pNetworkStackTest
{
	[Serializable]
	public class SampleClass
	{
		public string Name { get; set; }
		public int Age { get; set; }

		public SampleClass(string name, int age)
		{
			Name = name;
			Age = age;
		}
	}
}