using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Stowaway
{
	[JsonObject]
	public class QuantumMoonWaterColumnInfo
	{
		[DefaultValue(5)]
		public float size = 5;

		[DefaultValue(500)]
		public float height = 500;

		[DefaultValue(70)]
		public float flowSpeed = 70;

		public MColor tint;

		[DefaultValue(false)]
		public bool debug = false;
	}
}