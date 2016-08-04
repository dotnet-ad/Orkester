using System;
namespace Orkester
{
	public class OperationNotFoundException : InvalidOperationException
	{
		public OperationNotFoundException(string name) : base($"The operation {name} doesn't seem registered.")
		{
		}
	}
}

