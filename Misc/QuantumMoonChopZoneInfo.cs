using Newtonsoft.Json;
using System.ComponentModel;

namespace Stowaway
{
	[JsonObject]
	public class QuantumMoonChopZoneInfo
	{
		[DefaultValue(73)]
		public float radius = 73;
	}
}